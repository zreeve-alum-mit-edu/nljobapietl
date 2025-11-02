using System.Globalization;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class GeocodeStage
{
    private readonly string _csvPath;
    private Dictionary<string, (decimal lat, decimal lon)>? _cityLookup;

    public GeocodeStage(string basePath)
    {
        _csvPath = Path.Combine(basePath, "..", "us_cities.csv");
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== GEOCODE STAGE ===");

        // Load CSV into memory
        Console.WriteLine("Loading US cities geocoding data...");
        await LoadCityData();
        Console.WriteLine($"  Loaded {_cityLookup!.Count} city records");

        using var db = JobContext.Create();

        // Count total jobs needing geocoding
        var totalCount = await db.Jobs
            .Where(j => j.Status == "location_classified" &&
                        j.Latitude == null)
            .CountAsync();

        if (totalCount == 0)
        {
            Console.WriteLine("No jobs need geocoding.");
        }
        else
        {

        Console.WriteLine($"Found {totalCount} jobs needing geocoding");

        var successCount = 0;
        var notFoundCount = 0;
        var invalidCount = 0;
        var batchSize = 25000;
        var processedCount = 0;

        // Process in batches by re-querying
        while (true)
        {
            var batch = await db.Jobs
                .Where(j => j.Status == "location_classified" &&
                            j.Latitude == null)
                .Take(batchSize)
                .ToListAsync();

            if (batch.Count == 0)
                break;

            foreach (var job in batch)
            {
                // Check if country is not US - mark as invalid
                if (job.GeneratedCountry != null && job.GeneratedCountry.ToUpperInvariant() != "US")
                {
                    job.IsValid = false;
                    job.Status = "invalid";
                    invalidCount++;
                }
                // Only geocode US jobs
                else if (job.GeneratedCity != null && job.GeneratedState != null)
                {
                    var key = $"{job.GeneratedCity},{job.GeneratedState}".ToLowerInvariant();

                    if (_cityLookup.TryGetValue(key, out var coords))
                    {
                        job.Latitude = coords.lat;
                        job.Longitude = coords.lon;
                        job.Status = "geocoded";
                        successCount++;
                    }
                    else
                    {
                        // City not found - still mark as geocoded but leave lat/lon null
                        job.Status = "geocoded";
                        notFoundCount++;
                    }
                }
                else
                {
                    // Missing city/state - still mark as geocoded (US job but incomplete location data)
                    job.Status = "geocoded";
                    notFoundCount++;
                }

                processedCount++;
            }

            // Save batch and clear tracker
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            Console.WriteLine($"  Processed {processedCount}/{totalCount} jobs...");
        }

            Console.WriteLine($"\n  Successfully geocoded: {successCount}");
            Console.WriteLine($"  City not found: {notFoundCount}");
            Console.WriteLine($"  Invalid (non-US): {invalidCount}");
            Console.WriteLine($"  Total processed: {processedCount}");
        }

        // Update any remaining jobs with status 'location_classified' to 'geocoded'
        // (these are jobs that already had coordinates from a previous run)
        var skippedCount = await db.Jobs
            .Where(j => j.Status == "location_classified")
            .ExecuteUpdateAsync(setter => setter.SetProperty(j => j.Status, "geocoded"));

        if (skippedCount > 0)
        {
            Console.WriteLine($"  Updated {skippedCount} job(s) that already had coordinates to 'geocoded'");
        }

        Console.WriteLine("\n=== Geocoding Complete ===");

        return true;
    }

    private async Task LoadCityData()
    {
        _cityLookup = new Dictionary<string, (decimal lat, decimal lon)>();

        if (!System.IO.File.Exists(_csvPath))
        {
            throw new FileNotFoundException($"US cities CSV not found at {_csvPath}");
        }

        var lines = await System.IO.File.ReadAllLinesAsync(_csvPath);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = ParseCsvLine(lines[i]);
            if (parts.Length >= 4)
            {
                var city = parts[0];
                var state = parts[1];

                if (decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) &&
                    decimal.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
                {
                    var key = $"{city},{state}".ToLowerInvariant();

                    // If duplicate, keep the first one (or you could average them)
                    if (!_cityLookup.ContainsKey(key))
                    {
                        _cityLookup[key] = (lat, lon);
                    }
                }
            }
        }
    }

    private string[] ParseCsvLine(string line)
    {
        // Simple CSV parser (handles quoted fields)
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result.ToArray();
    }
}

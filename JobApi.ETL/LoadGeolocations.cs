using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.Etl;

public class LoadGeolocations
{
    public static async Task Main(string[] args)
    {
        // Load environment variables
        var envPath = Path.Combine("..", ".env");
        if (System.IO.File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
        }

        using var db = JobContext.Create();

        // Apply pending migrations
        await db.Database.MigrateAsync();

        Console.WriteLine("Checking if geolocations table is empty...");
        var count = await db.Geolocations.CountAsync();

        if (count > 0)
        {
            Console.WriteLine($"Table already contains {count} records. Skipping load.");
            return;
        }

        Console.WriteLine("Loading geolocations from CSV...");

        var csvPath = Environment.GetEnvironmentVariable("US_CITIES_CSV_PATH")
            ?? Path.Combine("..", "us_cities.csv");

        if (!System.IO.File.Exists(csvPath))
        {
            Console.WriteLine($"CSV file not found at {csvPath}");
            return;
        }

        var lines = await System.IO.File.ReadAllLinesAsync(csvPath);
        Console.WriteLine($"Found {lines.Length} lines in CSV");

        var geolocations = new List<Geolocation>();

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = ParseCsvLine(lines[i]);
            if (parts.Length >= 4)
            {
                var city = parts[0];
                var state = parts[1];

                if (decimal.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                    decimal.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                {
                    geolocations.Add(new Geolocation
                    {
                        Id = Guid.NewGuid(),
                        City = city,
                        State = state,
                        Country = "US",
                        Latitude = lat,
                        Longitude = lon,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (i % 1000 == 0)
            {
                Console.WriteLine($"Processed {i} lines...");
            }
        }

        Console.WriteLine($"Inserting {geolocations.Count} records into database...");

        await db.Geolocations.AddRangeAsync(geolocations);
        await db.SaveChangesAsync();

        Console.WriteLine("Done!");
    }

    private static string[] ParseCsvLine(string line)
    {
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

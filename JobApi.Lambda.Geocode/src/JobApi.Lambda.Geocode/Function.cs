using System.Diagnostics;
using System.Globalization;
using Amazon.Lambda.Core;
using JobApi.Common;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.Geocode;

public class Function
{
    private const int BatchSize = 5000;

    public async Task FunctionHandler(ILambdaContext context)
    {
        var totalStopwatch = Stopwatch.StartNew();

        context.Logger.LogInformation("=== Geocode Lambda Started ===");

        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        // Step 1: Check if there are any jobs to process (before loading CSV)
        context.Logger.LogInformation("Checking for jobs needing geocoding...");

        await using var countCmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs
            WHERE status = 'location_classified' AND latitude IS NULL", conn);

        var totalCount = (long)(await countCmd.ExecuteScalarAsync() ?? 0L);

        if (totalCount == 0)
        {
            context.Logger.LogInformation("No jobs need geocoding. Exiting.");
            context.Logger.LogInformation("=== Geocode Lambda Complete ===");
            return;
        }

        context.Logger.LogInformation($"Found {totalCount} jobs needing geocoding");

        // Step 2: Load CSV into memory
        context.Logger.LogInformation("Loading US cities geocoding data...");
        var csvStopwatch = Stopwatch.StartNew();
        var cityLookup = await LoadCityDataAsync(context);
        csvStopwatch.Stop();
        context.Logger.LogInformation($"Loaded {cityLookup.Count} city records in {csvStopwatch.ElapsedMilliseconds}ms");

        // Step 3: Process jobs in batches
        var successCount = 0;
        var notFoundCount = 0;
        var invalidCount = 0;
        var processedCount = 0;

        while (true)
        {
            var batchStopwatch = Stopwatch.StartNew();

            // Fetch batch
            var jobs = new List<(Guid id, string? city, string? state, string? country)>();

            await using (var cmd = new NpgsqlCommand(@"
                SELECT id, generated_city, generated_state, generated_country
                FROM jobs
                WHERE status = 'location_classified' AND latitude IS NULL
                LIMIT @limit", conn))
            {
                cmd.Parameters.AddWithValue("limit", BatchSize);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    jobs.Add((
                        reader.GetGuid(0),
                        reader.IsDBNull(1) ? null : reader.GetString(1),
                        reader.IsDBNull(2) ? null : reader.GetString(2),
                        reader.IsDBNull(3) ? null : reader.GetString(3)
                    ));
                }
            }

            if (jobs.Count == 0)
                break;

            // Process batch
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var geocodedIds = new List<Guid>();
                var geocodedWithCoordsIds = new List<Guid>();
                var geocodedLats = new List<decimal>();
                var geocodedLons = new List<decimal>();
                var invalidIds = new List<Guid>();

                foreach (var job in jobs)
                {
                    // Check if country is not US - mark as invalid
                    if (job.country != null && !job.country.Equals("US", StringComparison.OrdinalIgnoreCase))
                    {
                        invalidIds.Add(job.id);
                        invalidCount++;
                    }
                    // Only geocode US jobs
                    else if (job.city != null && job.state != null)
                    {
                        var key = $"{job.city},{job.state}".ToLowerInvariant();

                        if (cityLookup.TryGetValue(key, out var coords))
                        {
                            geocodedWithCoordsIds.Add(job.id);
                            geocodedLats.Add(coords.lat);
                            geocodedLons.Add(coords.lon);
                            successCount++;
                        }
                        else
                        {
                            // City not found - still mark as geocoded but leave lat/lon null
                            geocodedIds.Add(job.id);
                            notFoundCount++;
                        }
                    }
                    else
                    {
                        // Missing city/state - still mark as geocoded (US job but incomplete location data)
                        geocodedIds.Add(job.id);
                        notFoundCount++;
                    }
                }

                // Update jobs with coordinates
                if (geocodedWithCoordsIds.Count > 0)
                {
                    // Create temp table for bulk update
                    await using (var cmd = new NpgsqlCommand(@"
                        CREATE TEMP TABLE temp_geocode (
                            job_id UUID,
                            lat DECIMAL,
                            lon DECIMAL
                        ) ON COMMIT DROP", conn, transaction))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // COPY data into temp table
                    await using (var writer = await conn.BeginBinaryImportAsync(
                        "COPY temp_geocode (job_id, lat, lon) FROM STDIN (FORMAT BINARY)"))
                    {
                        for (int i = 0; i < geocodedWithCoordsIds.Count; i++)
                        {
                            await writer.StartRowAsync();
                            await writer.WriteAsync(geocodedWithCoordsIds[i], NpgsqlTypes.NpgsqlDbType.Uuid);
                            await writer.WriteAsync(geocodedLats[i], NpgsqlTypes.NpgsqlDbType.Numeric);
                            await writer.WriteAsync(geocodedLons[i], NpgsqlTypes.NpgsqlDbType.Numeric);
                        }
                        await writer.CompleteAsync();
                    }

                    // UPDATE jobs from temp table
                    await using (var cmd = new NpgsqlCommand(@"
                        UPDATE jobs j
                        SET
                            latitude = t.lat,
                            longitude = t.lon,
                            status = 'geocoded'
                        FROM temp_geocode t
                        WHERE j.id = t.job_id", conn, transaction))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Update jobs without coordinates (city not found or missing data)
                if (geocodedIds.Count > 0)
                {
                    await using var cmd = new NpgsqlCommand(@"
                        UPDATE jobs
                        SET status = 'geocoded'
                        WHERE id = ANY(@ids)", conn, transaction);
                    cmd.Parameters.AddWithValue("ids", geocodedIds.ToArray());
                    await cmd.ExecuteNonQueryAsync();
                }

                // Update invalid jobs (non-US)
                if (invalidIds.Count > 0)
                {
                    await using var cmd = new NpgsqlCommand(@"
                        UPDATE jobs
                        SET status = 'invalid - non-us-location',
                            is_valid = false
                        WHERE id = ANY(@ids)", conn, transaction);
                    cmd.Parameters.AddWithValue("ids", invalidIds.ToArray());
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                processedCount += jobs.Count;
                batchStopwatch.Stop();

                context.Logger.LogInformation($"[BATCH] Processed {jobs.Count} jobs in {batchStopwatch.ElapsedMilliseconds}ms (Total: {processedCount}/{totalCount})");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing batch: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Update any remaining jobs with status 'location_classified' to 'geocoded'
        // (these are jobs that already had coordinates from a previous run)
        await using (var cmd = new NpgsqlCommand(@"
            UPDATE jobs
            SET status = 'geocoded'
            WHERE status = 'location_classified'", conn))
        {
            var skippedCount = await cmd.ExecuteNonQueryAsync();
            if (skippedCount > 0)
            {
                context.Logger.LogInformation($"Updated {skippedCount} job(s) that already had coordinates to 'geocoded'");
            }
        }

        totalStopwatch.Stop();

        context.Logger.LogInformation($"\nSuccessfully geocoded: {successCount}");
        context.Logger.LogInformation($"City not found: {notFoundCount}");
        context.Logger.LogInformation($"Invalid (non-US): {invalidCount}");
        context.Logger.LogInformation($"Total processed: {processedCount}");
        context.Logger.LogInformation($"[PERF] Total execution time: {totalStopwatch.ElapsedMilliseconds}ms");

        context.Logger.LogInformation("=== Geocode Lambda Complete ===");
    }

    private async Task<Dictionary<string, (decimal lat, decimal lon)>> LoadCityDataAsync(ILambdaContext context)
    {
        var cityLookup = new Dictionary<string, (decimal lat, decimal lon)>();

        // The CSV file will be in the same directory as the Lambda executable
        var csvPath = Path.Combine(AppContext.BaseDirectory, "us_cities.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"US cities CSV not found at {csvPath}");
        }

        var lines = await File.ReadAllLinesAsync(csvPath);

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

                    // If duplicate, keep the first one
                    if (!cityLookup.ContainsKey(key))
                    {
                        cityLookup[key] = (lat, lon);
                    }
                }
            }
        }

        return cityLookup;
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

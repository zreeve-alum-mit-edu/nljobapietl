#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.3"
#r "nuget: System.Text.Json, 8.0.0"

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql;

var connString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Port=5432;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Keepalive=30;Timeout=15";

Console.WriteLine("Analyzing duplicate job descriptions...");

var duplicateHashes = new List<DuplicateJobGroup>();

using (var conn = new NpgsqlConnection(connString))
{
    await conn.OpenAsync();

    // Step 1: Find all hashes that appear more than once
    Console.WriteLine("Step 1: Finding duplicate hashes...");
    var duplicateHashValues = new List<string>();

    var findDuplicatesQuery = @"
        SELECT job_description_hash, COUNT(*) as count
        FROM jobs
        WHERE job_description_hash IS NOT NULL
        GROUP BY job_description_hash
        HAVING COUNT(*) > 1
        ORDER BY COUNT(*) DESC";

    using (var cmd = new NpgsqlCommand(findDuplicatesQuery, conn))
    using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            duplicateHashValues.Add(reader.GetString(0));
        }
    }

    Console.WriteLine($"Found {duplicateHashValues.Count} duplicate hashes");

    // Step 2: For each duplicate hash, get locations and URLs
    Console.WriteLine("Step 2: Analyzing locations and URLs for each duplicate...");

    int processed = 0;
    foreach (var hash in duplicateHashValues)
    {
        var group = new DuplicateJobGroup
        {
            JobDescriptionHash = hash,
            Locations = new List<LocationGroup>()
        };

        // Get all jobs with this hash
        var jobsQuery = @"
            SELECT j.id, j.generated_city, j.generated_state, j.generated_country, j.job_url
            FROM jobs j
            WHERE j.job_description_hash = @hash
            ORDER BY j.generated_country, j.generated_state, j.generated_city, j.job_url";

        using (var cmd = new NpgsqlCommand(jobsQuery, conn))
        {
            cmd.Parameters.AddWithValue("hash", hash);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                var locationDict = new Dictionary<string, LocationGroup>();

                while (await reader.ReadAsync())
                {
                    var jobId = reader.GetGuid(0);
                    var city = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var state = reader.IsDBNull(2) ? null : reader.GetString(2);
                    var country = reader.IsDBNull(3) ? null : reader.GetString(3);
                    var url = reader.IsDBNull(4) ? null : reader.GetString(4);

                    var locationKey = $"{country ?? "NULL"}|{state ?? "NULL"}|{city ?? "NULL"}";

                    if (!locationDict.ContainsKey(locationKey))
                    {
                        locationDict[locationKey] = new LocationGroup
                        {
                            GeneratedCity = city,
                            GeneratedState = state,
                            GeneratedCountry = country,
                            Urls = new List<string>()
                        };
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        locationDict[locationKey].Urls.Add(url);
                    }
                }

                group.Locations = locationDict.Values.ToList();
                group.TotalJobs = group.Locations.Sum(l => l.Urls.Count);
            }
        }

        duplicateHashes.Add(group);

        processed++;
        if (processed % 100 == 0)
        {
            Console.WriteLine($"Processed {processed}/{duplicateHashValues.Count} duplicate hashes...");
        }
    }
}

Console.WriteLine($"Analysis complete. Found {duplicateHashes.Count} duplicate groups");
Console.WriteLine($"Total duplicate jobs: {duplicateHashes.Sum(g => g.TotalJobs)}");

// Write to JSON
var outputPath = "/mnt/c/GIT/JobApi.New/duplicate_jobs_analysis.json";
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var json = JsonSerializer.Serialize(duplicateHashes, options);
File.WriteAllText(outputPath, json);

Console.WriteLine($"Analysis written to: {outputPath}");

// Summary statistics
Console.WriteLine("\n=== SUMMARY ===");
Console.WriteLine($"Total duplicate hash groups: {duplicateHashes.Count}");
Console.WriteLine($"Total duplicate jobs: {duplicateHashes.Sum(g => g.TotalJobs)}");
Console.WriteLine($"Average jobs per hash: {duplicateHashes.Average(g => g.TotalJobs):F2}");
Console.WriteLine($"Max jobs for one hash: {duplicateHashes.Max(g => g.TotalJobs)}");
Console.WriteLine($"Total locations across all duplicates: {duplicateHashes.Sum(g => g.Locations.Count)}");
Console.WriteLine($"Average locations per hash: {duplicateHashes.Average(g => g.Locations.Count):F2}");

public class DuplicateJobGroup
{
    [JsonPropertyName("job_description_hash")]
    public string JobDescriptionHash { get; set; }

    [JsonPropertyName("total_jobs")]
    public int TotalJobs { get; set; }

    [JsonPropertyName("locations")]
    public List<LocationGroup> Locations { get; set; }
}

public class LocationGroup
{
    [JsonPropertyName("generated_city")]
    public string? GeneratedCity { get; set; }

    [JsonPropertyName("generated_state")]
    public string? GeneratedState { get; set; }

    [JsonPropertyName("generated_country")]
    public string? GeneratedCountry { get; set; }

    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; }
}

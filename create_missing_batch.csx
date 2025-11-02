#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"
#r "nuget: Npgsql.EntityFrameworkCore.PostgreSQL, 8.0.0"
#r "nuget: DotNetEnv, 3.0.0"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;

Console.WriteLine("=== Creating batch file for 250k jobs ===\n");

// Load environment variables
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

// Set up output paths
var batchId = Guid.NewGuid();
var llmBatchFolder = "/mnt/c/GIT/JobApi.New/Data/llmbatch";
var filePath = Path.Combine(llmBatchFolder, $"workplace_batch_{batchId}_missing.jsonl");

Directory.CreateDirectory(llmBatchFolder);

Console.WriteLine($"Batch ID: {batchId}");
Console.WriteLine($"Output file: {filePath}\n");

// Use Npgsql directly for reading data
using var conn = new Npgsql.NpgsqlConnection(connString);
await conn.OpenAsync();

Console.WriteLine("Querying jobs with status 'llm_batches_generated' and no workplace...");

var cmd = new Npgsql.NpgsqlCommand(@"
    SELECT id, job_title, company_name, locality, region, country, location, job_description
    FROM jobs
    WHERE status = 'llm_batches_generated' AND generated_workplace IS NULL
    ORDER BY id
", conn);

var jobs = new List<dynamic>();
using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        jobs.Add(new
        {
            Id = reader.GetGuid(0),
            JobTitle = reader.IsDBNull(1) ? null : reader.GetString(1),
            CompanyName = reader.IsDBNull(2) ? null : reader.GetString(2),
            Locality = reader.IsDBNull(3) ? null : reader.GetString(3),
            Region = reader.IsDBNull(4) ? null : reader.GetString(4),
            Country = reader.IsDBNull(5) ? null : reader.GetString(5),
            Location = reader.IsDBNull(6) ? null : reader.GetString(6),
            JobDescription = reader.IsDBNull(7) ? null : reader.GetString(7)
        });
    }
}

Console.WriteLine($"Found {jobs.Count} jobs\n");

if (jobs.Count == 0)
{
    Console.WriteLine("No jobs found!");
    return;
}

// Generate batch file
Console.WriteLine("Generating batch file...");

var systemPrompt = @"You are a workplace type classifier. Analyze the job posting and determine the workplace type.

Respond with ONLY a JSON object in this format:
{""type"":""REMOTE|HYBRID|ONSITE"",""inferred"":true|false,""confidence"":""EXPLICIT|LIKELY|PROBABLY|GUESS""}

- type: REMOTE, HYBRID, or ONSITE
- inferred: true if the workplace type is not explicitly stated, false if it is clearly stated
- confidence: EXPLICIT if clearly stated, LIKELY if strong indicators, PROBABLY if moderate indicators, GUESS if weak indicators";

const int DescriptionMaxLength = 2000;

using (var writer = new StreamWriter(filePath))
{
    foreach (var job in jobs)
    {
        // Build location context
        var locationParts = new List<string>();
        if (!string.IsNullOrEmpty(job.Locality)) locationParts.Add(job.Locality);
        if (!string.IsNullOrEmpty(job.Region)) locationParts.Add(job.Region);
        if (!string.IsNullOrEmpty(job.Country)) locationParts.Add(job.Country);
        var locationContext = locationParts.Count > 0
            ? string.Join(", ", locationParts)
            : job.Location ?? "Not specified";

        // Truncate description
        string description = job.JobDescription ?? "No description provided";
        if (description.Length > DescriptionMaxLength)
        {
            description = description.Substring(0, DescriptionMaxLength) + "...";
        }

        // Build user message
        var userMessage = $@"Title: {job.JobTitle ?? "Not specified"}
Company: {job.CompanyName ?? "Not specified"}
Location: {locationContext}
Description: {description}";

        var batchRequest = new
        {
            custom_id = $"job_{job.Id}",
            method = "POST",
            url = "/v1/chat/completions",
            body = new
            {
                model = "gpt-5-nano",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_completion_tokens = 2000,
                response_format = new { type = "json_object" }
            }
        };

        var json = JsonSerializer.Serialize(batchRequest);
        await writer.WriteLineAsync(json);
    }
}

Console.WriteLine($"Batch file created: {filePath}");
Console.WriteLine($"Total requests: {jobs.Count}");

// Create workplace_batches tracking record
Console.WriteLine("\nCreating workplace_batches tracking record...");

var insertCmd = new Npgsql.NpgsqlCommand(@"
    INSERT INTO workplace_batches (id, file_id, batch_file_path, status, created_at)
    VALUES (@id, @fileId, @path, @status, @created)
", conn);

insertCmd.Parameters.AddWithValue("@id", batchId);
insertCmd.Parameters.AddWithValue("@fileId", Guid.Empty); // Use empty GUID for this one-off batch
insertCmd.Parameters.AddWithValue("@path", filePath);
insertCmd.Parameters.AddWithValue("@status", "pending");
insertCmd.Parameters.AddWithValue("@created", DateTime.UtcNow);

await insertCmd.ExecuteNonQueryAsync();

Console.WriteLine($"Created batch tracking record with ID: {batchId}");
Console.WriteLine("\n=== DONE ===");

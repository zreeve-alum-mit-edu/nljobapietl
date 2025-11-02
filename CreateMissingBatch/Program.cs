using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using Npgsql;

Console.WriteLine("=== Consolidating Location Batch Files ===\n");

// Load environment variables
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

// Build from components if not provided
if (string.IsNullOrEmpty(connString))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

    if (!string.IsNullOrEmpty(dbHost))
    {
        connString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Port={dbPort}";
    }
}

if (string.IsNullOrEmpty(connString))
{
    Console.WriteLine("ERROR: DB connection information not found");
    return;
}

// Get all batch files
var locationBatchFolder = "/mnt/c/GIT/JobApi.New/Data/locationbatch";
var allBatchFiles = Directory.GetFiles(locationBatchFolder, "location_batch_*.jsonl")
    .OrderBy(f => f)
    .ToList();

Console.WriteLine($"Found {allBatchFiles.Count} batch files");

if (allBatchFiles.Count == 0)
{
    Console.WriteLine("No batch files to consolidate!");
    return;
}

// Create consolidated file
var consolidatedBatchId = Guid.NewGuid();
var consolidatedFilePath = Path.Combine(locationBatchFolder, $"location_batch_consolidated_{consolidatedBatchId}.jsonl");

Console.WriteLine($"\nConsolidating into: {Path.GetFileName(consolidatedFilePath)}");

var totalLines = 0;
using (var writer = new StreamWriter(consolidatedFilePath))
{
    foreach (var batchFile in allBatchFiles)
    {
        var lines = await File.ReadAllLinesAsync(batchFile);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                await writer.WriteLineAsync(line);
                totalLines++;
            }
        }
    }
}

Console.WriteLine($"Wrote {totalLines} requests to consolidated file");

// Get file size
var fileInfo = new FileInfo(consolidatedFilePath);
Console.WriteLine($"File size: {fileInfo.Length / (1024.0 * 1024.0):F2} MB");

// Update database - delete all old location_batches records and create one new one
Console.WriteLine("\nUpdating location_batches table...");

using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

// Delete all existing location_batches records
var deleteCmd = new NpgsqlCommand("DELETE FROM location_batches", conn);
var deletedRows = await deleteCmd.ExecuteNonQueryAsync();
Console.WriteLine($"Deleted {deletedRows} old location_batches records");

// Insert new consolidated batch record
var insertCmd = new NpgsqlCommand(@"
    INSERT INTO location_batches (id, file_id, batch_file_path, status, created_at)
    VALUES (@id, @fileId, @path, @status, @created)
", conn);

insertCmd.Parameters.AddWithValue("@id", consolidatedBatchId);
insertCmd.Parameters.AddWithValue("@fileId", Guid.Empty); // No specific file_id for consolidated batch
insertCmd.Parameters.AddWithValue("@path", consolidatedFilePath);
insertCmd.Parameters.AddWithValue("@status", "pending");
insertCmd.Parameters.AddWithValue("@created", DateTime.UtcNow);

await insertCmd.ExecuteNonQueryAsync();
Console.WriteLine($"Created new location_batches record with ID: {consolidatedBatchId}");

// Delete old batch files
Console.WriteLine("\nDeleting old batch files...");
foreach (var batchFile in allBatchFiles)
{
    File.Delete(batchFile);
}
Console.WriteLine($"Deleted {allBatchFiles.Count} old batch files");

Console.WriteLine("\n=== DONE ===");
Console.WriteLine($"Consolidated batch file: {consolidatedFilePath}");
Console.WriteLine($"Total requests: {totalLines}");

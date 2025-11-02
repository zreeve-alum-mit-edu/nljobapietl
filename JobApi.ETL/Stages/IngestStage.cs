using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using DotNetEnv;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using FileEntity = JobApi.Common.Entities.File;

namespace JobApi.ETL.Stages;

public class IngestStage
{
    private readonly string _ingestableFolder;
    private readonly string _ingestedFolder;

    public IngestStage(string dataRootPath)
    {
        _ingestableFolder = Path.Combine(dataRootPath, "Ingestable");
        _ingestedFolder = Path.Combine(dataRootPath, "Ingested");

        // Ensure folders exist
        Directory.CreateDirectory(_ingestableFolder);
        Directory.CreateDirectory(_ingestedFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== INGEST STAGE ===");
        Console.WriteLine($"Looking for files in: {_ingestableFolder}");

        var jsonlFiles = Directory.GetFiles(_ingestableFolder, "*.jsonl")
            .Concat(Directory.GetFiles(_ingestableFolder, "*.jsonl.gz"))
            .OrderBy(f => f)
            .ToArray();

        if (jsonlFiles.Length == 0)
        {
            Console.WriteLine("No JSONL files found to ingest.");
            return true;
        }

        Console.WriteLine($"Found {jsonlFiles.Length} file(s) available");

        // Only process the first file found
        var filePath = jsonlFiles[0];
        Console.WriteLine($"Processing 1 file per run: {Path.GetFileName(filePath)}");

        try
        {
            await IngestFile(filePath);

            // Move to Ingested folder
            var destPath = Path.Combine(_ingestedFolder, Path.GetFileName(filePath));
            System.IO.File.Move(filePath, destPath);
            Console.WriteLine($"Moved to: {destPath}");

            if (jsonlFiles.Length > 1)
            {
                Console.WriteLine($"\n{jsonlFiles.Length - 1} file(s) remaining for next run");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR processing {Path.GetFileName(filePath)}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                if (ex.InnerException.InnerException != null)
                {
                    Console.WriteLine($"  Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                }
            }
            Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task IngestFile(string filePath)
    {
        using var db = JobContext.Create();

        // Create File record
        var fileRecord = new FileEntity
        {
            Id = Guid.NewGuid(),
            Filename = Path.GetFileName(filePath),
            DateProcessed = DateTime.UtcNow
        };

        await db.Files.AddAsync(fileRecord);
        await db.SaveChangesAsync();
        Console.WriteLine($"Created file record: {fileRecord.Id}");

        var jobs = new List<Job>();
        var batchSize = 2000;
        var lineCount = 0;
        var totalInserted = 0;

        var isGzipped = filePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"Streaming and parsing {(isGzipped ? "gzipped " : "")}JSONL file...");

        using var fileStream = System.IO.File.OpenRead(filePath);
        Stream readStream = isGzipped ? new GZipStream(fileStream, CompressionMode.Decompress) : fileStream;

        using var reader = new StreamReader(readStream);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineCount++;

            try
            {
                var jsonJob = JsonSerializer.Deserialize<JsonlJob>(line);
                if (jsonJob == null) continue;

                var job = MapToJob(jsonJob, fileRecord.Id);
                jobs.Add(job);

                if (jobs.Count >= batchSize)
                {
                    var inserted = await SaveJobBatch(db, jobs, lineCount);
                    totalInserted += inserted;
                    jobs.Clear();
                    db.ChangeTracker.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on line {lineCount}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                }
            }
        }

        // Insert remaining jobs
        if (jobs.Count > 0)
        {
            var inserted = await SaveJobBatch(db, jobs, lineCount);
            totalInserted += inserted;
            Console.WriteLine($"Inserted final batch. Total: {totalInserted} jobs");
        }

        Console.WriteLine($"Import complete! Total jobs inserted: {totalInserted}");
    }

    private static async Task<int> SaveJobBatch(JobContext db, List<Job> jobs, int lineCount)
    {
        try
        {
            await db.Jobs.AddRangeAsync(jobs);
            await db.SaveChangesAsync();
            Console.WriteLine($"Inserted {jobs.Count} jobs (line {lineCount})...");
            return jobs.Count;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Duplicate key error - save jobs one by one to skip duplicates
            Console.WriteLine($"Duplicate key detected in batch, processing jobs individually...");

            // Clear the change tracker to remove the failed batch
            db.ChangeTracker.Clear();

            int successCount = 0;
            int duplicateCount = 0;

            foreach (var job in jobs)
            {
                try
                {
                    await db.Jobs.AddAsync(job);
                    await db.SaveChangesAsync();
                    successCount++;
                }
                catch (DbUpdateException dupEx) when (dupEx.InnerException is PostgresException pgDup && pgDup.SqlState == "23505")
                {
                    // Skip this duplicate
                    duplicateCount++;
                    db.ChangeTracker.Clear();
                }
            }

            Console.WriteLine($"Inserted {successCount} jobs ({duplicateCount} duplicates skipped, line {lineCount})...");
            return successCount;
        }
    }

    private static Job MapToJob(JsonlJob jsonJob, Guid fileId)
    {
        return new Job
        {
            Id = Guid.NewGuid(),
            DateInserted = DateTime.UtcNow,
            Status = "ingested",
            IsValid = true,
            FileId = fileId,
            Portal = Truncate(jsonJob.Portal, 100),
            Source = Truncate(jsonJob.Source, 100),
            SourceCC = Truncate(jsonJob.SourceCC, 10),
            IsDuplicate = jsonJob.IsDuplicate,
            Locale = Truncate(jsonJob.Locale, 10),
            JobTitle = Truncate(jsonJob.Name, 500),
            JobUrl = Truncate(jsonJob.Url, 1000),
            JobDescription = jsonJob.Text,
            Location = Truncate(jsonJob.Location?.OrgAddress?.AddressLine, 500),
            Country = Truncate(jsonJob.Json?.SchemaOrg?.JobLocation?.Address?.AddressCountry
                      ?? jsonJob.Json?.JsonLD?.JobLocation?.Address?.AddressCountry, 100),
            Region = Truncate(jsonJob.Json?.SchemaOrg?.JobLocation?.Address?.AddressRegion
                     ?? jsonJob.Json?.JsonLD?.JobLocation?.Address?.AddressRegion, 100),
            Locality = Truncate(jsonJob.Json?.SchemaOrg?.JobLocation?.Address?.AddressLocality
                       ?? jsonJob.Json?.JsonLD?.JobLocation?.Address?.AddressLocality, 100),
            Postcode = Truncate(jsonJob.Json?.SchemaOrg?.JobLocation?.Address?.PostalCode
                       ?? jsonJob.Json?.JsonLD?.JobLocation?.Address?.PostalCode, 20),
            Latitude = jsonJob.Json?.SchemaOrg?.JobLocation?.Latitude
                       ?? jsonJob.Json?.JsonLD?.JobLocation?.Latitude,
            Longitude = jsonJob.Json?.SchemaOrg?.JobLocation?.Longitude
                        ?? jsonJob.Json?.JsonLD?.JobLocation?.Longitude,
            DatePosted = ParseDate(jsonJob.Json?.SchemaOrg?.DatePosted ?? jsonJob.Json?.JsonLD?.DatePosted),
            EmploymentType = Truncate(jsonJob.Json?.SchemaOrg?.EmploymentType
                            ?? jsonJob.Json?.JsonLD?.EmploymentType, 100),
            CompanyName = Truncate(jsonJob.Company?.Name, 500),
            CompanyUrl = Truncate(jsonJob.Company?.Info?.CareerPageURL, 1000),
            ValidThrough = ParseDate(jsonJob.Json?.SchemaOrg?.ValidThrough ?? jsonJob.Json?.JsonLD?.ValidThrough),
            WorkplaceType = null, // Will be classified by LLM in later stage
            Embedding = null // Will be populated later
        };
    }

    private static DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString)) return null;
        if (DateTime.TryParse(dateString, out var date))
        {
            // Ensure UTC for PostgreSQL
            if (date.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            else if (date.Kind == DateTimeKind.Local)
                return date.ToUniversalTime();
            return date;
        }
        return null;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}

// JSONL structure classes
public class JsonlJob
{
    [JsonPropertyName("idInSource")]
    public string? IdInSource { get; set; }

    [JsonPropertyName("portal")]
    public string? Portal { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("sourceCC")]
    public string? SourceCC { get; set; }

    [JsonPropertyName("isDuplicate")]
    public bool IsDuplicate { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("json")]
    public JsonData? Json { get; set; }

    [JsonPropertyName("location")]
    public LocationData? Location { get; set; }

    [JsonPropertyName("company")]
    public CompanyData? Company { get; set; }

    [JsonPropertyName("position")]
    public PositionData? Position { get; set; }
}

public class JsonData
{
    [JsonPropertyName("schemaOrg")]
    public SchemaOrgData? SchemaOrg { get; set; }

    [JsonPropertyName("jsonLD")]
    public SchemaOrgData? JsonLD { get; set; }
}

public class SchemaOrgData
{
    [JsonPropertyName("datePosted")]
    public string? DatePosted { get; set; }

    [JsonPropertyName("employmentType")]
    public string? EmploymentType { get; set; }

    [JsonPropertyName("validThrough")]
    public string? ValidThrough { get; set; }

    [JsonPropertyName("jobLocationType")]
    public string? JobLocationType { get; set; }

    [JsonPropertyName("jobLocation")]
    public JobLocationData? JobLocation { get; set; }
}

public class JobLocationData
{
    [JsonPropertyName("address")]
    public AddressData? Address { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }
}

public class AddressData
{
    [JsonPropertyName("addressCountry")]
    public string? AddressCountry { get; set; }

    [JsonPropertyName("addressRegion")]
    public string? AddressRegion { get; set; }

    [JsonPropertyName("addressLocality")]
    public string? AddressLocality { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
}

public class LocationData
{
    [JsonPropertyName("orgAddress")]
    public OrgAddressData? OrgAddress { get; set; }
}

public class OrgAddressData
{
    [JsonPropertyName("addressLine")]
    public string? AddressLine { get; set; }
}

public class CompanyData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("info")]
    public CompanyInfoData? Info { get; set; }
}

public class CompanyInfoData
{
    [JsonPropertyName("careerpageURL")]
    public string? CareerPageURL { get; set; }
}

public class PositionData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("workType")]
    public string? WorkType { get; set; }

    [JsonPropertyName("workPlace")]
    public string? WorkPlace { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }
}

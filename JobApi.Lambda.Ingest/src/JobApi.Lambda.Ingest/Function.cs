using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using FileEntity = JobApi.Common.Entities.File;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.Ingest;

public class Function
{
    private readonly IAmazonS3 _s3Client;

    public Function()
    {
        _s3Client = new AmazonS3Client();
    }

    // Constructor for testing with mock S3 client
    public Function(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    /// <summary>
    /// Lambda handler for ingesting JSONL job files from S3
    /// Triggered when a file is uploaded to the ingestable/intake/ folder
    /// </summary>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        foreach (var record in s3Event.Records)
        {
            var bucketName = record.S3.Bucket.Name;
            var key = record.S3.Object.Key;
            var fileName = Path.GetFileName(key);
            var processingKey = $"ingestable/processing/{fileName}";
            var errorKey = $"ingestable/error/{fileName}";
            var ingestedKey = $"ingested/{fileName}";

            context.Logger.LogInformation($"Attempting to claim file for processing: s3://{bucketName}/{key}");

            try
            {
                // Idempotency: Move file to processing folder. Only ONE Lambda instance will succeed.
                await _s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = key,
                    DestinationBucket = bucketName,
                    DestinationKey = processingKey
                });

                // Delete from original location
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                });

                context.Logger.LogInformation($"Successfully claimed file, now processing from: {processingKey}");

                await ProcessIngestFile(bucketName, processingKey, fileName, context);

                // Success - move to ingested folder
                await _s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = processingKey,
                    DestinationBucket = bucketName,
                    DestinationKey = ingestedKey
                });

                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = processingKey
                });

                context.Logger.LogInformation($"Successfully processed and moved to: {ingestedKey}");
            }
            catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // File was already moved by another Lambda instance - skip processing
                context.Logger.LogInformation($"File already claimed by another instance: {key}");
                return;
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing {key}: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");

                // Move to error folder for manual review
                try
                {
                    context.Logger.LogError($"Attempting to move {processingKey} to error folder...");

                    await _s3Client.CopyObjectAsync(new CopyObjectRequest
                    {
                        SourceBucket = bucketName,
                        SourceKey = processingKey,
                        DestinationBucket = bucketName,
                        DestinationKey = errorKey
                    });

                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = processingKey
                    });

                    context.Logger.LogError($"Successfully moved failed file to: {errorKey}");
                }
                catch (AmazonS3Exception s3MoveEx) when (s3MoveEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    context.Logger.LogError($"CRITICAL: Processing file no longer exists at {processingKey} - file may be lost!");
                }
                catch (Exception moveEx)
                {
                    context.Logger.LogError($"CRITICAL: Failed to move file to error folder: {moveEx.Message}");
                    context.Logger.LogError($"File is still at: {processingKey}");
                }

                // DO NOT throw - file already moved to error or lost, retrying won't help
            }
        }
    }

    private async Task ProcessIngestFile(string bucketName, string key, string fileName, ILambdaContext context)
    {
        var lineCount = 0;
        var totalInserted = 0;
        var batchSize = 2000;
        var jobs = new List<Job>(capacity: batchSize);

        // Create DbContext
        await using var db = JobContext.Create();

        // Create File record
        var fileRecord = new FileEntity
        {
            Id = Guid.NewGuid(),
            Filename = fileName,
            DateProcessed = DateTime.UtcNow
        };

        await db.Files.AddAsync(fileRecord);
        await db.SaveChangesAsync();
        context.Logger.LogInformation($"Created file record: {fileRecord.Id}");

        try
        {
            // Download and stream the file from S3
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(getObjectRequest);

            var isGzipped = fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            context.Logger.LogInformation($"Streaming and parsing {(isGzipped ? "gzipped " : "")}JSONL file...");

            Stream readStream = isGzipped
                ? new GZipStream(response.ResponseStream, CompressionMode.Decompress)
                : response.ResponseStream;

            using var reader = new StreamReader(readStream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineCount++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var jsonJob = JsonSerializer.Deserialize<JsonlJob>(line);
                    if (jsonJob == null) continue;

                    var job = MapToJob(jsonJob, fileRecord.Id);
                    jobs.Add(job);

                    if (jobs.Count >= batchSize)
                    {
                        var inserted = await SaveJobBatch(db, jobs, lineCount, context);
                        totalInserted += inserted;
                        jobs.Clear();
                        db.ChangeTracker.Clear();
                    }
                }
                catch (JsonException jsonEx)
                {
                    context.Logger.LogWarning($"Line {lineCount}: JSON parsing error: {jsonEx.Message}");
                }
                catch (Exception ex)
                {
                    context.Logger.LogWarning($"Line {lineCount}: Error processing line: {ex.Message}");
                }
            }

            // Insert remaining jobs
            if (jobs.Count > 0)
            {
                var inserted = await SaveJobBatch(db, jobs, lineCount, context);
                totalInserted += inserted;
                context.Logger.LogInformation($"Inserted final batch. Total: {totalInserted} jobs");
            }

            context.Logger.LogInformation($"Import complete! Total jobs inserted: {totalInserted} from {lineCount} lines");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to process file: {ex.Message}");
            throw;
        }
    }

    private static async Task<int> SaveJobBatch(JobContext db, List<Job> jobs, int lineCount, ILambdaContext context)
    {
        try
        {
            await db.Jobs.AddRangeAsync(jobs);
            await db.SaveChangesAsync();
            context.Logger.LogInformation($"Inserted {jobs.Count} jobs (line {lineCount})...");
            return jobs.Count;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Duplicate key error - save jobs one by one to skip duplicates
            context.Logger.LogInformation($"Duplicate key detected in batch, processing jobs individually...");

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
                    duplicateCount++;
                    db.ChangeTracker.Clear();
                }
            }

            context.Logger.LogInformation($"Inserted {successCount} jobs ({duplicateCount} duplicates skipped, line {lineCount})...");
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
            WorkplaceType = null,
            Embedding = null
        };
    }

    private static DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString)) return null;
        if (DateTime.TryParse(dateString, out var date))
        {
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

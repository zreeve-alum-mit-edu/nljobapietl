using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
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
        var skippedCount = 0;
        var totalInserted = 0;
        var batchSize = 2000;
        var processedJobs = new List<ProcessedJob>(capacity: batchSize);

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

                    var processedJob = MapToProcessedJob(jsonJob, fileRecord.Id);
                    if (processedJob == null)
                    {
                        skippedCount++;
                        continue; // Skip invalid jobs (no URL or no description)
                    }

                    processedJobs.Add(processedJob);

                    if (processedJobs.Count >= batchSize)
                    {
                        var inserted = await SaveJobBatch(db, processedJobs, lineCount, context);
                        totalInserted += inserted;
                        processedJobs.Clear();
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
            if (processedJobs.Count > 0)
            {
                var inserted = await SaveJobBatch(db, processedJobs, lineCount, context);
                totalInserted += inserted;
                context.Logger.LogInformation($"Inserted final batch. Total: {totalInserted} jobs");
            }

            context.Logger.LogInformation($"Import complete! Total jobs inserted: {totalInserted}, skipped: {skippedCount} from {lineCount} lines");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to process file: {ex.Message}");
            throw;
        }
    }

    private static async Task<int> SaveJobBatch(JobContext db, List<ProcessedJob> processedJobs, int lineCount, ILambdaContext context)
    {
        if (processedJobs.Count == 0) return 0;

        var totalInserted = 0;

        // Step 1: Find existing jobs by hash
        var hashes = processedJobs.Select(pj => pj.Hash).Distinct().ToList();
        var existingJobs = await db.Jobs
            .Where(j => hashes.Contains(j.JobDescriptionHash!))
            .Select(j => new { j.Id, j.JobDescriptionHash })
            .ToListAsync();

        var existingJobsByHash = existingJobs.ToDictionary(j => j.JobDescriptionHash!, j => j.Id);

        // Map ProcessedJob to actual job ID (existing or new)
        var jobIdMap = new Dictionary<string, Guid>(); // hash -> job_id

        var newJobs = new List<Job>();
        foreach (var pj in processedJobs)
        {
            if (existingJobsByHash.TryGetValue(pj.Hash, out var existingJobId))
            {
                jobIdMap[pj.Hash] = existingJobId;
            }
            else if (!jobIdMap.ContainsKey(pj.Hash))
            {
                // New job - will be inserted
                jobIdMap[pj.Hash] = pj.Job.Id;
                newJobs.Add(pj.Job);
            }
        }

        // Insert new jobs
        if (newJobs.Count > 0)
        {
            await db.Jobs.AddRangeAsync(newJobs);
            await db.SaveChangesAsync();
            totalInserted += newJobs.Count;
            context.Logger.LogInformation($"Inserted {newJobs.Count} new jobs");
        }

        // Step 2: Update ProcessedJob entities with correct job IDs
        foreach (var pj in processedJobs)
        {
            var actualJobId = jobIdMap[pj.Hash];
            pj.Location.JobId = actualJobId;
        }

        // Step 3: Find existing locations by (job_id, location)
        var locationKeys = processedJobs
            .Select(pj => new { JobId = pj.Location.JobId, Location = pj.LocationString })
            .Distinct()
            .ToList();

        var jobIds = locationKeys.Select(k => k.JobId).ToList();
        var existingLocations = await db.JobLocations
            .Where(jl => jobIds.Contains(jl.JobId))
            .Select(jl => new { jl.Id, jl.JobId, Location = jl.Location ?? string.Empty })
            .ToListAsync();

        var existingLocationsByKey = existingLocations
            .ToDictionary(jl => (jl.JobId, jl.Location), jl => jl.Id);

        // Map to actual location ID (existing or new)
        var locationIdMap = new Dictionary<(Guid jobId, string location), Guid>();

        var newLocations = new List<JobLocation>();
        foreach (var pj in processedJobs)
        {
            var key = (pj.Location.JobId, pj.LocationString);
            if (existingLocationsByKey.TryGetValue(key, out var existingLocationId))
            {
                locationIdMap[key] = existingLocationId;
            }
            else if (!locationIdMap.ContainsKey(key))
            {
                // New location - will be inserted
                locationIdMap[key] = pj.Location.Id;
                newLocations.Add(pj.Location);
            }
        }

        // Insert new locations
        if (newLocations.Count > 0)
        {
            await db.JobLocations.AddRangeAsync(newLocations);
            await db.SaveChangesAsync();
            context.Logger.LogInformation($"Inserted {newLocations.Count} new locations");
        }

        // Step 4: Update ProcessedJob URL entities with correct location IDs
        foreach (var pj in processedJobs)
        {
            var key = (pj.Location.JobId, pj.LocationString);
            var actualLocationId = locationIdMap[key];
            pj.Url.JobLocationId = actualLocationId;
        }

        // Step 5: Find existing URLs by (location_id, url)
        var locationIds = locationIdMap.Values.Distinct().ToList();
        var existingUrls = await db.JobLocationUrls
            .Where(jlu => locationIds.Contains(jlu.JobLocationId))
            .Select(jlu => new { jlu.JobLocationId, jlu.Url })
            .ToListAsync();

        var existingUrlSet = existingUrls
            .Select(u => (u.JobLocationId, u.Url))
            .ToHashSet();

        // Insert only new URLs
        var newUrls = processedJobs
            .Where(pj => !existingUrlSet.Contains((pj.Url.JobLocationId, pj.UrlString)))
            .Select(pj => pj.Url)
            .GroupBy(u => (u.JobLocationId, u.Url))
            .Select(g => g.First()) // Remove duplicates within this batch
            .ToList();

        if (newUrls.Count > 0)
        {
            await db.JobLocationUrls.AddRangeAsync(newUrls);
            await db.SaveChangesAsync();
            context.Logger.LogInformation($"Inserted {newUrls.Count} new URLs");
        }

        context.Logger.LogInformation($"Batch complete (line {lineCount}): {newJobs.Count} jobs, {newLocations.Count} locations, {newUrls.Count} URLs");
        return totalInserted;
    }

    private class ProcessedJob
    {
        public string Hash { get; set; } = string.Empty;
        public Job Job { get; set; } = null!;
        public JobLocation Location { get; set; } = null!;
        public JobLocationUrl Url { get; set; } = null!;
        public string LocationString { get; set; } = string.Empty;
        public string UrlString { get; set; } = string.Empty;
    }

    private static ProcessedJob? MapToProcessedJob(JsonlJob jsonJob, Guid fileId)
    {
        // Validate: Must have URL and description
        if (string.IsNullOrWhiteSpace(jsonJob.Url))
            return null;

        if (string.IsNullOrWhiteSpace(jsonJob.Text))
            return null;

        var hash = ComputeMd5Hash(jsonJob.Text)!;
        var jobId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var job = new Job
        {
            Id = jobId,
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
            JobDescription = jsonJob.Text,
            JobDescriptionHash = hash,
            DatePosted = ParseDate(jsonJob.Json?.SchemaOrg?.DatePosted ?? jsonJob.Json?.JsonLD?.DatePosted),
            EmploymentType = Truncate(jsonJob.Json?.SchemaOrg?.EmploymentType
                            ?? jsonJob.Json?.JsonLD?.EmploymentType, 100),
            CompanyName = Truncate(jsonJob.Company?.Name, 500),
            CompanyUrl = Truncate(jsonJob.Company?.Info?.CareerPageURL, 1000),
            ValidThrough = ParseDate(jsonJob.Json?.SchemaOrg?.ValidThrough ?? jsonJob.Json?.JsonLD?.ValidThrough),
            WorkplaceType = null
        };

        var locationString = Truncate(jsonJob.Location?.OrgAddress?.AddressLine, 500) ?? string.Empty;

        var location = new JobLocation
        {
            Id = locationId,
            JobId = jobId,
            Location = locationString,
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
                        ?? jsonJob.Json?.JsonLD?.JobLocation?.Longitude
        };

        var url = new JobLocationUrl
        {
            Id = Guid.NewGuid(),
            JobLocationId = locationId,
            Url = Truncate(jsonJob.Url, 1000)!
        };

        return new ProcessedJob
        {
            Hash = hash,
            Job = job,
            Location = location,
            Url = url,
            LocationString = locationString,
            UrlString = url.Url
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

    private static string? ComputeMd5Hash(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
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

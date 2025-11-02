using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.LocationBatchGenerate;

public class LocationBatchData
{
    public Guid Id { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Location { get; set; }
}

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private const int BatchSize = 25000;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _bucketName = "circuitdreams-nl-jobsearch-api";
    }

    // Constructor for testing
    public Function(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    /// <summary>
    /// Lambda handler for generating location batch files
    /// Triggered by ProcessLlmResults Lambda
    /// </summary>
    public async Task FunctionHandler(ILambdaContext context)
    {
        context.Logger.LogInformation("=== Location Batch Generation Started ===");

        await using var db = JobContext.Create();

        // Phase 1: Load location lookups
        var lookups = await db.LocationLookups.ToListAsync();
        var lookupDict = new Dictionary<string, LocationLookup>(StringComparer.OrdinalIgnoreCase);
        foreach (var lookup in lookups)
        {
            lookupDict[lookup.LocationText] = lookup;
        }
        context.Logger.LogInformation($"Loaded {lookupDict.Count} location lookup(s) from database");

        // Query jobs: status = 'workplace_classified', GeneratedCity = null, IsValid = true
        var allJobs = await db.Jobs
            .Where(j => j.Status == "workplace_classified" && j.GeneratedCity == null && j.IsValid == true)
            .ToListAsync();

        if (allJobs.Count == 0)
        {
            context.Logger.LogInformation("No jobs need location normalization");
            return;
        }

        context.Logger.LogInformation($"Found {allJobs.Count} jobs needing location normalization");

        // Separate jobs: lookup matches vs. needs LLM
        var lookupMatchedJobs = new List<Job>();
        var jobsNeedingLLM = new List<LocationBatchData>();

        foreach (var job in allJobs)
        {
            if (!string.IsNullOrEmpty(job.Location) && lookupDict.TryGetValue(job.Location, out var lookup))
            {
                // Found a lookup match - update job directly
                job.GeneratedCity = lookup.City;
                job.GeneratedState = lookup.State;
                job.GeneratedCountry = lookup.Country;
                job.Status = "location_classified";
                lookupMatchedJobs.Add(job);
            }
            else
            {
                // No lookup match - needs LLM processing
                jobsNeedingLLM.Add(new LocationBatchData
                {
                    Id = job.Id,
                    Locality = job.Locality,
                    Region = job.Region,
                    Country = job.Country,
                    Location = job.Location
                });
            }
        }

        context.Logger.LogInformation($"Matched {lookupMatchedJobs.Count} job(s) via lookup table");
        context.Logger.LogInformation($"{jobsNeedingLLM.Count} job(s) need LLM processing");

        // Save lookup-matched jobs
        if (lookupMatchedJobs.Count > 0)
        {
            await db.SaveChangesAsync();
            context.Logger.LogInformation($"Updated {lookupMatchedJobs.Count} job(s) to 'location_classified' via lookup");
        }

        // Process jobs that need LLM
        if (jobsNeedingLLM.Count == 0)
        {
            context.Logger.LogInformation("=== Location Batch Generation Complete ===");
            return;
        }

        // Create timestamp for batch naming
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        // Split into batches of 25k
        var batches = jobsNeedingLLM
            .Select((job, index) => new { job, index })
            .GroupBy(x => x.index / BatchSize)
            .Select(g => g.Select(x => x.job).ToList())
            .ToList();

        context.Logger.LogInformation($"Creating {batches.Count} batch file(s)");

        for (int batchNum = 0; batchNum < batches.Count; batchNum++)
        {
            var batch = batches[batchNum];
            var fileName = $"location_batch_{timestamp}_{batchNum + 1}.jsonl";
            var s3Key = $"location/locationbatch/{fileName}";

            context.Logger.LogInformation($"Generating batch {batchNum + 1}/{batches.Count}: {fileName} ({batch.Count} jobs)");

            // Generate batch file content
            var batchContent = GenerateBatchFileContent(batch);

            // Upload to S3
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(batchContent));
            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                InputStream = stream
            });

            context.Logger.LogInformation($"  Uploaded to s3://{_bucketName}/{s3Key}");

            // Create LocationBatch tracking record
            var locationBatch = new LocationBatch
            {
                Id = Guid.NewGuid(),
                BatchFilePath = s3Key,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await db.LocationBatches.AddAsync(locationBatch);
        }

        // Update jobs' status to 'location_batches_generated'
        var jobIds = jobsNeedingLLM.Select(j => j.Id).ToList();
        var jobsToUpdate = await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToListAsync();
        foreach (var job in jobsToUpdate)
        {
            job.Status = "location_batches_generated";
        }

        await db.SaveChangesAsync();
        context.Logger.LogInformation($"Updated {jobsToUpdate.Count} job(s) status to 'location_batches_generated'");

        context.Logger.LogInformation("=== Location Batch Generation Complete ===");
    }

    private string GenerateBatchFileContent(List<LocationBatchData> jobs)
    {
        var sb = new StringBuilder();
        foreach (var job in jobs)
        {
            var batchRequest = CreateBatchRequest(job);
            var json = JsonSerializer.Serialize(batchRequest);
            sb.AppendLine(json);
        }
        return sb.ToString();
    }

    private object CreateBatchRequest(LocationBatchData job)
    {
        // Build location context
        var locationParts = new List<string>();
        if (!string.IsNullOrEmpty(job.Locality)) locationParts.Add(job.Locality);
        if (!string.IsNullOrEmpty(job.Region)) locationParts.Add(job.Region);
        if (!string.IsNullOrEmpty(job.Country)) locationParts.Add(job.Country);
        var locationContext = locationParts.Count > 0
            ? string.Join(", ", locationParts)
            : job.Location ?? "Not specified";

        var systemPrompt = @"You are a location normalizer for US job postings. Extract the city, state, and country from the location string.

Respond with ONLY a JSON object in this format:
{""city"":""CityName"",""state"":""XX"",""country"":""US""}

Rules:
- city: Extract the city name if present. For metro areas like ""San Francisco Bay Area"" or ""Hampton Roads"", extract the primary city (e.g., ""San Francisco"", ""Norfolk""). Only set to null if truly vague like ""Remote"", ""USA"", or state-only.
- state: 2-letter state code (e.g., ""CA"", ""TX"", ""NY"", ""DC""). Use ""DC"" for Washington D.C. Return null if not a US location.
- country: ""US"" for United States jobs, null otherwise.
- Handle common formats: ""City, State"", ""City, ST"", ""City, State, USA""
- For Washington D.C., use city=""Washington"" and state=""DC"", not ""WA""
- Extract city from localities, even if they include the state name (e.g., ""Oklahoma City, Oklahoma"" → city=""Oklahoma City"")
- Be generous in extraction - prefer extracting a city over returning null";

        return new
        {
            custom_id = $"job_{job.Id}",
            method = "POST",
            url = "/v1/chat/completions",
            body = new
            {
                model = "gpt-4o-mini-2024-07-18",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"Location: {locationContext}" }
                },
                max_completion_tokens = 2000,
                response_format = new { type = "json_object" }
            }
        };
    }
}

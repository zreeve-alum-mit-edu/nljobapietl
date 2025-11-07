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
    public Guid Id { get; set; } // job_location.Id
    public Guid JobId { get; set; }
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

        // Query job_locations: job.status = 'workplace_classified', job_location.GeneratedCity = null, job.IsValid = true
        var allLocations = await db.JobLocations
            .Include(jl => jl.Job)
            .Where(jl => jl.Job!.Status == "workplace_classified" && jl.GeneratedCity == null && jl.Job.IsValid == true)
            .ToListAsync();

        if (allLocations.Count == 0)
        {
            context.Logger.LogInformation("No job locations need location normalization");
            return;
        }

        context.Logger.LogInformation($"Found {allLocations.Count} job locations needing location normalization");

        // Separate locations: lookup matches vs. needs LLM
        var lookupMatchedLocations = new List<JobLocation>();
        var locationsNeedingLLM = new List<LocationBatchData>();

        foreach (var location in allLocations)
        {
            if (!string.IsNullOrEmpty(location.Location) &&
                lookupDict.TryGetValue(location.Location, out var lookup) &&
                lookup.Confidence >= 10)
            {
                // Found a lookup match with sufficient confidence - update location directly
                location.GeneratedCity = lookup.City;
                location.GeneratedState = lookup.State;
                location.GeneratedCountry = lookup.Country;
                lookupMatchedLocations.Add(location);
            }
            else
            {
                // No lookup match or insufficient confidence - needs LLM processing
                locationsNeedingLLM.Add(new LocationBatchData
                {
                    Id = location.Id,
                    JobId = location.JobId,
                    Locality = location.Locality,
                    Region = location.Region,
                    Country = location.Country,
                    Location = location.Location
                });
            }
        }

        context.Logger.LogInformation($"Matched {lookupMatchedLocations.Count} location(s) via lookup table");
        context.Logger.LogInformation($"{locationsNeedingLLM.Count} location(s) need LLM processing");

        // Save lookup-matched locations and update job status if all locations are classified
        if (lookupMatchedLocations.Count > 0)
        {
            await db.SaveChangesAsync();

            // Check which jobs have all their locations classified
            var jobsWithMatchedLocations = lookupMatchedLocations.Select(l => l.JobId).Distinct().ToList();
            foreach (var jobId in jobsWithMatchedLocations)
            {
                var allJobLocations = await db.JobLocations.Where(jl => jl.JobId == jobId).ToListAsync();
                if (allJobLocations.All(jl => jl.GeneratedCity != null))
                {
                    var job = await db.Jobs.FindAsync(jobId);
                    if (job != null)
                    {
                        job.Status = "location_classified";
                    }
                }
            }
            await db.SaveChangesAsync();
            context.Logger.LogInformation($"Updated {lookupMatchedLocations.Count} location(s) via lookup");
        }

        // Process locations that need LLM
        if (locationsNeedingLLM.Count == 0)
        {
            context.Logger.LogInformation("=== Location Batch Generation Complete ===");
            return;
        }

        // Create timestamp for batch naming
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        // Split into batches of 25k
        var batches = locationsNeedingLLM
            .Select((location, index) => new { location, index })
            .GroupBy(x => x.index / BatchSize)
            .Select(g => g.Select(x => x.location).ToList())
            .ToList();

        context.Logger.LogInformation($"Creating {batches.Count} batch file(s)");

        for (int batchNum = 0; batchNum < batches.Count; batchNum++)
        {
            var batch = batches[batchNum];
            var fileName = $"location_batch_{timestamp}_{batchNum + 1}.jsonl";
            var s3Key = $"location/locationbatch/{fileName}";

            context.Logger.LogInformation($"Generating batch {batchNum + 1}/{batches.Count}: {fileName} ({batch.Count} locations)");

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

        // Update status of jobs that have locations in batch to 'location_batches_generated'
        var jobIds = locationsNeedingLLM.Select(l => l.JobId).Distinct().ToList();
        var jobsToUpdate = await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToListAsync();
        foreach (var job in jobsToUpdate)
        {
            job.Status = "location_batches_generated";
        }

        await db.SaveChangesAsync();
        context.Logger.LogInformation($"Updated {jobsToUpdate.Count} job(s) status to 'location_batches_generated'");

        context.Logger.LogInformation("=== Location Batch Generation Complete ===");
    }

    private string GenerateBatchFileContent(List<LocationBatchData> locations)
    {
        var sb = new StringBuilder();
        foreach (var location in locations)
        {
            var batchRequest = CreateBatchRequest(location);
            var json = JsonSerializer.Serialize(batchRequest);
            sb.AppendLine(json);
        }
        return sb.ToString();
    }

    private object CreateBatchRequest(LocationBatchData location)
    {
        // Build location context
        var locationParts = new List<string>();
        if (!string.IsNullOrEmpty(location.Locality)) locationParts.Add(location.Locality);
        if (!string.IsNullOrEmpty(location.Region)) locationParts.Add(location.Region);
        if (!string.IsNullOrEmpty(location.Country)) locationParts.Add(location.Country);
        var locationContext = locationParts.Count > 0
            ? string.Join(", ", locationParts)
            : location.Location ?? "Not specified";

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
            custom_id = $"location_{location.Id}",
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

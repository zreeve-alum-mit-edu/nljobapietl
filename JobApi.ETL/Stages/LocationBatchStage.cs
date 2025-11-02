using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using FileEntity = JobApi.Common.Entities.File;

namespace JobApi.ETL.Stages;

public class LocationBatchData
{
    public Guid Id { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Location { get; set; }
}

public class LocationBatchStage
{
    private readonly string _locationBatchFolder;
    private const int BatchSize = 25000;

    public LocationBatchStage(string dataRootPath)
    {
        _locationBatchFolder = Path.Combine(dataRootPath, "locationbatch");
        Directory.CreateDirectory(_locationBatchFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LOCATION BATCH GENERATION STAGE ===");

        using var db = JobContext.Create();

        // Load location lookups once (case-insensitive dictionary)
        var lookups = await db.LocationLookups.ToListAsync();
        var lookupDict = new Dictionary<string, LocationLookup>(StringComparer.OrdinalIgnoreCase);
        foreach (var lookup in lookups)
        {
            lookupDict[lookup.LocationText] = lookup;
        }
        Console.WriteLine($"Loaded {lookupDict.Count} location lookup(s) from database");

        // Get distinct file IDs for jobs with status = 'workplace_classified' and no generated_city
        var fileIds = await db.Jobs
            .Where(j => j.Status == "workplace_classified" && j.GeneratedCity == null)
            .Select(j => j.FileId)
            .Distinct()
            .ToListAsync();

        if (fileIds.Count == 0)
        {
            Console.WriteLine("No jobs with status 'workplace_classified' needing location normalization.");
            return true;
        }

        Console.WriteLine($"Found jobs from {fileIds.Count} file(s) needing location normalization");

        foreach (var fileId in fileIds)
        {
            var file = await db.Files.FindAsync(fileId);
            Console.WriteLine($"\nProcessing file: {file?.Filename ?? "Unknown"} (ID: {fileId})");

            try
            {
                await ProcessFile(db, fileId, lookupDict);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR processing file {fileId}: {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("\n=== Location Batch Generation Complete ===");
        return true;
    }

    private async Task ProcessFile(JobContext db, Guid fileId, Dictionary<string, LocationLookup> lookupDict)
    {
        // Get jobs with status = 'workplace_classified' and without generated_city
        var allJobs = await db.Jobs
            .Where(j => j.FileId == fileId && j.Status == "workplace_classified" && j.GeneratedCity == null)
            .ToListAsync();

        if (allJobs.Count == 0)
        {
            Console.WriteLine("  No jobs need location normalization");
            return;
        }

        Console.WriteLine($"  Found {allJobs.Count} jobs needing location normalization");

        // Check lookups and separate jobs
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

        Console.WriteLine($"  Matched {lookupMatchedJobs.Count} job(s) via lookup table");
        Console.WriteLine($"  {jobsNeedingLLM.Count} job(s) need LLM processing");

        // Save lookup-matched jobs
        if (lookupMatchedJobs.Count > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"  Updated {lookupMatchedJobs.Count} job(s) via lookup to 'location_classified'");
        }

        // Process jobs that need LLM
        if (jobsNeedingLLM.Count == 0)
        {
            return;
        }

        // Create batches for LLM processing
        var batches = jobsNeedingLLM
            .Select((job, index) => new { job, index })
            .GroupBy(x => x.index / BatchSize)
            .Select(g => g.Select(x => x.job).ToList())
            .ToList();

        Console.WriteLine($"  Creating {batches.Count} batch file(s)");

        for (int batchNum = 0; batchNum < batches.Count; batchNum++)
        {
            var batch = batches[batchNum];
            var fileName = $"location_batch_{fileId}_{batchNum + 1}.jsonl";
            var filePath = Path.Combine(_locationBatchFolder, fileName);

            Console.WriteLine($"  Generating batch {batchNum + 1}/{batches.Count}: {fileName} ({batch.Count} jobs)");

            await GenerateBatchFile(batch, filePath);

            // Create tracking record for this batch
            var locationBatch = new LocationBatch
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                BatchFilePath = filePath,
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
        Console.WriteLine($"  Updated {jobsToUpdate.Count} job(s) status to 'location_batches_generated'");
    }

    private async Task GenerateBatchFile(List<LocationBatchData> jobs, string filePath)
    {
        using var writer = new StreamWriter(filePath);

        foreach (var job in jobs)
        {
            var batchRequest = CreateBatchRequest(job);
            var json = JsonSerializer.Serialize(batchRequest);
            await writer.WriteLineAsync(json);
        }
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
                model = "gpt-5-nano",
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

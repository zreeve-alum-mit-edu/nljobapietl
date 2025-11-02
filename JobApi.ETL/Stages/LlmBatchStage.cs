using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using FileEntity = JobApi.Common.Entities.File;

namespace JobApi.ETL.Stages;

public class JobBatchData
{
    public Guid Id { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Location { get; set; }
    public string? JobDescription { get; set; }
}

public class LlmBatchStage
{
    private readonly string _llmBatchFolder;
    private const int BatchSize = 25000;
    private const int DescriptionMaxLength = 2000;

    public LlmBatchStage(string dataRootPath)
    {
        _llmBatchFolder = Path.Combine(dataRootPath, "llmbatch");
        Directory.CreateDirectory(_llmBatchFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LLM BATCH GENERATION STAGE ===");

        using var db = JobContext.Create();

        // Get distinct file IDs for jobs with status = 'ingested' and no generated_workplace
        var fileIds = await db.Jobs
            .Where(j => j.Status == "ingested" && j.GeneratedWorkplace == null)
            .Select(j => j.FileId)
            .Distinct()
            .ToListAsync();

        if (fileIds.Count == 0)
        {
            Console.WriteLine("No jobs with status 'ingested' needing workplace classification.");
            return true;
        }

        Console.WriteLine($"Found jobs from {fileIds.Count} file(s) needing classification");

        foreach (var fileId in fileIds)
        {
            var file = await db.Files.FindAsync(fileId);
            Console.WriteLine($"\nProcessing file: {file?.Filename ?? "Unknown"} (ID: {fileId})");

            try
            {
                await ProcessFile(db, fileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR processing file {fileId}: {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("\n=== LLM Batch Generation Complete ===");
        return true;
    }

    private async Task ProcessFile(JobContext db, Guid fileId)
    {
        // Get jobs with status = 'ingested' and without generated_workplace
        var jobs = await db.Jobs
            .Where(j => j.FileId == fileId && j.Status == "ingested" && j.GeneratedWorkplace == null)
            .Select(j => new JobBatchData
            {
                Id = j.Id,
                JobTitle = j.JobTitle,
                CompanyName = j.CompanyName,
                Locality = j.Locality,
                Region = j.Region,
                Country = j.Country,
                Location = j.Location,
                JobDescription = j.JobDescription
            })
            .ToListAsync();

        if (jobs.Count == 0)
        {
            Console.WriteLine("  No jobs need workplace classification");
            return;
        }

        Console.WriteLine($"  Found {jobs.Count} jobs needing classification");

        // Create batches
        var batches = jobs
            .Select((job, index) => new { job, index })
            .GroupBy(x => x.index / BatchSize)
            .Select(g => g.Select(x => x.job).ToList())
            .ToList();

        Console.WriteLine($"  Creating {batches.Count} batch file(s)");

        for (int batchNum = 0; batchNum < batches.Count; batchNum++)
        {
            var batch = batches[batchNum];
            var fileName = $"workplace_batch_{fileId}_{batchNum + 1}.jsonl";
            var filePath = Path.Combine(_llmBatchFolder, fileName);

            Console.WriteLine($"  Generating batch {batchNum + 1}/{batches.Count}: {fileName} ({batch.Count} jobs)");

            await GenerateBatchFile(batch, filePath);

            // Create tracking record for this batch
            var workplaceBatch = new WorkplaceBatch
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                BatchFilePath = filePath,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await db.WorkplaceBatches.AddAsync(workplaceBatch);
        }

        // Update all jobs' status to 'llm_batches_generated'
        var jobIds = jobs.Select(j => j.Id).ToList();
        var jobsToUpdate = await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToListAsync();
        foreach (var job in jobsToUpdate)
        {
            job.Status = "llm_batches_generated";
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"  Updated {jobsToUpdate.Count} job(s) status to 'llm_batches_generated'");
    }

    private async Task GenerateBatchFile(List<JobBatchData> jobs, string filePath)
    {
        using var writer = new StreamWriter(filePath);

        foreach (var job in jobs)
        {
            var batchRequest = CreateBatchRequest(job);
            var json = JsonSerializer.Serialize(batchRequest);
            await writer.WriteLineAsync(json);
        }
    }

    private object CreateBatchRequest(JobBatchData job)
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

        var systemPrompt = @"You are a workplace type classifier. Analyze the job posting and determine the workplace type.

Respond with ONLY a JSON object in this format:
{""type"":""REMOTE|HYBRID|ONSITE"",""inferred"":true|false,""confidence"":""EXPLICIT|LIKELY|PROBABLY|GUESS""}

- type: REMOTE, HYBRID, or ONSITE
- inferred: true if the workplace type is not explicitly stated, false if it is clearly stated
- confidence: EXPLICIT if clearly stated, LIKELY if strong indicators, PROBABLY if moderate indicators, GUESS if weak indicators";

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
                    new { role = "user", content = userMessage }
                },
                max_completion_tokens = 2000,
                response_format = new { type = "json_object" }
            }
        };
    }
}

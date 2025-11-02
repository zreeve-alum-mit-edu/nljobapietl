using System.Text.Json;
using System.Text.Json.Serialization;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace JobApi.ETL.Stages;

public class EmbeddingResultsStage
{
    private readonly string _embeddingResultFolder;

    public EmbeddingResultsStage(string dataRootPath)
    {
        _embeddingResultFolder = Path.Combine(dataRootPath, "embeddingresult");
        Directory.CreateDirectory(_embeddingResultFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== EMBEDDING RESULTS PROCESSING STAGE ===");

        var resultFiles = Directory.GetFiles(_embeddingResultFolder, "*.jsonl");

        if (resultFiles.Length == 0)
        {
            Console.WriteLine("No result files to process.");
            return true;
        }

        Console.WriteLine($"Found {resultFiles.Length} result file(s) to process");

        foreach (var filePath in resultFiles)
        {
            Console.WriteLine($"\nProcessing: {Path.GetFileName(filePath)}");

            try
            {
                // Create a fresh DbContext for each file to avoid connection/state issues
                using var db = JobContext.Create();
                await ProcessResultFile(db, filePath);

                // Delete the file after successful processing
                System.IO.File.Delete(filePath);
                Console.WriteLine($"  Deleted result file: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR processing {Path.GetFileName(filePath)}: {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("\n=== Embedding Results Processing Complete ===");
        return true;
    }

    private async Task ProcessResultFile(JobContext db, string filePath)
    {
        var successCount = 0;
        var errorCount = 0;
        var lineNumber = 0;
        var batchSize = 1000;
        var updates = new List<(Guid jobId, Vector embedding)>();

        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                // Parse JSON (no retry needed - parsing doesn't have transient failures)
                var batchResponse = JsonSerializer.Deserialize<EmbeddingBatchResult>(line);
                if (batchResponse == null) continue;

                // Get the custom_id which contains the job ID
                var customId = batchResponse.CustomId;
                var jobIdString = customId.Replace("job_", "");
                var jobId = Guid.Parse(jobIdString);

                // Check if the response has an error
                if (batchResponse.Error != null)
                {
                    Console.WriteLine($"  Line {lineNumber}: Error for job {jobId}: {batchResponse.Error.Message}");
                    errorCount++;
                    continue;
                }

                // Check if request was successful
                if (batchResponse.Response?.StatusCode != 200)
                {
                    Console.WriteLine($"  Line {lineNumber}: Error for job {jobId}: Status {batchResponse.Response?.StatusCode}");
                    errorCount++;
                    continue;
                }

                // Extract embedding vector
                var embeddingData = batchResponse.Response?.Body?.Data?.FirstOrDefault()?.Embedding;
                if (embeddingData == null || embeddingData.Length != 1536)
                {
                    Console.WriteLine($"  Line {lineNumber}: Invalid embedding for job {jobId}");
                    errorCount++;
                    continue;
                }

                // Convert to pgvector Vector type
                var vector = new Vector(embeddingData);
                updates.Add((jobId, vector));

                // Batch update
                if (updates.Count >= batchSize)
                {
                    try
                    {
                        await UpdateEmbeddings(updates);
                        successCount += updates.Count;
                        Console.WriteLine($"  Updated {successCount} embeddings (line {lineNumber})...");
                        updates.Clear();
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"  FATAL: Database update failed after all retries at line {lineNumber}: {dbEx.Message}");
                        throw; // Fail the file immediately
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"  Line {lineNumber}: JSON parsing error: {jsonEx.Message}");
                errorCount++;
            }
            catch (Exception ex)
            {
                // This will catch the rethrown database exception
                throw;
            }
        }

        // Update remaining embeddings
        if (updates.Count > 0)
        {
            await UpdateEmbeddings(updates);
            successCount += updates.Count;
            Console.WriteLine($"  Updated final batch. Total: {successCount} embeddings");
        }

        Console.WriteLine($"  Processed {successCount} job(s) successfully");
        if (errorCount > 0)
        {
            Console.WriteLine($"  {errorCount} error(s) encountered");
        }
    }

    private async Task UpdateEmbeddings(List<(Guid jobId, Vector embedding)> updates)
    {
        // Create a fresh DbContext for this batch (don't reuse on retry)
        await using var db = JobContext.Create();

        // Use EF Core's execution strategy for transient retry handling
        // Don't use manual transactions - the execution strategy handles that
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Load all jobs in a single query, filtering to only those without embeddings
            var jobIds = updates.Select(u => u.jobId).ToList();
            var jobs = await db.Jobs
                .Where(j => jobIds.Contains(j.Id) && j.Embedding == null)
                .ToDictionaryAsync(j => j.Id);

            // Apply updates only to jobs that don't have embeddings yet
            foreach (var (jobId, embedding) in updates)
            {
                if (jobs.TryGetValue(jobId, out var job))
                {
                    job.Embedding = embedding;
                    job.Status = "embedded";
                }
            }

            // Only save if there are actual changes
            if (jobs.Count > 0)
            {
                await db.SaveChangesAsync();
            }
        });
    }
}

// JSON structure classes for batch results
public class EmbeddingBatchResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public EmbeddingBatchResponse? Response { get; set; }

    [JsonPropertyName("error")]
    public EmbeddingBatchError? Error { get; set; }
}

public class EmbeddingBatchResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("body")]
    public EmbeddingResponseBody? Body { get; set; }
}

public class EmbeddingResponseBody
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("data")]
    public List<EmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public EmbeddingUsageData? Usage { get; set; }
}

public class EmbeddingData
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class EmbeddingUsageData
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class EmbeddingBatchError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using JobApi.Common;
using JobApi.Common.Entities;
using Pgvector;

namespace JobApi.ETL;

public class EmbeddingResultsProcessor
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run process-results <batch-results-file>");
            return 1;
        }

        var filePath = args[0];
        if (!System.IO.File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return 1;
        }

        Console.WriteLine("Loading environment variables...");
        Env.Load();

        Console.WriteLine($"Processing batch results from: {filePath}");
        await ProcessResults(filePath);
        return 0;
    }

    private static async Task ProcessResults(string filePath)
    {
        using var db = JobContext.Create();

        var batchSize = 100;
        var updates = new List<(Guid jobId, Vector embedding)>();
        var lineCount = 0;
        var totalUpdated = 0;
        var errorCount = 0;

        Console.WriteLine("Streaming and parsing results file...");

        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineCount++;

            try
            {
                var result = JsonSerializer.Deserialize<BatchResult>(line);
                if (result == null) continue;

                // Extract job GUID from custom_id (format: "job-{guid}")
                var jobId = Guid.Parse(result.CustomId.Replace("job-", ""));

                // Check if request was successful
                if (result.Response?.StatusCode != 200)
                {
                    Console.WriteLine($"Error for job {jobId}: Status {result.Response?.StatusCode}");
                    errorCount++;
                    continue;
                }

                // Extract embedding vector
                var embeddingData = result.Response?.Body?.Data?.FirstOrDefault()?.Embedding;
                if (embeddingData == null || embeddingData.Length != 1536)
                {
                    Console.WriteLine($"Invalid embedding for job {jobId}");
                    errorCount++;
                    continue;
                }

                // Convert to pgvector Vector type
                var vector = new Vector(embeddingData);
                updates.Add((jobId, vector));

                // Batch update
                if (updates.Count >= batchSize)
                {
                    await UpdateEmbeddings(db, updates);
                    totalUpdated += updates.Count;
                    Console.WriteLine($"Updated {totalUpdated} embeddings (line {lineCount})...");
                    updates.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on line {lineCount}: {ex.Message}");
                errorCount++;
            }
        }

        // Update remaining embeddings
        if (updates.Count > 0)
        {
            await UpdateEmbeddings(db, updates);
            totalUpdated += updates.Count;
            Console.WriteLine($"Updated final batch. Total: {totalUpdated} embeddings");
        }

        Console.WriteLine($"\n✅ Processing complete!");
        Console.WriteLine($"   Total updated: {totalUpdated}");
        Console.WriteLine($"   Errors: {errorCount}");
    }

    private static async Task UpdateEmbeddings(JobContext db, List<(Guid jobId, Vector embedding)> updates)
    {
        foreach (var (jobId, embedding) in updates)
        {
            var job = await db.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Embedding = embedding;
            }
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }
}

// JSON structure classes for batch results
public class BatchResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public BatchResponse? Response { get; set; }
}

public class BatchResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("body")]
    public BatchResponseBody? Body { get; set; }
}

public class BatchResponseBody
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("data")]
    public List<EmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public UsageData? Usage { get; set; }
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

public class UsageData
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

using System.Net.Http.Headers;
using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Create Embedding Batch Records ===\n");

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                     ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        Console.WriteLine("Fetching batches from OpenAI...");
        var batches = await GetAllOpenAiBatches(httpClient);
        Console.WriteLine($"Found {batches.Count} total batches on OpenAI\n");

        // Filter for embedding batches matching our pattern: embedding_batch_*_part_*.jsonl
        var embeddingBatches = batches.Where(b =>
        {
            if (b.TryGetProperty("metadata", out var metadata) &&
                metadata.ValueKind == JsonValueKind.Object &&
                metadata.TryGetProperty("description", out var desc))
            {
                var description = desc.GetString();
                return description != null &&
                       description.StartsWith("embedding_batch_") &&
                       description.Contains("_part_") &&
                       description.EndsWith(".jsonl");
            }
            return false;
        }).ToList();

        Console.WriteLine($"Found {embeddingBatches.Count} embedding batch files\n");

        if (embeddingBatches.Count == 0)
        {
            Console.WriteLine("No embedding batches to process.");
            return;
        }

        using var db = JobContext.Create();

        // Get existing batch records to avoid duplicates
        var existingBatchIdsList = await db.EmbeddingBatches
            .Select(b => b.OpenAiBatchId)
            .ToListAsync();

        var existingBatchIds = new HashSet<string?>(existingBatchIdsList);

        Console.WriteLine($"Found {existingBatchIds.Count} existing database records\n");

        var createdCount = 0;
        var skippedCount = 0;

        foreach (var batch in embeddingBatches)
        {
            var batchId = batch.GetProperty("id").GetString()!;
            var inputFileId = batch.GetProperty("input_file_id").GetString()!;
            var status = batch.GetProperty("status").GetString()!;
            var createdAt = batch.GetProperty("created_at").GetInt64();
            var metadata = batch.GetProperty("metadata");
            var fileName = metadata.GetProperty("description").GetString()!;

            if (existingBatchIds.Contains(batchId))
            {
                Console.WriteLine($"SKIP: {fileName} - already in database");
                skippedCount++;
                continue;
            }

            var embeddingBatch = new EmbeddingBatch
            {
                Id = Guid.NewGuid(),
                BatchFilePath = fileName,
                OpenAiBatchId = batchId,
                OpenAiInputFileId = inputFileId,
                Status = "submitted", // Mark as submitted so stage 13 can track it
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(createdAt).UtcDateTime,
                SubmittedAt = DateTimeOffset.FromUnixTimeSeconds(createdAt).UtcDateTime
            };

            db.EmbeddingBatches.Add(embeddingBatch);
            Console.WriteLine($"CREATE: {fileName} (OpenAI status: {status})");
            createdCount++;
        }

        if (createdCount > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"\n✓ Created {createdCount} database record(s)");
        }

        if (skippedCount > 0)
        {
            Console.WriteLine($"✓ Skipped {skippedCount} existing record(s)");
        }

        Console.WriteLine("\n=== Complete ===");
    }

    static async Task<List<JsonElement>> GetAllOpenAiBatches(HttpClient httpClient)
    {
        var batches = new List<JsonElement>();
        var hasMore = true;
        string? afterId = null;

        while (hasMore)
        {
            var url = "https://api.openai.com/v1/batches?limit=100";
            if (afterId != null)
            {
                url += $"&after={afterId}";
            }

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var data = result.GetProperty("data");
            foreach (var batch in data.EnumerateArray())
            {
                batches.Add(batch);
            }

            hasMore = result.GetProperty("has_more").GetBoolean();
            if (hasMore && data.GetArrayLength() > 0)
            {
                var lastBatch = data[data.GetArrayLength() - 1];
                afterId = lastBatch.GetProperty("id").GetString();
            }
            else
            {
                hasMore = false;
            }
        }

        return batches;
    }
}

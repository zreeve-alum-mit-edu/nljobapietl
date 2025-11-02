using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using FileEntity = JobApi.Common.Entities.File;

namespace JobApi.ETL.Stages;

public class EmbeddingBatchData
{
    public Guid Id { get; set; }
    public string? JobTitle { get; set; }
    public string? JobDescription { get; set; }
}

public class EmbeddingBatchStage
{
    private readonly string _embeddingBatchFolder;
    private const int BatchSize = 20000;
    private const int MaxInputLength = 32000; // ~8000 tokens for text-embedding-3-small

    public EmbeddingBatchStage(string dataRootPath)
    {
        _embeddingBatchFolder = Path.Combine(dataRootPath, "embeddingbatch");
        Directory.CreateDirectory(_embeddingBatchFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== EMBEDDING BATCH GENERATION STAGE ===");

        using var db = JobContext.Create();

        // Count total jobs needing embeddings
        var totalCount = await db.Jobs
            .Where(j => j.Embedding == null &&
                        j.IsValid == true &&
                        j.GeneratedCountry == "US")
            .CountAsync();

        if (totalCount == 0)
        {
            Console.WriteLine("No valid US jobs needing embeddings.");
            return true;
        }

        Console.WriteLine($"Found {totalCount} valid US jobs needing embeddings");

        var estimatedBatches = (int)Math.Ceiling((double)totalCount / BatchSize);
        Console.WriteLine($"Creating ~{estimatedBatches} batch file(s) (batch size: {BatchSize})");

        // Process in chunks to avoid loading everything into memory
        var processedCount = 0;
        var batchNum = 1;
        var currentBatchJobs = new List<EmbeddingBatchData>();

        while (processedCount < totalCount)
        {
            var chunkSize = 10000; // Load 10k at a time
            var chunk = await db.Jobs
                .Where(j => j.Embedding == null &&
                            j.IsValid == true &&
                            j.GeneratedCountry == "US")
                .OrderBy(j => j.Id)
                .Skip(processedCount)
                .Take(chunkSize)
                .Select(j => new EmbeddingBatchData
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    JobDescription = j.JobDescription
                })
                .ToListAsync();

            if (chunk.Count == 0)
                break;

            // Add to current batch
            currentBatchJobs.AddRange(chunk);

            // Write out complete batches
            while (currentBatchJobs.Count >= BatchSize)
            {
                var batchToWrite = currentBatchJobs.Take(BatchSize).ToList();
                currentBatchJobs.RemoveRange(0, BatchSize);

                var fileName = $"embedding_batch_{batchNum}.jsonl";
                var filePath = Path.Combine(_embeddingBatchFolder, fileName);

                Console.WriteLine($"Generating batch {batchNum}: {fileName} ({batchToWrite.Count} jobs)");
                await GenerateBatchFile(batchToWrite, filePath);
                batchNum++;
            }

            processedCount += chunk.Count;
            Console.WriteLine($"  Processed {processedCount}/{totalCount} jobs...");
        }

        // Write remaining jobs as final batch
        if (currentBatchJobs.Count > 0)
        {
            var fileName = $"embedding_batch_{batchNum}.jsonl";
            var filePath = Path.Combine(_embeddingBatchFolder, fileName);

            Console.WriteLine($"Generating batch {batchNum}: {fileName} ({currentBatchJobs.Count} jobs)");
            await GenerateBatchFile(currentBatchJobs, filePath);
        }

        Console.WriteLine("\n=== Embedding Batch Generation Complete ===");
        return true;
    }

    private async Task GenerateBatchFile(List<EmbeddingBatchData> jobs, string filePath)
    {
        using var writer = new StreamWriter(filePath);

        foreach (var job in jobs)
        {
            var batchRequest = CreateBatchRequest(job);
            var json = JsonSerializer.Serialize(batchRequest);
            await writer.WriteLineAsync(json);
        }
    }

    private object CreateBatchRequest(EmbeddingBatchData job)
    {
        // Combine job title and description
        var input = $"{job.JobTitle}\n\n{job.JobDescription}";

        // Truncate if too long (max ~8000 tokens for text-embedding-3-small)
        // Rough estimate: 1 token ≈ 4 chars, so ~32000 chars max
        if (input.Length > MaxInputLength)
        {
            input = input.Substring(0, MaxInputLength);
        }

        return new
        {
            custom_id = $"job_{job.Id}",
            method = "POST",
            url = "/v1/embeddings",
            body = new
            {
                model = "text-embedding-3-small",
                input = input,
                input_type = "document"
            }
        };
    }
}

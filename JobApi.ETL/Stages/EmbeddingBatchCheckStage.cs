using System.Net.Http.Headers;
using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class EmbeddingBatchCheckStage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _embeddingResultFolder;

    public EmbeddingBatchCheckStage(string dataRootPath)
    {
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        _embeddingResultFolder = Path.Combine(dataRootPath, "embeddingresult");
        Directory.CreateDirectory(_embeddingResultFolder);

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== EMBEDDING BATCH CHECK STAGE ===");

        using var db = JobContext.Create();

        // Get all submitted batches
        var submittedBatches = await db.EmbeddingBatches
            .Where(b => b.Status == "submitted")
            .ToListAsync();

        if (submittedBatches.Count == 0)
        {
            Console.WriteLine("No submitted batches to check.");
            return true;
        }

        Console.WriteLine($"Checking status of {submittedBatches.Count} submitted batch(es)");

        foreach (var batch in submittedBatches)
        {
            Console.WriteLine($"\nChecking batch: {batch.OpenAiBatchId}");

            try
            {
                var status = await CheckBatchStatus(batch.OpenAiBatchId!);
                Console.WriteLine($"  Status: {status.Status}");

                switch (status.Status)
                {
                    case "completed":
                        Console.WriteLine("  Batch completed! Downloading results...");
                        await DownloadResults(batch, status.OutputFileId!);
                        batch.Status = "completed";
                        batch.CompletedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        Console.WriteLine("  Results downloaded and batch marked complete");
                        break;

                    case "failed":
                    case "expired":
                    case "cancelled":
                        Console.WriteLine($"  Batch {status.Status}");
                        batch.Status = status.Status;
                        batch.ErrorMessage = $"Batch {status.Status}";
                        await db.SaveChangesAsync();
                        break;

                    case "validating":
                    case "in_progress":
                    case "finalizing":
                    case "cancelling":
                        Console.WriteLine($"  Batch still processing ({status.Status})");
                        break;

                    default:
                        Console.WriteLine($"  Unknown status: {status.Status}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Embedding Batch Check Complete ===");
        return true;
    }

    private async Task<BatchStatus> CheckBatchStatus(string batchId)
    {
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/batches/{batchId}");
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        var status = result.GetProperty("status").GetString()!;
        string? outputFileId = null;

        if (result.TryGetProperty("output_file_id", out var outputFileElement))
        {
            outputFileId = outputFileElement.GetString();
        }

        return new BatchStatus
        {
            Status = status,
            OutputFileId = outputFileId
        };
    }

    private async Task DownloadResults(EmbeddingBatch batch, string outputFileId)
    {
        const int maxRetries = 6;
        var retryDelayMs = 1000; // Start with 1 second

        var fileName = $"embedding_results_{batch.Id}_{Path.GetFileName(batch.BatchFilePath)}";
        var filePath = Path.Combine(_embeddingResultFolder, fileName);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}/content");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                await System.IO.File.WriteAllTextAsync(filePath, content);

                Console.WriteLine($"  Saved results to: {fileName}");
                return; // Success!
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"  Download failed (attempt {attempt}/{maxRetries}): {ex.Message}");
                Console.WriteLine($"  Retrying in {retryDelayMs}ms...");
                await Task.Delay(retryDelayMs);
                retryDelayMs *= 2; // Exponential backoff
            }
        }

        // If we get here, all retries failed
        throw new Exception($"Failed to download results after {maxRetries} attempts");
    }

    private class BatchStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? OutputFileId { get; set; }
    }
}

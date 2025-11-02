using System.Net.Http.Headers;
using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class LlmBatchCheckStage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _llmResultFolder;

    public LlmBatchCheckStage(string dataRootPath)
    {
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        _llmResultFolder = Path.Combine(dataRootPath, "llmresult");
        Directory.CreateDirectory(_llmResultFolder);

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LLM BATCH CHECK STAGE ===");

        using var db = JobContext.Create();

        // Get all submitted batches
        var submittedBatches = await db.WorkplaceBatches
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
                        if (string.IsNullOrEmpty(status.OutputFileId))
                        {
                            Console.WriteLine("  ERROR: No output file ID returned from OpenAI");
                            throw new Exception("No output file ID in completed batch");
                        }
                        Console.WriteLine($"  Output File ID: {status.OutputFileId}");
                        await DownloadResults(batch, status.OutputFileId!);

                        // Download error file if it exists
                        if (!string.IsNullOrEmpty(status.ErrorFileId))
                        {
                            Console.WriteLine($"  Error File ID: {status.ErrorFileId}");
                            await DownloadErrorFile(batch, status.ErrorFileId!);
                        }

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
                // Don't mark as failed yet - might be temporary network issue
            }
        }

        Console.WriteLine("\n=== LLM Batch Check Complete ===");
        return true;
    }

    private async Task<BatchStatus> CheckBatchStatus(string batchId)
    {
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/batches/{batchId}");
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"  Batch API Response: {responseJson}");
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        var status = result.GetProperty("status").GetString()!;
        string? outputFileId = null;
        string? errorFileId = null;

        if (result.TryGetProperty("output_file_id", out var outputFileElement))
        {
            outputFileId = outputFileElement.GetString();
        }

        if (result.TryGetProperty("error_file_id", out var errorFileElement))
        {
            errorFileId = errorFileElement.GetString();
        }

        return new BatchStatus
        {
            Status = status,
            OutputFileId = outputFileId,
            ErrorFileId = errorFileId
        };
    }

    private async Task DownloadResults(WorkplaceBatch batch, string outputFileId)
    {
        // Download the results file
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}/content");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Save to llmresult folder with a unique name
        var fileName = $"workplace_results_{batch.FileId}_{Path.GetFileName(batch.BatchFilePath)}";
        var filePath = Path.Combine(_llmResultFolder, fileName);

        await System.IO.File.WriteAllTextAsync(filePath, content);

        Console.WriteLine($"  Saved results to: {fileName}");
    }

    private async Task DownloadErrorFile(WorkplaceBatch batch, string errorFileId)
    {
        // Download the error file
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{errorFileId}/content");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Save to llmresult folder with a unique name indicating it's an error file
        var fileName = $"workplace_errors_{batch.FileId}_{Path.GetFileName(batch.BatchFilePath)}";
        var filePath = Path.Combine(_llmResultFolder, fileName);

        await System.IO.File.WriteAllTextAsync(filePath, content);

        Console.WriteLine($"  Saved error file to: {fileName}");
    }

    private class BatchStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? OutputFileId { get; set; }
        public string? ErrorFileId { get; set; }
    }
}

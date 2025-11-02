using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class LlmBatchSubmitStage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const int MaxBatchesInFlight = 2;

    public LlmBatchSubmitStage()
    {
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LLM BATCH SUBMIT STAGE ===");

        using var db = JobContext.Create();

        // Check how many batches are currently in flight
        var inFlightCount = await db.WorkplaceBatches
            .CountAsync(b => b.Status == "submitted");

        Console.WriteLine($"Batches currently in flight: {inFlightCount}/{MaxBatchesInFlight}");

        if (inFlightCount >= MaxBatchesInFlight)
        {
            Console.WriteLine($"Maximum batches in flight ({MaxBatchesInFlight}) reached. Skipping submission.");
            return true;
        }

        // Calculate how many more batches we can submit
        var availableSlots = MaxBatchesInFlight - inFlightCount;

        // Get pending batches, ordered by creation date (oldest first)
        var pendingBatches = await db.WorkplaceBatches
            .Where(b => b.Status == "pending")
            .OrderBy(b => b.CreatedAt)
            .Take(availableSlots)
            .ToListAsync();

        if (pendingBatches.Count == 0)
        {
            Console.WriteLine("No pending batches to submit.");
            return true;
        }

        Console.WriteLine($"Submitting {pendingBatches.Count} batch(es) (available slots: {availableSlots})");

        foreach (var batch in pendingBatches)
        {
            Console.WriteLine($"\nProcessing batch: {batch.Id}");
            Console.WriteLine($"  File: {batch.BatchFilePath}");

            try
            {
                // Step 1: Upload file to OpenAI
                Console.WriteLine("  Uploading file to OpenAI...");
                var fileId = await UploadBatchFile(batch.BatchFilePath);
                Console.WriteLine($"  File uploaded: {fileId}");

                // Step 2: Create batch
                Console.WriteLine("  Creating batch job...");
                var batchId = await CreateBatch(fileId);
                Console.WriteLine($"  Batch created: {batchId}");

                // Step 3: Update tracking record
                batch.OpenAiInputFileId = fileId;
                batch.OpenAiBatchId = batchId;
                batch.Status = "submitted";
                batch.SubmittedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                // Step 4: Delete local batch file
                if (System.IO.File.Exists(batch.BatchFilePath))
                {
                    System.IO.File.Delete(batch.BatchFilePath);
                    Console.WriteLine("  Local batch file deleted");
                }

                Console.WriteLine("  Batch submitted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
                batch.Status = "failed";
                batch.ErrorMessage = ex.Message;
                await db.SaveChangesAsync();
            }
        }

        Console.WriteLine("\n=== LLM Batch Submit Complete ===");
        return true;
    }

    private async Task<string> UploadBatchFile(string filePath)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(fileContent, "file", Path.GetFileName(filePath));
        form.Add(new StringContent("batch"), "purpose");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/files", form);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return result.GetProperty("id").GetString()
               ?? throw new Exception("Failed to get file ID from OpenAI response");
    }

    private async Task<string> CreateBatch(string inputFileId)
    {
        var requestBody = new
        {
            input_file_id = inputFileId,
            endpoint = "/v1/chat/completions",
            completion_window = "24h"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/batches", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return result.GetProperty("id").GetString()
               ?? throw new Exception("Failed to get batch ID from OpenAI response");
    }
}

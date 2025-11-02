using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;

namespace JobApi.ETL.Stages;

public class EmbeddingBatchSubmitStage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _embeddingBatchFolder;

    public EmbeddingBatchSubmitStage(string dataRootPath)
    {
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        _embeddingBatchFolder = Path.Combine(dataRootPath, "embeddingbatch");
        Directory.CreateDirectory(_embeddingBatchFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== EMBEDDING BATCH SUBMIT STAGE ===");

        // Get all batch files in the folder
        var batchFiles = Directory.GetFiles(_embeddingBatchFolder, "*.jsonl");

        if (batchFiles.Length == 0)
        {
            Console.WriteLine("No batch files to submit.");
            return true;
        }

        Console.WriteLine($"Found {batchFiles.Length} batch file(s) to submit");

        // First, check OpenAI for existing batches to avoid duplicates
        Console.WriteLine("Checking existing batches on OpenAI...");
        var existingBatches = await GetExistingOpenAiBatches();
        Console.WriteLine($"Found {existingBatches.Count} existing batch(es) on OpenAI");

        var successCount = 0;
        var errorCount = 0;
        var skippedCount = 0;

        foreach (var filePath in batchFiles)
        {
            var fileName = Path.GetFileName(filePath);
            Console.WriteLine($"\nProcessing: {fileName}");

            try
            {
                // Check if this batch file already exists on OpenAI
                var existingBatch = existingBatches.FirstOrDefault(b =>
                    b.TryGetProperty("metadata", out var metadata) &&
                    metadata.ValueKind == JsonValueKind.Object &&
                    metadata.TryGetProperty("description", out var desc) &&
                    desc.GetString() == fileName);

                if (existingBatch.ValueKind != default)
                {
                    Console.WriteLine($"  Batch already exists on OpenAI: {existingBatch.GetProperty("id").GetString()}");
                    Console.WriteLine($"  Deleting local file");

                    System.IO.File.Delete(filePath);
                    skippedCount++;
                    continue;
                }

                // Step 1: Upload file to OpenAI
                Console.WriteLine("  Uploading file to OpenAI...");
                var fileId = await UploadBatchFile(filePath);
                Console.WriteLine($"  File uploaded: {fileId}");

                // Step 2: Create batch
                Console.WriteLine("  Creating batch job...");
                var batchId = await CreateBatch(fileId, fileName);
                Console.WriteLine($"  Batch created: {batchId}");

                // Step 3: Create database record
                Console.WriteLine("  Creating database record...");
                using (var db = JobContext.Create())
                {
                    var embeddingBatch = new EmbeddingBatch
                    {
                        Id = Guid.NewGuid(),
                        BatchFilePath = fileName,
                        OpenAiBatchId = batchId,
                        OpenAiInputFileId = fileId,
                        Status = "submitted",
                        CreatedAt = DateTime.UtcNow,
                        SubmittedAt = DateTime.UtcNow
                    };

                    db.EmbeddingBatches.Add(embeddingBatch);
                    await db.SaveChangesAsync();
                }
                Console.WriteLine("  Database record created");

                // Step 4: Delete local batch file
                System.IO.File.Delete(filePath);
                Console.WriteLine("  Local batch file deleted");

                Console.WriteLine("  Batch submitted successfully");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
                errorCount++;
            }
        }

        Console.WriteLine("\n=== Embedding Batch Submit Complete ===");
        Console.WriteLine($"Successfully submitted: {successCount}");
        Console.WriteLine($"Skipped (already on OpenAI): {skippedCount}");
        Console.WriteLine($"Failed: {errorCount}");
        return true;
    }

    private async Task<List<JsonElement>> GetExistingOpenAiBatches()
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

            var response = await _httpClient.GetAsync(url);
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

    private async Task<string> CreateBatch(string inputFileId, string fileName)
    {
        var requestBody = new
        {
            input_file_id = inputFileId,
            endpoint = "/v1/embeddings",
            completion_window = "24h",
            metadata = new
            {
                description = fileName
            }
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

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.LocationBatchSubmit;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly HttpClient _httpClient;
    private readonly string _bucketName;
    private readonly string _apiKey;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _bucketName = "circuitdreams-nl-jobsearch-api";
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    // Constructor for testing
    public Function(IAmazonS3 s3Client, HttpClient httpClient, string bucketName, string apiKey)
    {
        _s3Client = s3Client;
        _httpClient = httpClient;
        _bucketName = bucketName;
        _apiKey = apiKey;
    }

    /// <summary>
    /// Lambda handler for submitting location batch files to OpenAI
    /// Triggered by S3 event when files are uploaded to location/locationbatch/
    /// </summary>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        context.Logger.LogInformation("=== Location Batch Submit Started ===");

        foreach (var record in s3Event.Records)
        {
            var s3Key = record.S3.Object.Key;
            var fileName = Path.GetFileName(s3Key);

            context.Logger.LogInformation($"Processing file: {s3Key}");

            try
            {
                // Step 1: Download file from S3
                context.Logger.LogInformation("Downloading file from S3...");
                var fileBytes = await DownloadFileFromS3(s3Key);
                context.Logger.LogInformation($"Downloaded {fileBytes.Length} bytes");

                // Step 2: Upload to OpenAI
                context.Logger.LogInformation("Uploading to OpenAI Files API...");
                var openAiFileId = await UploadToOpenAI(fileBytes, fileName);
                context.Logger.LogInformation($"OpenAI File ID: {openAiFileId}");

                // Step 3: Create OpenAI batch
                context.Logger.LogInformation("Creating OpenAI batch job...");
                var openAiBatchId = await CreateOpenAIBatch(openAiFileId);
                context.Logger.LogInformation($"OpenAI Batch ID: {openAiBatchId}");

                // Step 4: Update database
                context.Logger.LogInformation("Updating location_batches record...");
                await UpdateLocationBatch(s3Key, openAiFileId, openAiBatchId, context);

                // Step 5: Delete from S3
                context.Logger.LogInformation("Deleting file from S3...");
                await _s3Client.DeleteObjectAsync(_bucketName, s3Key);

                context.Logger.LogInformation($"Successfully submitted batch: {fileName}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"ERROR processing {fileName}: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");

                // Move to failed folder and update database
                await HandleFailure(s3Key, ex.Message, context);
            }
        }

        context.Logger.LogInformation("=== Location Batch Submit Complete ===");
    }

    private async Task<byte[]> DownloadFileFromS3(string s3Key)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, s3Key);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<string> UploadToOpenAI(byte[] fileBytes, string fileName)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent("batch"), "purpose");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/files", form);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return result.GetProperty("id").GetString()
               ?? throw new Exception("Failed to get file ID from OpenAI response");
    }

    private async Task<string> CreateOpenAIBatch(string inputFileId)
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

    private async Task UpdateLocationBatch(string s3Key, string openAiFileId, string openAiBatchId, ILambdaContext context)
    {
        await using var db = JobContext.Create();

        var batch = await db.LocationBatches
            .FirstOrDefaultAsync(b => b.BatchFilePath == s3Key);

        if (batch == null)
        {
            context.Logger.LogWarning($"No location_batches record found for: {s3Key}");
            return;
        }

        batch.OpenAiInputFileId = openAiFileId;
        batch.OpenAiBatchId = openAiBatchId;
        batch.Status = "submitted";
        batch.SubmittedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        context.Logger.LogInformation("Database updated successfully");
    }

    private async Task HandleFailure(string s3Key, string errorMessage, ILambdaContext context)
    {
        try
        {
            var fileName = Path.GetFileName(s3Key);
            var failedKey = $"location/batchuploadfailed/{fileName}";

            // Copy to failed folder
            context.Logger.LogInformation($"Copying to failed folder: {failedKey}");
            await _s3Client.CopyObjectAsync(_bucketName, s3Key, _bucketName, failedKey);

            // Delete from original location
            context.Logger.LogInformation("Deleting from original location...");
            await _s3Client.DeleteObjectAsync(_bucketName, s3Key);

            // Update database
            await using var db = JobContext.Create();
            var batch = await db.LocationBatches
                .FirstOrDefaultAsync(b => b.BatchFilePath == s3Key);

            if (batch != null)
            {
                batch.Status = "failed";
                batch.ErrorMessage = errorMessage;
                await db.SaveChangesAsync();
                context.Logger.LogInformation("Database updated with failure status");
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error in failure handler: {ex.Message}");
        }
    }
}

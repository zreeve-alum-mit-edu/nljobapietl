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

namespace JobApi.Lambda.EmbeddingBatchSubmit;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly HttpClient _httpClient;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _httpClient = new HttpClient();

        var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openAiApiKey))
        {
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set");
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");
    }

    // Constructor for testing with mock clients
    public Function(IAmazonS3 s3Client, HttpClient httpClient)
    {
        _s3Client = s3Client;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Lambda handler for submitting embedding batch files to OpenAI
    /// Triggered when a file is uploaded to the embeddingbatch/intake/ folder
    /// </summary>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        foreach (var record in s3Event.Records)
        {
            var bucketName = record.S3.Bucket.Name;
            var key = record.S3.Object.Key;

            context.Logger.LogInformation($"Triggered by: s3://{bucketName}/{key}");

            try
            {
                await ProcessBatchFile(bucketName, key, context);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing batch file: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");

                // Move file to failure folder
                try
                {
                    await MoveBatchFile(bucketName, key, "embeddingbatch/batchuploadfailure", context);
                }
                catch (Exception moveEx)
                {
                    context.Logger.LogError($"CRITICAL: Failed to move file to failure folder: {moveEx.Message}");
                }

                // DO NOT throw - we've handled the error by moving the file
            }
        }
    }

    private async Task ProcessBatchFile(string bucketName, string key, ILambdaContext context)
    {
        // Download batch file from S3
        context.Logger.LogInformation($"Downloading batch file from s3://{bucketName}/{key}");

        string batchFileContent;
        using (var response = await _s3Client.GetObjectAsync(bucketName, key))
        using (var reader = new StreamReader(response.ResponseStream))
        {
            batchFileContent = await reader.ReadToEndAsync();
        }

        context.Logger.LogInformation($"Downloaded {batchFileContent.Length} bytes");

        // Upload to OpenAI Files API
        context.Logger.LogInformation("Uploading batch file to OpenAI Files API...");
        var openAiInputFileId = await UploadBatchFileToOpenAi(batchFileContent, Path.GetFileName(key), context);
        context.Logger.LogInformation($"OpenAI input file created: {openAiInputFileId}");

        // Create batch with OpenAI Batches API
        context.Logger.LogInformation("Creating batch with OpenAI Batches API...");
        var openAiBatchId = await CreateOpenAiBatch(openAiInputFileId, context);
        context.Logger.LogInformation($"OpenAI batch created: {openAiBatchId}");

        // Create embedding_batches tracking record
        context.Logger.LogInformation("Creating embedding_batches tracking record...");
        await CreateEmbeddingBatchRecord(key, openAiInputFileId, openAiBatchId, context);

        // Move file to success folder
        context.Logger.LogInformation("Moving file to batchsubmitted folder...");
        await MoveBatchFile(bucketName, key, "embeddingbatch/batchsubmitted", context);

        context.Logger.LogInformation($"Successfully processed batch file: {key}");
    }

    private async Task<string> UploadBatchFileToOpenAi(string content, string fileName, ILambdaContext context)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent("batch"), "purpose");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/files", form);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            context.Logger.LogError($"OpenAI Files API error: {response.StatusCode}");
            context.Logger.LogError($"Response: {responseContent}");
            throw new HttpRequestException($"Failed to upload file to OpenAI: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return result.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("OpenAI did not return a file ID");
    }

    private async Task<string> CreateOpenAiBatch(string inputFileId, ILambdaContext context)
    {
        var requestBody = new
        {
            input_file_id = inputFileId,
            endpoint = "/v1/embeddings",
            completion_window = "24h"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/batches", content);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            context.Logger.LogError($"OpenAI Batches API error: {response.StatusCode}");
            context.Logger.LogError($"Response: {responseContent}");
            throw new HttpRequestException($"Failed to create batch in OpenAI: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return result.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("OpenAI did not return a batch ID");
    }

    private async Task CreateEmbeddingBatchRecord(string s3Key, string openAiInputFileId, string openAiBatchId, ILambdaContext context)
    {
        await using var db = JobContext.Create();

        var embeddingBatch = new EmbeddingBatch
        {
            Id = Guid.NewGuid(),
            BatchFilePath = s3Key,
            OpenAiInputFileId = openAiInputFileId,
            OpenAiBatchId = openAiBatchId,
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

        db.EmbeddingBatches.Add(embeddingBatch);
        await db.SaveChangesAsync();

        context.Logger.LogInformation($"Created embedding_batches record: {embeddingBatch.Id}");
    }

    private async Task MoveBatchFile(string bucketName, string sourceKey, string destinationFolder, ILambdaContext context)
    {
        var fileName = Path.GetFileName(sourceKey);
        var destinationKey = $"{destinationFolder}/{fileName}";

        context.Logger.LogInformation($"Copying from {sourceKey} to {destinationKey}");

        // Copy to destination
        await _s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = bucketName,
            SourceKey = sourceKey,
            DestinationBucket = bucketName,
            DestinationKey = destinationKey
        });

        // Delete original
        await _s3Client.DeleteObjectAsync(bucketName, sourceKey);

        context.Logger.LogInformation($"Moved file to s3://{bucketName}/{destinationKey}");
    }
}

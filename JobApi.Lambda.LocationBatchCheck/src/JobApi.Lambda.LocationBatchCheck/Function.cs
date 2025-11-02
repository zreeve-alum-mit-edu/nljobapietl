using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.LocationBatchCheck;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly HttpClient _httpClient;
    private readonly string _bucketName;
    private readonly int _maxBatchesPerRun;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _httpClient = new HttpClient();
        _bucketName = "circuitdreams-nl-jobsearch-api";

        var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openAiApiKey))
        {
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set");
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Read max batches per run from env var, default to 3
        var maxBatchesEnv = Environment.GetEnvironmentVariable("MAX_BATCHES_PER_RUN");
        _maxBatchesPerRun = int.TryParse(maxBatchesEnv, out var maxBatches) ? maxBatches : 3;
    }

    // Constructor for testing
    public Function(IAmazonS3 s3Client, HttpClient httpClient, string bucketName, int maxBatchesPerRun = 3)
    {
        _s3Client = s3Client;
        _httpClient = httpClient;
        _bucketName = bucketName;
        _maxBatchesPerRun = maxBatchesPerRun;
    }

    /// <summary>
    /// Lambda handler for checking location batch status
    /// Triggered hourly by EventBridge
    /// </summary>
    public async Task FunctionHandler(ILambdaContext context)
    {
        context.Logger.LogInformation("=== Location Batch Check Started ===");
        context.Logger.LogInformation($"Max batches per run: {_maxBatchesPerRun}");

        await using var db = JobContext.Create();

        // Get submitted batches, limited by max per run
        var submittedBatches = await db.LocationBatches
            .Where(b => b.Status == "submitted")
            .OrderBy(b => b.SubmittedAt)
            .Take(_maxBatchesPerRun)
            .ToListAsync();

        if (submittedBatches.Count == 0)
        {
            context.Logger.LogInformation("No submitted batches to check");
            return;
        }

        context.Logger.LogInformation($"Checking status of {submittedBatches.Count} submitted batch(es)");

        foreach (var batch in submittedBatches)
        {
            context.Logger.LogInformation($"Checking batch: {batch.OpenAiBatchId}");

            try
            {
                var status = await CheckBatchStatus(batch.OpenAiBatchId!, context);
                context.Logger.LogInformation($"  Status: {status.Status}");

                switch (status.Status)
                {
                    case "completed":
                        context.Logger.LogInformation("  Batch completed! Downloading results...");

                        if (string.IsNullOrEmpty(status.OutputFileId))
                        {
                            context.Logger.LogError("  ERROR: No output file ID returned from OpenAI");
                            throw new Exception("No output file ID in completed batch");
                        }

                        context.Logger.LogInformation($"  Output File ID: {status.OutputFileId}");
                        await DownloadAndUploadResults(batch, status.OutputFileId!, context);

                        // Download error file if it exists
                        if (!string.IsNullOrEmpty(status.ErrorFileId))
                        {
                            context.Logger.LogInformation($"  Error File ID: {status.ErrorFileId}");
                            await DownloadAndUploadErrorFile(batch, status.ErrorFileId!, context);
                        }

                        batch.Status = "completed";
                        batch.CompletedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        context.Logger.LogInformation("  Results downloaded and batch marked complete");
                        break;

                    case "failed":
                    case "expired":
                    case "cancelled":
                        context.Logger.LogInformation($"  Batch {status.Status}");
                        batch.Status = status.Status;
                        batch.ErrorMessage = $"Batch {status.Status}";
                        await db.SaveChangesAsync();
                        break;

                    case "validating":
                    case "in_progress":
                    case "finalizing":
                    case "cancelling":
                        context.Logger.LogInformation($"  Batch still processing ({status.Status})");
                        break;

                    default:
                        context.Logger.LogInformation($"  Unknown status: {status.Status}");
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"  ERROR processing batch {batch.OpenAiBatchId}: {ex.Message}");
                context.Logger.LogError($"  Full exception: {ex}");
                // Don't mark as failed yet - might be temporary network issue
            }
        }

        context.Logger.LogInformation("=== Location Batch Check Complete ===");
    }

    private async Task<BatchStatus> CheckBatchStatus(string batchId, ILambdaContext context)
    {
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/batches/{batchId}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            context.Logger.LogError($"OpenAI Batches API error: {response.StatusCode}");
            context.Logger.LogError($"Response: {responseContent}");
            throw new HttpRequestException($"Failed to check batch status: {response.StatusCode}");
        }

        context.Logger.LogInformation($"  Batch API Response: {responseContent}");
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

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

    private async Task DownloadAndUploadResults(LocationBatch batch, string outputFileId, ILambdaContext context)
    {
        // Get file info to get the original filename
        var fileInfoResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}");
        fileInfoResponse.EnsureSuccessStatusCode();

        var fileInfoContent = await fileInfoResponse.Content.ReadAsStringAsync();
        var fileInfo = JsonSerializer.Deserialize<JsonElement>(fileInfoContent);
        var filename = fileInfo.GetProperty("filename").GetString()!;

        context.Logger.LogInformation($"  Downloading file: {filename}");

        // Download the results file
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}/content");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        context.Logger.LogInformation($"  Downloaded {content.Length} bytes");

        // Upload to S3
        var s3Key = $"location/locationresults/{filename}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = stream
        });

        context.Logger.LogInformation($"  Uploaded to s3://{_bucketName}/{s3Key}");
    }

    private async Task DownloadAndUploadErrorFile(LocationBatch batch, string errorFileId, ILambdaContext context)
    {
        // Get file info to get the original filename
        var fileInfoResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{errorFileId}");
        fileInfoResponse.EnsureSuccessStatusCode();

        var fileInfoContent = await fileInfoResponse.Content.ReadAsStringAsync();
        var fileInfo = JsonSerializer.Deserialize<JsonElement>(fileInfoContent);
        var filename = fileInfo.GetProperty("filename").GetString()!;

        context.Logger.LogInformation($"  Downloading error file: {filename}");

        // Download the error file
        var response = await _httpClient.GetAsync($"https://api.openai.com/v1/files/{errorFileId}/content");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        context.Logger.LogInformation($"  Downloaded {content.Length} bytes");

        // Upload to S3
        var s3Key = $"location/locationresults/{filename}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = stream
        });

        context.Logger.LogInformation($"  Uploaded error file to s3://{_bucketName}/{s3Key}");
    }

    private class BatchStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? OutputFileId { get; set; }
        public string? ErrorFileId { get; set; }
    }
}

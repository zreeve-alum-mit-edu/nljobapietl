using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.ProcessLlmResults;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private const int MaxRetries = 15;
    private const int InitialRetryDelayMs = 2000;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _bucketName = "circuitdreams-nl-jobsearch-api";
    }

    // Constructor for testing
    public Function(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    /// <summary>
    /// Lambda handler for processing LLM batch result files
    /// Triggered by S3 event when files are uploaded to llmbatch/batchresults/
    /// </summary>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        context.Logger.LogInformation("=== Process LLM Results Started ===");

        foreach (var record in s3Event.Records)
        {
            var s3Key = record.S3.Object.Key;
            var fileName = Path.GetFileName(s3Key);

            context.Logger.LogInformation($"Processing file: {fileName}");

            try
            {
                // Step 1: Move file to processing folder for idempotency
                var processingKey = $"llmbatch/batchresultprocessing/{fileName}";

                context.Logger.LogInformation($"  Moving to {processingKey}");
                await _s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = s3Key,
                    DestinationBucket = _bucketName,
                    DestinationKey = processingKey
                });

                await _s3Client.DeleteObjectAsync(_bucketName, s3Key);
                context.Logger.LogInformation("  File moved successfully");

                // Step 2: Download file content
                var getObjectResponse = await _s3Client.GetObjectAsync(_bucketName, processingKey);
                using var reader = new StreamReader(getObjectResponse.ResponseStream);
                var fileContent = await reader.ReadToEndAsync();

                // Step 3: Fork based on file type
                if (fileName.Contains("_error.jsonl"))
                {
                    context.Logger.LogInformation("  Processing as ERROR file");
                    await ProcessErrorFile(fileContent, context);
                }
                else if (fileName.Contains("_output.jsonl"))
                {
                    context.Logger.LogInformation("  Processing as OUTPUT file");
                    await ProcessResultFile(fileContent, context);
                }
                else
                {
                    context.Logger.LogWarning($"  Unknown file type: {fileName}");
                }

                // Step 4: Delete file after successful processing
                await _s3Client.DeleteObjectAsync(_bucketName, processingKey);
                context.Logger.LogInformation($"  Deleted processed file: {fileName}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"ERROR processing {fileName}: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");
                throw;
            }
        }

        // Trigger LocationBatchGenerate Lambda asynchronously
        try
        {
            context.Logger.LogInformation("Triggering LocationBatchGenerate Lambda...");
            var lambdaClient = new AmazonLambdaClient();
            await lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = "JobApi-LocationBatchGenerate",
                InvocationType = InvocationType.Event // Fire and forget
            });
            context.Logger.LogInformation("LocationBatchGenerate Lambda triggered successfully");
        }
        catch (Exception ex)
        {
            context.Logger.LogWarning($"Failed to trigger LocationBatchGenerate Lambda: {ex.Message}");
            // Don't throw - this is non-critical
        }

        context.Logger.LogInformation("=== Process LLM Results Complete ===");
    }

    private async Task ProcessResultFile(string fileContent, ILambdaContext context)
    {
        var successCount = 0;
        var errorCount = 0;
        var lineNumber = 0;

        var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // First pass: collect all job IDs
        var jobIds = new List<Guid>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var batchResponse = JsonSerializer.Deserialize<JsonElement>(line);
                var customId = batchResponse.GetProperty("custom_id").GetString()!;
                var jobIdString = customId.Replace("job_", "");
                var jobId = Guid.Parse(jobIdString);
                jobIds.Add(jobId);
            }
            catch
            {
                // Skip malformed lines in first pass
            }
        }

        context.Logger.LogInformation($"  Loading {jobIds.Count} jobs from database...");

        // Batch load all jobs in a single query
        await using var db = JobContext.Create();
        var jobs = await RetryWithExponentialBackoffAsync(
            async () => await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToDictionaryAsync(j => j.Id),
            $"Batch load {jobIds.Count} jobs",
            context);

        context.Logger.LogInformation($"  Loaded {jobs.Count} jobs, processing results...");

        // Second pass: process each line
        foreach (var line in lines)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var batchResponse = JsonSerializer.Deserialize<JsonElement>(line);

                // Get the custom_id which contains the job ID
                var customId = batchResponse.GetProperty("custom_id").GetString()!;
                var jobIdString = customId.Replace("job_", "");
                var jobId = Guid.Parse(jobIdString);

                // Check if the response has an error
                if (batchResponse.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
                {
                    var errorMessage = errorElement.GetProperty("message").GetString();
                    context.Logger.LogInformation($"  Line {lineNumber}: Error for job {jobId}: {errorMessage}");
                    HandleJobError(jobs, jobId, lineNumber, context);
                    errorCount++;
                    continue;
                }

                // Get the response content
                var response = batchResponse.GetProperty("response");
                var body = response.GetProperty("body");
                var choices = body.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var content = message.GetProperty("content").GetString();

                // Check if content is empty or null (can happen when model hits token limit)
                if (string.IsNullOrWhiteSpace(content))
                {
                    context.Logger.LogInformation($"  Line {lineNumber}: Empty content (likely hit token limit) for job {jobId}");
                    HandleJobError(jobs, jobId, lineNumber, context);
                    errorCount++;
                    continue;
                }

                // Parse the workplace classification JSON
                var classification = JsonSerializer.Deserialize<JsonElement>(content);
                var workplaceType = classification.GetProperty("type").GetString();
                var inferred = classification.GetProperty("inferred").GetBoolean();
                var confidence = classification.GetProperty("confidence").GetString();

                // Look up job from preloaded dictionary
                if (jobs.TryGetValue(jobId, out var job))
                {
                    job.GeneratedWorkplace = workplaceType;
                    job.GeneratedWorkplaceInferred = inferred;
                    job.GeneratedWorkplaceConfidence = confidence;
                    job.Status = "workplace_classified";
                    successCount++;
                }
                else
                {
                    context.Logger.LogInformation($"  Line {lineNumber}: Job {jobId} not found in database");
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogInformation($"  Line {lineNumber}: Error parsing response: {ex.Message}");

                // Try to extract job ID from the line for error handling
                try
                {
                    var batchResponse = JsonSerializer.Deserialize<JsonElement>(line);
                    var customId = batchResponse.GetProperty("custom_id").GetString()!;
                    var jobIdString = customId.Replace("job_", "");
                    var jobId = Guid.Parse(jobIdString);
                    HandleJobError(jobs, jobId, lineNumber, context);
                }
                catch
                {
                    // If we can't extract job ID, just skip
                }

                errorCount++;
            }
        }

        // Save all changes (with retry logic)
        await RetryWithExponentialBackoffAsync(
            async () => await db.SaveChangesAsync(),
            "SaveChangesAsync (result file)",
            context);

        context.Logger.LogInformation($"  Processed {successCount} job(s) successfully");
        if (errorCount > 0)
        {
            context.Logger.LogInformation($"  {errorCount} error(s) encountered");
        }
    }

    private async Task ProcessErrorFile(string fileContent, ILambdaContext context)
    {
        var errorCount = 0;
        var lineNumber = 0;

        var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // First pass: collect all job IDs
        var jobIds = new List<Guid>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var errorEntry = JsonSerializer.Deserialize<JsonElement>(line);
                var customId = errorEntry.GetProperty("custom_id").GetString()!;
                var jobIdString = customId.Replace("job_", "");
                var jobId = Guid.Parse(jobIdString);
                jobIds.Add(jobId);
            }
            catch
            {
                // Skip malformed lines in first pass
            }
        }

        context.Logger.LogInformation($"  Loading {jobIds.Count} jobs from database...");

        // Batch load all jobs in a single query
        await using var db = JobContext.Create();
        var jobs = await RetryWithExponentialBackoffAsync(
            async () => await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToDictionaryAsync(j => j.Id),
            $"Batch load {jobIds.Count} jobs (error file)",
            context);

        context.Logger.LogInformation($"  Loaded {jobs.Count} jobs, processing errors...");

        // Second pass: process each line
        foreach (var line in lines)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var errorEntry = JsonSerializer.Deserialize<JsonElement>(line);

                // Get the custom_id which contains the job ID
                var customId = errorEntry.GetProperty("custom_id").GetString()!;
                var jobIdString = customId.Replace("job_", "");
                var jobId = Guid.Parse(jobIdString);

                // Get error details from response.body.error.message
                string errorMessage = "Unknown error";
                if (errorEntry.TryGetProperty("response", out var responseElement))
                {
                    if (responseElement.TryGetProperty("body", out var bodyElement))
                    {
                        if (bodyElement.TryGetProperty("error", out var errorElement))
                        {
                            if (errorElement.TryGetProperty("message", out var msgElement))
                            {
                                errorMessage = msgElement.GetString() ?? "Unknown error";
                            }
                        }
                    }
                }

                context.Logger.LogInformation($"  Line {lineNumber}: Error for job {jobId}: {errorMessage}");

                // Handle the error through retry flow
                HandleJobError(jobs, jobId, lineNumber, context);
                errorCount++;
            }
            catch (Exception ex)
            {
                context.Logger.LogInformation($"  Line {lineNumber}: Error parsing error entry: {ex.Message}");
                errorCount++;
            }
        }

        // Save all changes (with retry logic)
        await RetryWithExponentialBackoffAsync(
            async () => await db.SaveChangesAsync(),
            "SaveChangesAsync (error file)",
            context);

        context.Logger.LogInformation($"  Processed {errorCount} error(s) from error file");
    }

    private void HandleJobError(Dictionary<Guid, Job> jobs, Guid jobId, int lineNumber, ILambdaContext context)
    {
        // Look up job from preloaded dictionary
        if (!jobs.TryGetValue(jobId, out var job))
        {
            context.Logger.LogInformation($"  Line {lineNumber}: Job {jobId} not found in dictionary for retry handling");
            return;
        }

        // Increment retry count
        job.LlmWorkplaceRetryCount++;

        if (job.LlmWorkplaceRetryCount >= 3)
        {
            // Failed after 3 attempts - mark as failed
            job.Status = "failed - llm-workplace-generation";
            job.IsValid = false;
            context.Logger.LogInformation($"  Line {lineNumber}: Job {jobId} failed after {job.LlmWorkplaceRetryCount} attempts - marked as failed");
        }
        else
        {
            // Reset to ingested for retry
            job.Status = "ingested";
            context.Logger.LogInformation($"  Line {lineNumber}: Job {jobId} set back to 'ingested' for retry (attempt {job.LlmWorkplaceRetryCount}/3)");
        }
    }

    // Aggressive retry wrapper with exponential backoff
    private async Task<T> RetryWithExponentialBackoffAsync<T>(Func<Task<T>> operation, string operationName, ILambdaContext context)
    {
        var retryDelay = InitialRetryDelayMs;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                context.Logger.LogInformation($"  DB Error on {operationName} (attempt {attempt + 1}/{MaxRetries}): {ex.Message}");
                context.Logger.LogInformation($"  Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff: 2s, 4s, 8s, 16s, 32s, 64s...
            }
        }

        throw new Exception($"Failed {operationName} after {MaxRetries} attempts");
    }

    // Non-generic version for void operations
    private async Task RetryWithExponentialBackoffAsync(Func<Task> operation, string operationName, ILambdaContext context)
    {
        var retryDelay = InitialRetryDelayMs;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                context.Logger.LogInformation($"  DB Error on {operationName} (attempt {attempt + 1}/{MaxRetries}): {ex.Message}");
                context.Logger.LogInformation($"  Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff: 2s, 4s, 8s, 16s, 32s, 64s...
            }
        }

        throw new Exception($"Failed {operationName} after {MaxRetries} attempts");
    }
}

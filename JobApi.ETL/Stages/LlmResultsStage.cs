using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class LlmResultsStage
{
    private readonly string _llmResultFolder;
    private const int MaxRetries = 15;
    private const int InitialRetryDelayMs = 2000;

    public LlmResultsStage(string dataRootPath)
    {
        _llmResultFolder = Path.Combine(dataRootPath, "llmresult");
        Directory.CreateDirectory(_llmResultFolder);
    }

    // Aggressive retry wrapper with exponential backoff
    private async Task<T> RetryWithExponentialBackoffAsync<T>(Func<Task<T>> operation, string operationName)
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
                Console.WriteLine($"  DB Error on {operationName} (attempt {attempt + 1}/{MaxRetries}): {ex.Message}");
                Console.WriteLine($"  Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff: 2s, 4s, 8s, 16s, 32s, 64s...
            }
        }

        throw new Exception($"Failed {operationName} after {MaxRetries} attempts");
    }

    // Non-generic version for void operations
    private async Task RetryWithExponentialBackoffAsync(Func<Task> operation, string operationName)
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
                Console.WriteLine($"  DB Error on {operationName} (attempt {attempt + 1}/{MaxRetries}): {ex.Message}");
                Console.WriteLine($"  Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff: 2s, 4s, 8s, 16s, 32s, 64s...
            }
        }

        throw new Exception($"Failed {operationName} after {MaxRetries} attempts");
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LLM RESULTS PROCESSING STAGE ===");

        var resultFiles = Directory.GetFiles(_llmResultFolder, "workplace_results_*.jsonl");
        var errorFiles = Directory.GetFiles(_llmResultFolder, "workplace_errors_*.jsonl");

        if (resultFiles.Length == 0 && errorFiles.Length == 0)
        {
            Console.WriteLine("No result or error files to process.");
            return true;
        }

        Console.WriteLine($"Found {resultFiles.Length} result file(s) and {errorFiles.Length} error file(s) to process");

        using var db = JobContext.Create();

        // Process result files
        foreach (var filePath in resultFiles)
        {
            Console.WriteLine($"\nProcessing results: {Path.GetFileName(filePath)}");

            try
            {
                await ProcessResultFile(db, filePath);

                // Delete the file after successful processing
                System.IO.File.Delete(filePath);
                Console.WriteLine($"  Deleted result file: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR processing {Path.GetFileName(filePath)}: {ex.Message}");
                return false;
            }
        }

        // Process error files
        foreach (var filePath in errorFiles)
        {
            Console.WriteLine($"\nProcessing errors: {Path.GetFileName(filePath)}");

            try
            {
                await ProcessErrorFile(db, filePath);

                // Delete the file after successful processing
                System.IO.File.Delete(filePath);
                Console.WriteLine($"  Deleted error file: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR processing {Path.GetFileName(filePath)}: {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("\n=== LLM Results Processing Complete ===");
        return true;
    }

    private async Task ProcessResultFile(JobContext db, string filePath)
    {
        var successCount = 0;
        var errorCount = 0;
        var lineNumber = 0;

        // First pass: collect all job IDs
        var jobIds = new List<Guid>();
        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
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

        Console.WriteLine($"  Loading {jobIds.Count} jobs from database...");

        // Batch load all jobs in a single query
        var jobs = await RetryWithExponentialBackoffAsync(
            async () => await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToDictionaryAsync(j => j.Id),
            $"Batch load {jobIds.Count} jobs");

        Console.WriteLine($"  Loaded {jobs.Count} jobs, processing results...");

        // Second pass: process each line
        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
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
                    Console.WriteLine($"  Line {lineNumber}: Error for job {jobId}: {errorMessage}");
                    HandleJobError(jobs, jobId, lineNumber);
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
                    Console.WriteLine($"  Line {lineNumber}: Empty content (likely hit token limit) for job {jobId}");
                    HandleJobError(jobs, jobId, lineNumber);
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
                    Console.WriteLine($"  Line {lineNumber}: Job {jobId} not found in database");
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Line {lineNumber}: Error parsing response: {ex.Message}");

                // Try to extract job ID from the line for error handling
                try
                {
                    var batchResponse = JsonSerializer.Deserialize<JsonElement>(line);
                    var customId = batchResponse.GetProperty("custom_id").GetString()!;
                    var jobIdString = customId.Replace("job_", "");
                    var jobId = Guid.Parse(jobIdString);
                    HandleJobError(jobs, jobId, lineNumber);
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
            "SaveChangesAsync (result file)");

        Console.WriteLine($"  Processed {successCount} job(s) successfully");
        if (errorCount > 0)
        {
            Console.WriteLine($"  {errorCount} error(s) encountered");
        }
    }

    private async Task ProcessErrorFile(JobContext db, string filePath)
    {
        var errorCount = 0;
        var lineNumber = 0;

        // First pass: collect all job IDs
        var jobIds = new List<Guid>();
        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
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

        Console.WriteLine($"  Loading {jobIds.Count} jobs from database...");

        // Batch load all jobs in a single query
        var jobs = await RetryWithExponentialBackoffAsync(
            async () => await db.Jobs.Where(j => jobIds.Contains(j.Id)).ToDictionaryAsync(j => j.Id),
            $"Batch load {jobIds.Count} jobs (error file)");

        Console.WriteLine($"  Loaded {jobs.Count} jobs, processing errors...");

        // Second pass: process each line
        await foreach (var line in System.IO.File.ReadLinesAsync(filePath))
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

                Console.WriteLine($"  Line {lineNumber}: Error for job {jobId}: {errorMessage}");

                // Handle the error through retry flow
                HandleJobError(jobs, jobId, lineNumber);
                errorCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Line {lineNumber}: Error parsing error entry: {ex.Message}");
                errorCount++;
            }
        }

        // Save all changes (with retry logic)
        await RetryWithExponentialBackoffAsync(
            async () => await db.SaveChangesAsync(),
            "SaveChangesAsync (error file)");

        Console.WriteLine($"  Processed {errorCount} error(s) from error file");
    }

    private void HandleJobError(Dictionary<Guid, Job> jobs, Guid jobId, int lineNumber)
    {
        // Look up job from preloaded dictionary
        if (!jobs.TryGetValue(jobId, out var job))
        {
            Console.WriteLine($"  Line {lineNumber}: Job {jobId} not found in dictionary for retry handling");
            return;
        }

        // Increment retry count
        job.LlmWorkplaceRetryCount++;

        if (job.LlmWorkplaceRetryCount >= 3)
        {
            // Failed after 3 attempts - mark as failed
            job.Status = "failed - llm-workplace-generation";
            job.IsValid = false;
            Console.WriteLine($"  Line {lineNumber}: Job {jobId} failed after {job.LlmWorkplaceRetryCount} attempts - marked as failed");
        }
        else
        {
            // Reset to ingested for retry
            job.Status = "ingested";
            Console.WriteLine($"  Line {lineNumber}: Job {jobId} set back to 'ingested' for retry (attempt {job.LlmWorkplaceRetryCount}/3)");
        }
    }
}

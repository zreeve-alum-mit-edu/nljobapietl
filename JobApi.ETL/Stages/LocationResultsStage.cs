using System.Text.Json;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApi.ETL.Stages;

public class LocationResultsStage
{
    private readonly string _locationResultFolder;

    public LocationResultsStage(string dataRootPath)
    {
        _locationResultFolder = Path.Combine(dataRootPath, "locationresult");
        Directory.CreateDirectory(_locationResultFolder);
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine("=== LOCATION RESULTS PROCESSING STAGE ===");

        var resultFiles = Directory.GetFiles(_locationResultFolder, "location_results_chunk_*.jsonl")
            .OrderBy(f => f)
            .ToArray();
        var errorFiles = Directory.GetFiles(_locationResultFolder, "location_errors_*.jsonl");

        if (resultFiles.Length == 0 && errorFiles.Length == 0)
        {
            Console.WriteLine("No result or error files to process.");
            return true;
        }

        Console.WriteLine($"Found {resultFiles.Length} result chunk file(s) and {errorFiles.Length} error file(s) to process");

        using var db = JobContext.Create();

        // Process result files
        int fileCount = 0;
        foreach (var filePath in resultFiles)
        {
            fileCount++;
            Console.WriteLine($"\n[{fileCount}/{resultFiles.Length}] Processing results: {Path.GetFileName(filePath)}");

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
                Console.WriteLine($"  Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"  Inner exception type: {ex.InnerException.GetType().Name}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"  Inner inner exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                Console.WriteLine($"  Stack trace: {ex.StackTrace}");
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
                Console.WriteLine($"  Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"  Inner exception type: {ex.InnerException.GetType().Name}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"  Inner inner exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                Console.WriteLine($"  Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        Console.WriteLine("\n=== Location Results Processing Complete ===");
        return true;
    }

    private async Task ProcessResultFile(JobContext db, string filePath)
    {
        var successCount = 0;
        var errorCount = 0;
        var validationErrorCount = 0;
        var lineNumber = 0;
        const int BatchSize = 5000;

        Console.WriteLine("  Processing result file with Entity Framework...");

        // Create error output file
        var errorFilePath = filePath.Replace("location_results_chunk_", "location_errors_chunk_");
        using var errorWriter = new StreamWriter(errorFilePath, append: false);

        // Process line by line and accumulate job IDs for batch loading
        var jobUpdates = new List<(Guid JobId, string? City, string? State, string? Country, string Status, bool IsValid)>();
        var retryJobIds = new HashSet<Guid>();

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
                    retryJobIds.Add(jobId);
                    await errorWriter.WriteLineAsync(line);
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

                // Check if content is empty or null
                if (string.IsNullOrWhiteSpace(content))
                {
                    retryJobIds.Add(jobId);
                    await errorWriter.WriteLineAsync(line);
                    errorCount++;
                    continue;
                }

                // Parse the location normalization JSON
                var location = JsonSerializer.Deserialize<JsonElement>(content);

                string? city = null;
                string? state = null;
                string? country = null;

                if (location.TryGetProperty("city", out var cityElement) && cityElement.ValueKind != JsonValueKind.Null)
                {
                    city = cityElement.GetString();
                }

                // Validate state (must be 2 chars or null/empty)
                if (location.TryGetProperty("state", out var stateElement) && stateElement.ValueKind != JsonValueKind.Null)
                {
                    var stateValue = stateElement.GetString();
                    if (!string.IsNullOrEmpty(stateValue))
                    {
                        if (stateValue.Length > 2)
                        {
                            // Invalid state - write to error file for retry
                            retryJobIds.Add(jobId);
                            await errorWriter.WriteLineAsync(line);
                            validationErrorCount++;
                            continue;
                        }
                        state = stateValue;
                    }
                }

                // Validate and normalize country (must be 2 chars or null/empty, USA → US)
                if (location.TryGetProperty("country", out var countryElement) && countryElement.ValueKind != JsonValueKind.Null)
                {
                    var countryValue = countryElement.GetString();
                    if (!string.IsNullOrEmpty(countryValue))
                    {
                        // Normalize USA to US
                        if (countryValue.Equals("USA", StringComparison.OrdinalIgnoreCase))
                        {
                            countryValue = "US";
                        }

                        if (countryValue.Length > 2)
                        {
                            // Invalid country - write to error file for retry
                            retryJobIds.Add(jobId);
                            await errorWriter.WriteLineAsync(line);
                            validationErrorCount++;
                            continue;
                        }
                        country = countryValue;
                    }
                }

                // Determine status and validity
                string status;
                bool isValid;
                if (string.IsNullOrEmpty(country) || !country.Equals("US", StringComparison.OrdinalIgnoreCase))
                {
                    status = "invalid - non-us-location";
                    isValid = false;
                }
                else
                {
                    status = "location_classified";
                    isValid = true;
                }

                jobUpdates.Add((jobId, city, state, country, status, isValid));
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Line {lineNumber}: Parse error: {ex.Message}");
                errorCount++;
            }

            // Execute batch when we hit the batch size
            if (jobUpdates.Count >= BatchSize || retryJobIds.Count >= BatchSize)
            {
                await ExecuteUpdateBatch(db, jobUpdates, retryJobIds);
                jobUpdates.Clear();
                retryJobIds.Clear();

                if (lineNumber % 10000 == 0)
                {
                    Console.WriteLine($"  Processed {lineNumber} lines ({successCount} successful, {errorCount + validationErrorCount} errors)...");
                }
            }
        }

        // Execute remaining updates
        if (jobUpdates.Count > 0 || retryJobIds.Count > 0)
        {
            await ExecuteUpdateBatch(db, jobUpdates, retryJobIds);
        }

        Console.WriteLine($"\nProcessing complete!");
        Console.WriteLine($"  Total processed: {successCount} successful");
        Console.WriteLine($"  Validation errors (state/country > 2 chars): {validationErrorCount}");
        Console.WriteLine($"  Other errors: {errorCount}");

        // Delete error file if empty
        errorWriter.Close();
        if (new FileInfo(errorFilePath).Length == 0)
        {
            System.IO.File.Delete(errorFilePath);
        }
    }

    private async Task ExecuteUpdateBatch(
        JobContext db,
        List<(Guid JobId, string? City, string? State, string? Country, string Status, bool IsValid)> updates,
        HashSet<Guid> retryJobIds)
    {
        // Collect all job IDs we need to load
        var allJobIds = updates.Select(u => u.JobId).Union(retryJobIds).ToHashSet();

        if (allJobIds.Count == 0)
            return;

        // Load all jobs in a single query
        var jobs = await db.Jobs
            .Where(j => allJobIds.Contains(j.Id))
            .ToDictionaryAsync(j => j.Id);

        // Apply successful updates
        foreach (var (jobId, city, state, country, status, isValid) in updates)
        {
            if (jobs.TryGetValue(jobId, out var job))
            {
                job.GeneratedCity = city;
                job.GeneratedState = state;
                job.GeneratedCountry = country;
                job.Status = status;
                job.IsValid = isValid;
            }
        }

        // Handle retries (increment retry count, reset or fail)
        foreach (var jobId in retryJobIds)
        {
            if (jobs.TryGetValue(jobId, out var job))
            {
                var newRetryCount = job.LlmLocationRetryCount + 1;
                job.LlmLocationRetryCount = newRetryCount;

                if (newRetryCount >= 3)
                {
                    // Failed after 3 attempts
                    job.Status = "failed - llm-location-generation";
                    job.IsValid = false;
                }
                else
                {
                    // Reset for retry
                    job.Status = "workplace_classified";
                }
            }
        }

        // Save all changes in one transaction
        await db.SaveChangesAsync();

        // Clear change tracker to release memory
        db.ChangeTracker.Clear();
    }

    private async Task ProcessErrorFile(JobContext db, string filePath)
    {
        var errorCount = 0;
        var lineNumber = 0;

        Console.WriteLine("  Processing error file with direct SQL updates...");

        // Process line by line and execute updates directly
        var retryUpdates = new List<(Guid JobId, int LineNumber, string ErrorMessage)>();

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

                retryUpdates.Add((jobId, lineNumber, errorMessage));
                errorCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Line {lineNumber}: Error parsing error entry: {ex.Message}");
                errorCount++;
            }
        }

        // Execute retry updates
        if (retryUpdates.Count > 0)
        {
            var jobIds = retryUpdates.Select(r => r.JobId).ToHashSet();
            var jobs = await db.Jobs
                .Where(j => jobIds.Contains(j.Id))
                .Select(j => new { j.Id, j.LlmLocationRetryCount })
                .ToDictionaryAsync(j => j.Id, j => j.LlmLocationRetryCount);

            foreach (var retry in retryUpdates)
            {
                if (jobs.TryGetValue(retry.JobId, out var currentRetryCount))
                {
                    var newRetryCount = currentRetryCount + 1;

                    Console.WriteLine($"  Line {retry.LineNumber}: Error for job {retry.JobId}: {retry.ErrorMessage}");

                    if (newRetryCount >= 3)
                    {
                        // Failed after 3 attempts
                        await db.Jobs
                            .Where(j => j.Id == retry.JobId)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(j => j.LlmLocationRetryCount, newRetryCount)
                                .SetProperty(j => j.Status, "failed - llm-location-generation")
                                .SetProperty(j => j.IsValid, false));

                        Console.WriteLine($"  Line {retry.LineNumber}: Job {retry.JobId} failed after {newRetryCount} attempts - marked as failed");
                    }
                    else
                    {
                        // Reset for retry
                        await db.Jobs
                            .Where(j => j.Id == retry.JobId)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(j => j.LlmLocationRetryCount, newRetryCount)
                                .SetProperty(j => j.Status, "workplace_classified"));

                        Console.WriteLine($"  Line {retry.LineNumber}: Job {retry.JobId} set back to 'workplace_classified' for retry (attempt {newRetryCount}/3)");
                    }
                }
            }
        }

        Console.WriteLine($"  Processed {errorCount} error(s) from error file");
    }
}

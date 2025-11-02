using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.ProcessLocationResults;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private const int BatchSize = 5000;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _bucketName = "circuitdreams-nl-jobsearch-api";
    }

    public Function(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        context.Logger.LogInformation("=== Process Location Results Started ===");

        foreach (var record in s3Event.Records)
        {
            var sourceKey = record.S3.Object.Key;
            var fileName = Path.GetFileName(sourceKey);

            context.Logger.LogInformation($"Processing file: {fileName}");

            try
            {
                // Step 1: Try to claim the file by moving it to processing folder
                var processingKey = $"location/locationresultprocessing/{fileName}";

                context.Logger.LogInformation($"Attempting to claim file by moving to: {processingKey}");

                try
                {
                    await _s3Client.CopyObjectAsync(new CopyObjectRequest
                    {
                        SourceBucket = _bucketName,
                        SourceKey = sourceKey,
                        DestinationBucket = _bucketName,
                        DestinationKey = processingKey
                    });

                    await _s3Client.DeleteObjectAsync(_bucketName, sourceKey);
                    context.Logger.LogInformation("File claimed successfully");
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    context.Logger.LogInformation("File already claimed by another Lambda instance. Skipping.");
                    continue;
                }

                // Step 2: Determine if this is an error file or output file
                var isErrorFile = fileName.Contains("error", StringComparison.OrdinalIgnoreCase);

                if (isErrorFile)
                {
                    context.Logger.LogInformation("Processing as ERROR file");
                    await ProcessErrorFile(processingKey, fileName, context);
                }
                else
                {
                    context.Logger.LogInformation("Processing as OUTPUT file");
                    await ProcessOutputFile(processingKey, fileName, context);
                }

                // Step 3: Delete the file from processing folder
                context.Logger.LogInformation("Deleting file from processing folder...");
                await _s3Client.DeleteObjectAsync(_bucketName, processingKey);

                context.Logger.LogInformation($"Successfully processed and deleted: {fileName}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"ERROR processing {fileName}: {ex.Message}");
                context.Logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        context.Logger.LogInformation("=== Process Location Results Complete ===");
    }

    private async Task ProcessOutputFile(string s3Key, string fileName, ILambdaContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Download file
        context.Logger.LogInformation("Downloading file from S3...");
        var response = await _s3Client.GetObjectAsync(_bucketName, s3Key);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();
        context.Logger.LogInformation($"Downloaded {content.Length} bytes");

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        context.Logger.LogInformation($"Total lines to process: {lines.Length}");

        var successRecords = new List<LocationRecord>();
        var retryJobIds = new HashSet<Guid>();
        var parseErrors = 0;

        // Parse all lines
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var batchResponse = JsonSerializer.Deserialize<JsonElement>(line);
                var customId = batchResponse.GetProperty("custom_id").GetString()!;
                var jobId = Guid.Parse(customId.Replace("job_", ""));

                // Check for error
                if (batchResponse.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
                {
                    retryJobIds.Add(jobId);
                    continue;
                }

                // Get response content
                var responseBody = batchResponse.GetProperty("response").GetProperty("body");
                var choices = responseBody.GetProperty("choices");
                var message = choices[0].GetProperty("message");
                var contentStr = message.GetProperty("content").GetString();

                if (string.IsNullOrWhiteSpace(contentStr))
                {
                    retryJobIds.Add(jobId);
                    continue;
                }

                // Parse location JSON
                var location = JsonSerializer.Deserialize<JsonElement>(contentStr);

                string? city = null;
                string? state = null;
                string? country = null;

                if (location.TryGetProperty("city", out var cityElement) && cityElement.ValueKind != JsonValueKind.Null)
                {
                    city = cityElement.GetString();
                }

                // Validate state (max 2 chars)
                if (location.TryGetProperty("state", out var stateElement) && stateElement.ValueKind != JsonValueKind.Null)
                {
                    var stateValue = stateElement.GetString();
                    if (!string.IsNullOrEmpty(stateValue))
                    {
                        if (stateValue.Length > 2)
                        {
                            retryJobIds.Add(jobId);
                            continue;
                        }
                        state = stateValue;
                    }
                }

                // Validate and normalize country (max 2 chars, USA → US)
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
                            retryJobIds.Add(jobId);
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

                successRecords.Add(new LocationRecord
                {
                    JobId = jobId,
                    City = city,
                    State = state,
                    Country = country,
                    Status = status,
                    IsValid = isValid
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning($"Line {i + 1}: Parse error: {ex.Message}");
                parseErrors++;
            }
        }

        context.Logger.LogInformation($"Parsing complete: {successRecords.Count} successful, {retryJobIds.Count} retries, {parseErrors} parse errors");

        // Process in batches
        var totalBatches = (int)Math.Ceiling((successRecords.Count + retryJobIds.Count) / (double)BatchSize);
        var successBatches = successRecords.Chunk(BatchSize).ToList();
        var retryBatches = retryJobIds.Chunk(BatchSize).ToList();

        context.Logger.LogInformation($"Processing {successBatches.Count} success batches and {retryBatches.Count} retry batches");

        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        var batchNum = 0;

        // Process success batches
        foreach (var batch in successBatches)
        {
            batchNum++;
            context.Logger.LogInformation($"[BATCH {batchNum}] Processing {batch.Length} successful location records");

            var batchStopwatch = Stopwatch.StartNew();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Create temp table
                await using (var cmd = new NpgsqlCommand(@"
                    CREATE TEMP TABLE temp_location_updates (
                        job_id UUID,
                        city TEXT,
                        state TEXT,
                        country TEXT,
                        status TEXT,
                        is_valid BOOLEAN
                    ) ON COMMIT DROP", conn, transaction))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // COPY data into temp table
                var copyStopwatch = Stopwatch.StartNew();
                await using (var writer = await conn.BeginBinaryImportAsync(
                    "COPY temp_location_updates (job_id, city, state, country, status, is_valid) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var record in batch)
                    {
                        await writer.StartRowAsync();
                        await writer.WriteAsync(record.JobId, NpgsqlTypes.NpgsqlDbType.Uuid);
                        await writer.WriteAsync(record.City, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.State, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.Country, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.Status, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.IsValid, NpgsqlTypes.NpgsqlDbType.Boolean);
                    }
                    await writer.CompleteAsync();
                }
                copyStopwatch.Stop();
                context.Logger.LogInformation($"[PERF] COPY took {copyStopwatch.ElapsedMilliseconds}ms for {batch.Length} records");

                // UPDATE jobs from temp table
                var updateStopwatch = Stopwatch.StartNew();
                await using (var cmd = new NpgsqlCommand(@"
                    UPDATE jobs j
                    SET
                        generated_city = t.city,
                        generated_state = t.state,
                        generated_country = t.country,
                        status = t.status,
                        is_valid = t.is_valid
                    FROM temp_location_updates t
                    WHERE j.id = t.job_id", conn, transaction))
                {
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    updateStopwatch.Stop();
                    context.Logger.LogInformation($"[PERF] UPDATE took {updateStopwatch.ElapsedMilliseconds}ms, {rowsAffected} rows affected");
                }

                await transaction.CommitAsync();
                batchStopwatch.Stop();
                context.Logger.LogInformation($"[BATCH {batchNum}] TOTAL batch time: {batchStopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"[BATCH {batchNum}] Error: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Process retry batches
        foreach (var batch in retryBatches)
        {
            batchNum++;
            context.Logger.LogInformation($"[BATCH {batchNum}] Processing {batch.Length} retry records");

            await ProcessRetryBatch(conn, batch.ToList(), context);
        }

        stopwatch.Stop();
        context.Logger.LogInformation($"[PERF] TOTAL processing time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private async Task ProcessErrorFile(string s3Key, string fileName, ILambdaContext context)
    {
        context.Logger.LogInformation("Downloading error file from S3...");
        var response = await _s3Client.GetObjectAsync(_bucketName, s3Key);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        context.Logger.LogInformation($"Total error lines to process: {lines.Length}");

        var retryJobIds = new List<Guid>();

        // Parse all error lines to get job IDs
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var errorEntry = JsonSerializer.Deserialize<JsonElement>(line);
                var customId = errorEntry.GetProperty("custom_id").GetString()!;
                var jobId = Guid.Parse(customId.Replace("job_", ""));
                retryJobIds.Add(jobId);
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning($"Line {i + 1}: Error parsing: {ex.Message}");
            }
        }

        context.Logger.LogInformation($"Parsed {retryJobIds.Count} job IDs from error file");

        // Process retries in batches
        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        var batches = retryJobIds.Chunk(BatchSize).ToList();
        var batchNum = 0;

        foreach (var batch in batches)
        {
            batchNum++;
            context.Logger.LogInformation($"[BATCH {batchNum}/{batches.Count}] Processing {batch.Length} error retries");
            await ProcessRetryBatch(conn, batch.ToList(), context);
        }
    }

    private async Task ProcessRetryBatch(NpgsqlConnection conn, List<Guid> jobIds, ILambdaContext context)
    {
        if (jobIds.Count == 0) return;

        var stopwatch = Stopwatch.StartNew();

        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Get current retry counts
            var retryCountsCmd = new NpgsqlCommand(@"
                SELECT id, llm_location_retry_count
                FROM jobs
                WHERE id = ANY(@ids)", conn, transaction);
            retryCountsCmd.Parameters.AddWithValue("ids", jobIds.ToArray());

            var retryCounts = new Dictionary<Guid, int>();
            await using (var reader = await retryCountsCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    retryCounts[reader.GetGuid(0)] = reader.GetInt32(1);
                }
            }

            // Separate into retry and fail lists
            var retryList = new List<Guid>();
            var failList = new List<Guid>();

            foreach (var jobId in jobIds)
            {
                if (retryCounts.TryGetValue(jobId, out var currentCount))
                {
                    var newCount = currentCount + 1;
                    if (newCount >= 3)
                    {
                        failList.Add(jobId);
                    }
                    else
                    {
                        retryList.Add(jobId);
                    }
                }
            }

            // Update retries (reset to workplace_classified)
            if (retryList.Count > 0)
            {
                var retryCmd = new NpgsqlCommand(@"
                    UPDATE jobs
                    SET
                        llm_location_retry_count = llm_location_retry_count + 1,
                        status = 'workplace_classified'
                    WHERE id = ANY(@ids)", conn, transaction);
                retryCmd.Parameters.AddWithValue("ids", retryList.ToArray());
                var retryRows = await retryCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Reset {retryRows} jobs to 'workplace_classified' for retry");
            }

            // Update failures
            if (failList.Count > 0)
            {
                var failCmd = new NpgsqlCommand(@"
                    UPDATE jobs
                    SET
                        llm_location_retry_count = llm_location_retry_count + 1,
                        status = 'failed - llm-location-generation',
                        is_valid = false
                    WHERE id = ANY(@ids)", conn, transaction);
                failCmd.Parameters.AddWithValue("ids", failList.ToArray());
                var failRows = await failCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Marked {failRows} jobs as 'failed - llm-location-generation' after 3 attempts");
            }

            await transaction.CommitAsync();

            stopwatch.Stop();
            context.Logger.LogInformation($"[PERF] Retry batch processing took {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error in retry batch: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }

    private class LocationRecord
    {
        public Guid JobId { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }
}

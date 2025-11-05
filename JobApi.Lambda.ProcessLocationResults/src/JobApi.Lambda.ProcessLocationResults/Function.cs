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
        var retryLocationIds = new HashSet<Guid>();
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
                var locationId = Guid.Parse(customId.Replace("location_", ""));

                // Check for error
                if (batchResponse.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
                {
                    retryLocationIds.Add(locationId);
                    continue;
                }

                // Get response content
                var responseBody = batchResponse.GetProperty("response").GetProperty("body");
                var choices = responseBody.GetProperty("choices");
                var message = choices[0].GetProperty("message");
                var contentStr = message.GetProperty("content").GetString();

                if (string.IsNullOrWhiteSpace(contentStr))
                {
                    retryLocationIds.Add(locationId);
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
                            retryLocationIds.Add(locationId);
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
                            retryLocationIds.Add(locationId);
                            continue;
                        }
                        country = countryValue;
                    }
                }

                successRecords.Add(new LocationRecord
                {
                    LocationId = locationId,
                    City = city,
                    State = state,
                    Country = country
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning($"Line {i + 1}: Parse error: {ex.Message}");
                parseErrors++;
            }
        }

        context.Logger.LogInformation($"Parsing complete: {successRecords.Count} successful, {retryLocationIds.Count} retries, {parseErrors} parse errors");

        // Process in batches
        var totalBatches = (int)Math.Ceiling((successRecords.Count + retryLocationIds.Count) / (double)BatchSize);
        var successBatches = successRecords.Chunk(BatchSize).ToList();
        var retryBatches = retryLocationIds.Chunk(BatchSize).ToList();

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
                        location_id UUID,
                        city TEXT,
                        state TEXT,
                        country TEXT
                    ) ON COMMIT DROP", conn, transaction))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // COPY data into temp table
                var copyStopwatch = Stopwatch.StartNew();
                await using (var writer = await conn.BeginBinaryImportAsync(
                    "COPY temp_location_updates (location_id, city, state, country) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var record in batch)
                    {
                        await writer.StartRowAsync();
                        await writer.WriteAsync(record.LocationId, NpgsqlTypes.NpgsqlDbType.Uuid);
                        await writer.WriteAsync(record.City, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.State, NpgsqlTypes.NpgsqlDbType.Text);
                        await writer.WriteAsync(record.Country, NpgsqlTypes.NpgsqlDbType.Text);
                    }
                    await writer.CompleteAsync();
                }
                copyStopwatch.Stop();
                context.Logger.LogInformation($"[PERF] COPY took {copyStopwatch.ElapsedMilliseconds}ms for {batch.Length} records");

                // UPDATE job_locations from temp table
                var updateStopwatch = Stopwatch.StartNew();
                await using (var cmd = new NpgsqlCommand(@"
                    UPDATE job_locations jl
                    SET
                        generated_city = t.city,
                        generated_state = t.state,
                        generated_country = t.country
                    FROM temp_location_updates t
                    WHERE jl.id = t.location_id", conn, transaction))
                {
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    updateStopwatch.Stop();
                    context.Logger.LogInformation($"[PERF] UPDATE took {updateStopwatch.ElapsedMilliseconds}ms, {rowsAffected} rows affected");
                }

                // Update job status for jobs where all locations are now classified
                var statusUpdateStopwatch = Stopwatch.StartNew();
                await using (var statusCmd = new NpgsqlCommand(@"
                    UPDATE jobs j
                    SET
                        status = CASE
                            WHEN EXISTS (
                                SELECT 1 FROM job_locations jl
                                WHERE jl.job_id = j.id AND jl.generated_country = 'US'
                            ) THEN 'location_classified'
                            ELSE 'invalid - non-us-location'
                        END,
                        is_valid = EXISTS (
                            SELECT 1 FROM job_locations jl
                            WHERE jl.job_id = j.id AND jl.generated_country = 'US'
                        )
                    WHERE j.id IN (
                        SELECT DISTINCT jl.job_id
                        FROM job_locations jl
                        INNER JOIN temp_location_updates t ON jl.id = t.location_id
                    )
                    AND NOT EXISTS (
                        SELECT 1 FROM job_locations jl2
                        WHERE jl2.job_id = j.id AND jl2.generated_city IS NULL
                    )", conn, transaction))
                {
                    var statusRows = await statusCmd.ExecuteNonQueryAsync();
                    statusUpdateStopwatch.Stop();
                    context.Logger.LogInformation($"[PERF] Status UPDATE took {statusUpdateStopwatch.ElapsedMilliseconds}ms, {statusRows} jobs updated");
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

        var retryLocationIds = new List<Guid>();

        // Parse all error lines to get location IDs
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var errorEntry = JsonSerializer.Deserialize<JsonElement>(line);
                var customId = errorEntry.GetProperty("custom_id").GetString()!;
                var locationId = Guid.Parse(customId.Replace("location_", ""));
                retryLocationIds.Add(locationId);
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning($"Line {i + 1}: Error parsing: {ex.Message}");
            }
        }

        context.Logger.LogInformation($"Parsed {retryLocationIds.Count} location IDs from error file");

        // Process retries in batches
        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        var batches = retryLocationIds.Chunk(BatchSize).ToList();
        var batchNum = 0;

        foreach (var batch in batches)
        {
            batchNum++;
            context.Logger.LogInformation($"[BATCH {batchNum}/{batches.Count}] Processing {batch.Length} error retries");
            await ProcessRetryBatch(conn, batch.ToList(), context);
        }
    }

    private async Task ProcessRetryBatch(NpgsqlConnection conn, List<Guid> locationIds, ILambdaContext context)
    {
        if (locationIds.Count == 0) return;

        var stopwatch = Stopwatch.StartNew();

        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Get current retry counts from job_locations
            var retryCountsCmd = new NpgsqlCommand(@"
                SELECT id, llm_location_retry_count, job_id
                FROM job_locations
                WHERE id = ANY(@ids)", conn, transaction);
            retryCountsCmd.Parameters.AddWithValue("ids", locationIds.ToArray());

            var retryCounts = new Dictionary<Guid, (int count, Guid jobId)>();
            await using (var reader = await retryCountsCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    retryCounts[reader.GetGuid(0)] = (reader.GetInt32(1), reader.GetGuid(2));
                }
            }

            // Separate into retry and fail lists
            var retryList = new List<Guid>();
            var failedJobIds = new HashSet<Guid>(); // Jobs that should be marked as failed

            foreach (var locationId in locationIds)
            {
                if (retryCounts.TryGetValue(locationId, out var info))
                {
                    var newCount = info.count + 1;
                    if (newCount >= 3)
                    {
                        failedJobIds.Add(info.jobId);
                    }
                    else
                    {
                        retryList.Add(locationId);
                    }
                }
            }

            // Update retry counts on job_locations
            if (retryList.Count > 0)
            {
                var retryCmd = new NpgsqlCommand(@"
                    UPDATE job_locations
                    SET llm_location_retry_count = llm_location_retry_count + 1
                    WHERE id = ANY(@ids)", conn, transaction);
                retryCmd.Parameters.AddWithValue("ids", retryList.ToArray());
                var retryRows = await retryCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Incremented retry count on {retryRows} locations for retry");

                // Reset job status to 'workplace_classified' for jobs with locations being retried
                var jobRetryCmd = new NpgsqlCommand(@"
                    UPDATE jobs
                    SET status = 'workplace_classified'
                    WHERE id IN (
                        SELECT DISTINCT job_id FROM job_locations WHERE id = ANY(@ids)
                    )", conn, transaction);
                jobRetryCmd.Parameters.AddWithValue("ids", retryList.ToArray());
                var jobRetryRows = await jobRetryCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Reset {jobRetryRows} jobs to 'workplace_classified' for location retry");
            }

            // Mark failed jobs
            if (failedJobIds.Count > 0)
            {
                // Increment retry count on failed locations
                var failLocationCmd = new NpgsqlCommand(@"
                    UPDATE job_locations
                    SET llm_location_retry_count = llm_location_retry_count + 1
                    WHERE job_id = ANY(@job_ids)", conn, transaction);
                failLocationCmd.Parameters.AddWithValue("job_ids", failedJobIds.ToArray());
                await failLocationCmd.ExecuteNonQueryAsync();

                // Mark jobs as failed
                var failCmd = new NpgsqlCommand(@"
                    UPDATE jobs
                    SET
                        status = 'failed - llm-location-generation',
                        is_valid = false
                    WHERE id = ANY(@ids)", conn, transaction);
                failCmd.Parameters.AddWithValue("ids", failedJobIds.ToArray());
                var failRows = await failCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Marked {failRows} jobs as 'failed - llm-location-generation' after 3 location classification attempts");
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
        public Guid LocationId { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }
}

using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Pgvector;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.ETL;

public class Function
{
    private readonly IAmazonS3 _s3Client;

    public Function()
    {
        _s3Client = new AmazonS3Client();
    }

    // Constructor for testing with mock S3 client
    public Function(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    /// <summary>
    /// Lambda handler for processing embedding results from S3
    /// Triggered when a file is uploaded to the embeddingresult folder
    /// Does exactly what Stage 14 does for JUST the file that triggered it
    /// </summary>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        foreach (var record in s3Event.Records)
        {
            var bucketName = record.S3.Bucket.Name;
            var key = record.S3.Object.Key;
            var fileName = Path.GetFileName(key);
            var processingKey = $"embeddingresult/processing/{fileName}";
            var errorKey = $"embeddingresult/error/{fileName}";

            context.Logger.LogInformation($"Attempting to claim file for processing: s3://{bucketName}/{key}");

            try
            {
                // Idempotency: Move file to processing folder. Only ONE Lambda instance will succeed.
                // This prevents duplicate processing when multiple instances are triggered.
                await _s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = key,
                    DestinationBucket = bucketName,
                    DestinationKey = processingKey
                });

                // Delete from original location
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                });

                context.Logger.LogInformation($"Successfully claimed file, now processing from: {processingKey}");

                await ProcessResultFile(bucketName, processingKey, context);

                // Success - delete from processing folder
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = processingKey
                });

                context.Logger.LogInformation($"Successfully processed and deleted: {processingKey}");
            }
            catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // File was already moved by another Lambda instance - skip processing
                context.Logger.LogInformation($"File already claimed by another instance: {key}");
                return;
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing {key}: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");

                // Move to error folder for manual review - CRITICAL for data safety
                try
                {
                    // First verify the file still exists in processing folder
                    context.Logger.LogError($"Attempting to move {processingKey} to error folder...");

                    await _s3Client.CopyObjectAsync(new CopyObjectRequest
                    {
                        SourceBucket = bucketName,
                        SourceKey = processingKey,
                        DestinationBucket = bucketName,
                        DestinationKey = errorKey
                    });

                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = processingKey
                    });

                    context.Logger.LogError($"Successfully moved failed file to: {errorKey}");
                }
                catch (AmazonS3Exception s3MoveEx) when (s3MoveEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    context.Logger.LogError($"CRITICAL: Processing file no longer exists at {processingKey} - file may be lost!");
                    context.Logger.LogError($"This should never happen - investigate immediately!");
                }
                catch (Exception moveEx)
                {
                    context.Logger.LogError($"CRITICAL: Failed to move file to error folder: {moveEx.Message}");
                    context.Logger.LogError($"File is still at: {processingKey}");
                }

                // DO NOT throw - file already moved to error or lost, retrying won't help
            }
        }
    }

    private async Task ProcessResultFile(string bucketName, string key, ILambdaContext context)
    {
        var successCount = 0;
        var errorCount = 0;
        var lineNumber = 0;
        var batchSize = 500; // Small batches to keep UPDATE under 3min timeout
        var updates = new List<(Guid jobId, Vector embedding)>(capacity: batchSize);

        // Create DbContext and get Npgsql connection
        await using var db = JobContext.Create();
        var npgConn = (NpgsqlConnection)db.Database.GetDbConnection();
        if (npgConn.State != System.Data.ConnectionState.Open)
            await npgConn.OpenAsync();

        await using var tx = await npgConn.BeginTransactionAsync();

        // Create temp table once per transaction and set synchronous_commit off
        const string initSql = @"
            CREATE TEMP TABLE IF NOT EXISTS tmp_embeddings (
                job_id uuid PRIMARY KEY,
                embedding vector(1536)
            ) ON COMMIT PRESERVE ROWS;
            SET LOCAL synchronous_commit = off;";

        await using (var initCmd = new NpgsqlCommand(initSql, npgConn, tx))
        {
            initCmd.CommandTimeout = 180; // 3 minutes
            await initCmd.ExecuteNonQueryAsync();
        }

        try
        {
            // Download and stream the file from S3
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(getObjectRequest);
            using var reader = new StreamReader(response.ResponseStream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    // Parse JSON - same logic as Stage 14
                    var batchResponse = JsonSerializer.Deserialize<EmbeddingBatchResult>(line);
                    if (batchResponse == null) continue;

                    // Get the custom_id which contains the job ID
                    var customId = batchResponse.CustomId;
                    var jobIdString = customId.Replace("job_", "");
                    var jobId = Guid.Parse(jobIdString);

                    // Check if the response has an error
                    if (batchResponse.Error != null)
                    {
                        context.Logger.LogWarning($"Line {lineNumber}: Error for job {jobId}: {batchResponse.Error.Message}");
                        errorCount++;
                        continue;
                    }

                    // Check if request was successful
                    if (batchResponse.Response?.StatusCode != 200)
                    {
                        context.Logger.LogWarning($"Line {lineNumber}: Error for job {jobId}: Status {batchResponse.Response?.StatusCode}");
                        errorCount++;
                        continue;
                    }

                    // Extract embedding vector
                    var embeddingData = batchResponse.Response?.Body?.Data?.FirstOrDefault()?.Embedding;
                    if (embeddingData == null || embeddingData.Length != 1536)
                    {
                        context.Logger.LogWarning($"Line {lineNumber}: Invalid embedding for job {jobId}");
                        errorCount++;
                        continue;
                    }

                    // Convert to pgvector Vector type
                    var vector = new Vector(embeddingData);
                    updates.Add((jobId, vector));

                    // Flush in large chunks - COPY loves bigger batches
                    if (updates.Count >= batchSize)
                    {
                        try
                        {
                            await UpdateEmbeddingsFastAsync(npgConn, tx, updates, context);
                            successCount += updates.Count;
                            context.Logger.LogInformation($"Updated {successCount} embeddings (line {lineNumber})...");
                            updates.Clear();
                        }
                        catch (Exception dbEx)
                        {
                            context.Logger.LogError($"FATAL: Database update failed at line {lineNumber}: {dbEx.Message}");
                            throw; // Fail the file immediately
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    context.Logger.LogWarning($"Line {lineNumber}: JSON parsing error: {jsonEx.Message}");
                    errorCount++;
                }
                catch (Exception ex) when (ex is not JsonException)
                {
                    // This will catch the rethrown database exception
                    throw;
                }
            }

            // Final flush
            if (updates.Count > 0)
            {
                await UpdateEmbeddingsFastAsync(npgConn, tx, updates, context);
                successCount += updates.Count;
                context.Logger.LogInformation($"Updated final batch. Total: {successCount} embeddings");
            }

            // Commit transaction - if this fails, the entire function will throw
            context.Logger.LogInformation($"Committing transaction for {successCount} total embeddings...");
            await tx.CommitAsync();
            context.Logger.LogInformation($"Transaction committed successfully");

            context.Logger.LogInformation($"Processed {successCount} job(s) successfully");
            if (errorCount > 0)
            {
                context.Logger.LogInformation($"{errorCount} error(s) encountered");
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Transaction failed, rolling back: {ex.Message}");
            try
            {
                await tx.RollbackAsync();
                context.Logger.LogError($"Transaction rolled back successfully");
            }
            catch (Exception rbEx)
            {
                context.Logger.LogError($"Rollback also failed: {rbEx.Message}");
            }
            throw; // Rethrow to trigger error file handling
        }
    }

    private static async Task UpdateEmbeddingsFastAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        IReadOnlyList<(Guid id, Vector embedding)> rows,
        ILambdaContext context)
    {
        if (rows.Count == 0) return;

        var totalTimer = System.Diagnostics.Stopwatch.StartNew();

        // TRUNCATE clears the temp table for this batch (table was created at transaction start)
        const string truncateSql = "TRUNCATE tmp_embeddings;";

        await using (var truncateCmd = new NpgsqlCommand(truncateSql, conn, tx))
        {
            truncateCmd.CommandTimeout = 180; // 3 minutes
            await truncateCmd.ExecuteNonQueryAsync();
        }

        // COPY (binary) is orders of magnitude faster than text literals
        var copyTimer = System.Diagnostics.Stopwatch.StartNew();
        await using (var writer = await conn.BeginBinaryImportAsync(
            "COPY tmp_embeddings (job_id, embedding) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var (id, emb) in rows)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(id, NpgsqlDbType.Uuid);
                // Pgvector type handler lets us write Vector directly
                await writer.WriteAsync(emb);
            }
            await writer.CompleteAsync();
        }
        copyTimer.Stop();
        context.Logger.LogInformation($"[PERF] COPY took {copyTimer.ElapsedMilliseconds}ms for {rows.Count} rows");

        // UPSERT into job_embeddings (insert if not exists, update if exists)
        // Also copy generated_workplace from jobs table
        var updateTimer = System.Diagnostics.Stopwatch.StartNew();
        const string upsertSql = @"
            INSERT INTO job_embeddings (job_id, embedding, generated_workplace)
            SELECT t.job_id, t.embedding, j.generated_workplace
            FROM tmp_embeddings t
            INNER JOIN jobs j ON t.job_id = j.id
            ON CONFLICT (job_id) DO UPDATE
            SET embedding = EXCLUDED.embedding,
                generated_workplace = EXCLUDED.generated_workplace;

            UPDATE jobs j
            SET status = 'embedded'
            FROM tmp_embeddings t
            WHERE j.id = t.job_id
              AND j.status != 'embedded';";

        await using (var cmd = new NpgsqlCommand(upsertSql, conn, tx))
        {
            cmd.CommandTimeout = 180; // 3 minutes
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            updateTimer.Stop();
            context.Logger.LogInformation($"[PERF] UPSERT/UPDATE took {updateTimer.ElapsedMilliseconds}ms, affected {rowsAffected} rows");
        }

        // Delete existing centroid assignments for these jobs
        var deleteTimer = System.Diagnostics.Stopwatch.StartNew();
        const string deleteSql = @"
            DELETE FROM centroid_assignments ca
            USING tmp_embeddings t
            WHERE ca.job_id = t.job_id;";

        await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
        {
            deleteCmd.CommandTimeout = 180; // 3 minutes
            var deletedRows = await deleteCmd.ExecuteNonQueryAsync();
            deleteTimer.Stop();
            context.Logger.LogInformation($"[PERF] DELETE centroid_assignments took {deleteTimer.ElapsedMilliseconds}ms, deleted {deletedRows} rows");
        }

        // Find 6 closest centroids for each job and insert assignments
        // Only process jobs that exist in the jobs table (old batches may contain deleted jobs)
        var centroidTimer = System.Diagnostics.Stopwatch.StartNew();
        const string centroidSql = @"
            INSERT INTO centroid_assignments (job_id, centroid_id)
            SELECT job_id, centroid_id
            FROM (
                SELECT
                    t.job_id,
                    c.id as centroid_id,
                    ROW_NUMBER() OVER (PARTITION BY t.job_id ORDER BY t.embedding <=> c.centroid) as rank
                FROM tmp_embeddings t
                INNER JOIN jobs j ON t.job_id = j.id
                CROSS JOIN centroids c
            ) ranked
            WHERE rank <= 6;";

        await using (var centroidCmd = new NpgsqlCommand(centroidSql, conn, tx))
        {
            centroidCmd.CommandTimeout = 180; // 3 minutes
            var centroidRows = await centroidCmd.ExecuteNonQueryAsync();
            centroidTimer.Stop();
            context.Logger.LogInformation($"[PERF] INSERT centroid_assignments took {centroidTimer.ElapsedMilliseconds}ms, inserted {centroidRows} rows ({centroidRows / rows.Count} per job)");
        }

        totalTimer.Stop();
        context.Logger.LogInformation($"[PERF] TOTAL UpdateEmbeddingsFast took {totalTimer.ElapsedMilliseconds}ms");
    }
}

// JSON structure classes for batch results - same as Stage 14
public class EmbeddingBatchResult
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("response")]
    public EmbeddingBatchResponse? Response { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("error")]
    public EmbeddingBatchError? Error { get; set; }
}

public class EmbeddingBatchResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("body")]
    public EmbeddingResponseBody? Body { get; set; }
}

public class EmbeddingResponseBody
{
    [System.Text.Json.Serialization.JsonPropertyName("object")]
    public string? Object { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public List<EmbeddingData>? Data { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("model")]
    public string? Model { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("usage")]
    public EmbeddingUsageData? Usage { get; set; }
}

public class EmbeddingData
{
    [System.Text.Json.Serialization.JsonPropertyName("object")]
    public string? Object { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("index")]
    public int Index { get; set; }
}

public class EmbeddingUsageData
{
    [System.Text.Json.Serialization.JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class EmbeddingBatchError
{
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string? Message { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string? Type { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string? Code { get; set; }
}

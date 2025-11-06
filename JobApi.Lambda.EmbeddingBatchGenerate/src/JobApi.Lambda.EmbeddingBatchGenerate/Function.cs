using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.EmbeddingBatchGenerate;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private const int BatchSize = 5000;
    private const int MaxInputLength = 32000; // ~8000 tokens for text-embedding-3-small

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

    public async Task FunctionHandler(ILambdaContext context)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var runId = Guid.NewGuid(); // Unique ID for this run to make batch filenames unique

        context.Logger.LogInformation("=== Embedding Batch Generate Started ===");
        context.Logger.LogInformation($"Run ID: {runId}");

        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        // Count total jobs needing embeddings
        context.Logger.LogInformation("Checking for jobs needing embeddings...");

        await using var countCmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            LEFT JOIN job_embeddings je ON je.job_id = j.id
            WHERE (je.job_id IS NULL OR je.embedding IS NULL)
              AND j.is_valid = true
              AND j.status != 'embedding_batch_sent'", conn);

        var totalCount = (long)(await countCmd.ExecuteScalarAsync() ?? 0L);

        if (totalCount == 0)
        {
            context.Logger.LogInformation("No valid US jobs needing embeddings.");
            context.Logger.LogInformation("=== Embedding Batch Generate Complete ===");
            return;
        }

        context.Logger.LogInformation($"Found {totalCount} valid US jobs needing embeddings");

        var estimatedBatches = (int)Math.Ceiling((double)totalCount / BatchSize);
        context.Logger.LogInformation($"Creating ~{estimatedBatches} batch file(s) (batch size: {BatchSize})");

        var processedCount = 0;
        var batchNum = 1;
        var currentBatchJobs = new List<EmbeddingBatchData>();

        while (processedCount < totalCount)
        {
            var chunkSize = 10000; // Load 10k at a time
            var jobs = new List<EmbeddingBatchData>();

            await using (var cmd = new NpgsqlCommand(@"
                SELECT j.id, j.job_title, j.job_description
                FROM jobs j
                LEFT JOIN job_embeddings je ON je.job_id = j.id
                WHERE (je.job_id IS NULL OR je.embedding IS NULL)
                  AND j.is_valid = true
                  AND j.status != 'embedding_batch_sent'
                ORDER BY j.id
                LIMIT @limit OFFSET @offset", conn))
            {
                cmd.Parameters.AddWithValue("limit", chunkSize);
                cmd.Parameters.AddWithValue("offset", processedCount);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    jobs.Add(new EmbeddingBatchData
                    {
                        Id = reader.GetGuid(0),
                        JobTitle = reader.IsDBNull(1) ? null : reader.GetString(1),
                        JobDescription = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }
            }

            if (jobs.Count == 0)
                break;

            // Add to current batch
            currentBatchJobs.AddRange(jobs);

            // Write out complete batches
            while (currentBatchJobs.Count >= BatchSize)
            {
                var batchToWrite = currentBatchJobs.Take(BatchSize).ToList();
                currentBatchJobs.RemoveRange(0, BatchSize);

                var fileName = $"embedding_batch_{runId}_{batchNum}.jsonl";

                context.Logger.LogInformation($"Generating batch {batchNum}: {fileName} ({batchToWrite.Count} jobs)");
                await GenerateAndUploadBatchFile(batchToWrite, fileName, conn, context);
                batchNum++;
            }

            processedCount += jobs.Count;
            context.Logger.LogInformation($"Processed {processedCount}/{totalCount} jobs...");
        }

        // Write remaining jobs as final batch
        if (currentBatchJobs.Count > 0)
        {
            var fileName = $"embedding_batch_{runId}_{batchNum}.jsonl";

            context.Logger.LogInformation($"Generating batch {batchNum}: {fileName} ({currentBatchJobs.Count} jobs)");
            await GenerateAndUploadBatchFile(currentBatchJobs, fileName, conn, context);
        }

        totalStopwatch.Stop();
        context.Logger.LogInformation($"[PERF] Total execution time: {totalStopwatch.ElapsedMilliseconds}ms");
        context.Logger.LogInformation("=== Embedding Batch Generate Complete ===");
    }

    private async Task GenerateAndUploadBatchFile(List<EmbeddingBatchData> jobs, string fileName, NpgsqlConnection conn, ILambdaContext context)
    {
        var uploadStopwatch = Stopwatch.StartNew();

        // Stream JSONL content directly to avoid building huge strings in memory
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            foreach (var job in jobs)
            {
                var batchRequest = CreateBatchRequest(job);
                var json = JsonSerializer.Serialize(batchRequest);
                await writer.WriteLineAsync(json);
            }
            await writer.FlushAsync();
        }

        stream.Position = 0;
        var streamLength = stream.Length;

        // Upload to S3
        var s3Key = $"embeddingbatch/intake/{fileName}";

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = stream,
            ContentType = "application/x-ndjson"
        });

        uploadStopwatch.Stop();
        context.Logger.LogInformation($"[PERF] Generated and uploaded {fileName} in {uploadStopwatch.ElapsedMilliseconds}ms ({streamLength} bytes)");

        // Update job status to 'embedding_batch_sent' for all jobs in this batch
        var updateStopwatch = Stopwatch.StartNew();
        var jobIds = jobs.Select(j => j.Id).ToList();

        await using var updateCmd = new NpgsqlCommand(@"
            UPDATE jobs
            SET status = 'embedding_batch_sent'
            WHERE id = ANY(@ids)", conn);
        updateCmd.Parameters.AddWithValue("ids", jobIds);

        var rowsUpdated = await updateCmd.ExecuteNonQueryAsync();
        updateStopwatch.Stop();

        context.Logger.LogInformation($"[PERF] Updated {rowsUpdated} job statuses to 'embedding_batch_sent' in {updateStopwatch.ElapsedMilliseconds}ms");
    }

    private object CreateBatchRequest(EmbeddingBatchData job)
    {
        // Combine job title and description
        var input = $"{job.JobTitle}\n\n{job.JobDescription}";

        // Truncate if too long (max ~8000 tokens for text-embedding-3-small)
        // Rough estimate: 1 token ≈ 4 chars, so ~32000 chars max
        if (input.Length > MaxInputLength)
        {
            input = input.Substring(0, MaxInputLength);
        }

        return new
        {
            custom_id = $"job_{job.Id}",
            method = "POST",
            url = "/v1/embeddings",
            body = new
            {
                model = "text-embedding-3-small",
                input = input,
                input_type = "document"
            }
        };
    }

    private class EmbeddingBatchData
    {
        public Guid Id { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
    }
}

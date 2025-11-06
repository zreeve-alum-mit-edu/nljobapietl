using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JobApi.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.LlmBatchGenerate;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private const int BatchSize = 25000;
    private const int DescriptionMaxLength = 2000;

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
    /// Lambda handler for generating LLM batch files for workplace classification
    /// Triggered when a file is uploaded to the ingested/ folder
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
                await GenerateLlmBatches(bucketName, context);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error generating LLM batches: {ex.Message}");
                context.Logger.LogError($"Full exception: {ex}");

                // Set any jobs stuck in 'llm_batches_generating' to failed status
                try
                {
                    await ResetFailedJobs(context);
                }
                catch (Exception resetEx)
                {
                    context.Logger.LogError($"CRITICAL: Failed to reset job statuses: {resetEx.Message}");
                }

                // DO NOT throw - we've handled the error by marking jobs as failed
            }
        }
    }

    private async Task GenerateLlmBatches(string bucketName, ILambdaContext context)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var batchJobIds = new List<List<Guid>>();

        await using var db = JobContext.Create();
        var npgConn = (NpgsqlConnection)db.Database.GetDbConnection();
        if (npgConn.State != System.Data.ConnectionState.Open)
            await npgConn.OpenAsync();

        await using var tx = await npgConn.BeginTransactionAsync();

        try
        {
            // SELECT FOR UPDATE locks the rows, preventing other Lambda instances from grabbing them
            context.Logger.LogInformation("Querying and locking jobs with status = 'ingested'...");

            // Query jobs and join with job_locations to get location data
            // Use subquery to lock jobs first, then join with aggregated locations
            const string selectSql = @"
                SELECT locked.id, locked.job_title, locked.company_name, locked.job_description,
                       STRING_AGG(DISTINCT jl.locality, ' / ') as localities,
                       STRING_AGG(DISTINCT jl.region, ' / ') as regions,
                       STRING_AGG(DISTINCT jl.country, ' / ') as countries,
                       STRING_AGG(DISTINCT jl.location, ' / ') as locations
                FROM (
                    SELECT j.id, j.job_title, j.company_name, j.job_description
                    FROM jobs j
                    WHERE j.status = 'ingested'
                    FOR UPDATE
                ) locked
                LEFT JOIN job_locations jl ON locked.id = jl.job_id
                GROUP BY locked.id, locked.job_title, locked.company_name, locked.job_description";

            var jobs = new List<JobBatchData>();

            await using (var cmd = new NpgsqlCommand(selectSql, npgConn, tx))
            {
                cmd.CommandTimeout = 300; // 5 minutes
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    jobs.Add(new JobBatchData
                    {
                        Id = reader.GetGuid(0),
                        JobTitle = reader.IsDBNull(1) ? null : reader.GetString(1),
                        CompanyName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        JobDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Locality = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Region = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Country = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Location = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }
            }

            if (jobs.Count == 0)
            {
                context.Logger.LogInformation("No jobs with status 'ingested' found.");
                await tx.CommitAsync();
                return;
            }

            context.Logger.LogInformation($"Found {jobs.Count} jobs to process");

            // Immediately update status to 'llm_batches_generating' while we still have the lock
            var jobIds = jobs.Select(j => j.Id).ToList();

            const string updateSql = @"
                UPDATE jobs
                SET status = 'llm_batches_generating'
                WHERE id = ANY(@ids)";

            await using (var updateCmd = new NpgsqlCommand(updateSql, npgConn, tx))
            {
                updateCmd.Parameters.AddWithValue("ids", jobIds.ToArray());
                updateCmd.CommandTimeout = 300;
                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                context.Logger.LogInformation($"Updated {rowsAffected} jobs to 'llm_batches_generating'");
            }

            // Commit transaction - releases the lock
            await tx.CommitAsync();
            context.Logger.LogInformation("Transaction committed, lock released");

            // Now generate batch files (outside the transaction)
            var batches = jobs
                .Select((job, index) => new { job, index })
                .GroupBy(x => x.index / BatchSize)
                .Select(g => g.Select(x => x.job).ToList())
                .ToList();

            context.Logger.LogInformation($"Creating {batches.Count} batch file(s)");

            for (int batchNum = 0; batchNum < batches.Count; batchNum++)
            {
                var batch = batches[batchNum];
                var fileName = $"workplace_batch_{timestamp}_{batchNum + 1}.jsonl";

                context.Logger.LogInformation($"Generating batch {batchNum + 1}/{batches.Count}: {fileName} ({batch.Count} jobs)");

                // Generate batch file content
                var batchContent = GenerateBatchFileContent(batch);

                // Upload directly to S3
                var s3Key = $"llmbatch/createdbatches/{fileName}";
                await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(batchContent));

                await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = stream
                });

                context.Logger.LogInformation($"Uploaded to s3://{bucketName}/{s3Key}");

                // Track which job IDs are in this batch
                batchJobIds.Add(batch.Select(j => j.Id).ToList());
            }

            // Update all jobs to 'llm_batches_generated'
            var allJobIds = batchJobIds.SelectMany(x => x).ToList();
            await UpdateJobsStatus(allJobIds, "llm_batches_generated", context);

            context.Logger.LogInformation($"Successfully generated {batches.Count} batch file(s) for {jobs.Count} jobs");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error during batch generation: {ex.Message}");
            await tx.RollbackAsync();
            throw; // Will be caught by outer handler
        }
    }

    private string GenerateBatchFileContent(List<JobBatchData> jobs)
    {
        var lines = new List<string>();

        foreach (var job in jobs)
        {
            var batchRequest = CreateBatchRequest(job);
            var json = JsonSerializer.Serialize(batchRequest);
            lines.Add(json);
        }

        return string.Join("\n", lines);
    }

    private object CreateBatchRequest(JobBatchData job)
    {
        // Build location context
        var locationParts = new List<string>();
        if (!string.IsNullOrEmpty(job.Locality)) locationParts.Add(job.Locality);
        if (!string.IsNullOrEmpty(job.Region)) locationParts.Add(job.Region);
        if (!string.IsNullOrEmpty(job.Country)) locationParts.Add(job.Country);
        var locationContext = locationParts.Count > 0
            ? string.Join(", ", locationParts)
            : job.Location ?? "Not specified";

        // Truncate description
        string description = job.JobDescription ?? "No description provided";
        if (description.Length > DescriptionMaxLength)
        {
            description = description.Substring(0, DescriptionMaxLength) + "...";
        }

        // Build user message
        var userMessage = $@"Title: {job.JobTitle ?? "Not specified"}
Company: {job.CompanyName ?? "Not specified"}
Location: {locationContext}
Description: {description}";

        var systemPrompt = @"You are a workplace type classifier. Analyze the job posting and determine the workplace type.

Respond with ONLY a JSON object in this format:
{""type"":""REMOTE|HYBRID|ONSITE"",""inferred"":true|false,""confidence"":""EXPLICIT|LIKELY|PROBABLY|GUESS""}

- type: REMOTE, HYBRID, or ONSITE
- inferred: true if the workplace type is not explicitly stated, false if it is clearly stated
- confidence: EXPLICIT if clearly stated, LIKELY if strong indicators, PROBABLY if moderate indicators, GUESS if weak indicators";

        return new
        {
            custom_id = $"job_{job.Id}",
            method = "POST",
            url = "/v1/chat/completions",
            body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_completion_tokens = 2000,
                response_format = new { type = "json_object" }
            }
        };
    }

    private async Task UpdateJobsStatus(List<Guid> jobIds, string status, ILambdaContext context)
    {
        await using var db = JobContext.Create();
        var npgConn = (NpgsqlConnection)db.Database.GetDbConnection();
        if (npgConn.State != System.Data.ConnectionState.Open)
            await npgConn.OpenAsync();

        const string updateSql = @"
            UPDATE jobs
            SET status = @status
            WHERE id = ANY(@ids)";

        await using var cmd = new NpgsqlCommand(updateSql, npgConn);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("ids", jobIds.ToArray());
        cmd.CommandTimeout = 300;

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        context.Logger.LogInformation($"Updated {rowsAffected} jobs to status '{status}'");
    }

    private async Task ResetFailedJobs(ILambdaContext context)
    {
        context.Logger.LogInformation("Resetting failed jobs to 'llm_batch_generation_failed' status...");

        await using var db = JobContext.Create();
        var npgConn = (NpgsqlConnection)db.Database.GetDbConnection();
        if (npgConn.State != System.Data.ConnectionState.Open)
            await npgConn.OpenAsync();

        const string resetSql = @"
            UPDATE jobs
            SET status = 'llm_batch_generation_failed'
            WHERE status = 'llm_batches_generating'";

        await using var cmd = new NpgsqlCommand(resetSql, npgConn);
        cmd.CommandTimeout = 300;

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        context.Logger.LogInformation($"Reset {rowsAffected} jobs to 'llm_batch_generation_failed'");
    }
}

public class JobBatchData
{
    public Guid Id { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Location { get; set; }
    public string? JobDescription { get; set; }
}

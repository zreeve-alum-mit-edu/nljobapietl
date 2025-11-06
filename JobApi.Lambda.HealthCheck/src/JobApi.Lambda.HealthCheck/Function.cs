using System.Diagnostics;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using JobApi.Common;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.HealthCheck;

public class Function
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string? _snsTopicArn;

    public Function()
    {
        _snsClient = new AmazonSimpleNotificationServiceClient();
        _snsTopicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN");
    }

    public Function(IAmazonSimpleNotificationService snsClient, string? snsTopicArn)
    {
        _snsClient = snsClient;
        _snsTopicArn = snsTopicArn;
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var issues = new List<string>();
        var warnings = new List<string>();

        context.Logger.LogInformation("=== Database Health Check Started ===");

        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        // Check 1: Jobs missing embeddings
        context.Logger.LogInformation("Check 1: Looking for jobs missing embeddings...");
        var jobsMissingEmbeddings = await CheckJobsMissingEmbeddings(conn, context);
        if (jobsMissingEmbeddings > 0)
        {
            var message = $"ISSUE: {jobsMissingEmbeddings} job(s) are missing embeddings";
            context.Logger.LogError(message);
            issues.Add(message);
        }
        else
        {
            context.Logger.LogInformation("✓ All jobs have embeddings");
        }

        // Check 2: Jobs missing locations
        context.Logger.LogInformation("Check 2: Looking for jobs missing locations...");
        var jobsMissingLocations = await CheckJobsMissingLocations(conn, context);
        if (jobsMissingLocations > 0)
        {
            var message = $"ISSUE: {jobsMissingLocations} job(s) are missing locations";
            context.Logger.LogError(message);
            issues.Add(message);
        }
        else
        {
            context.Logger.LogInformation("✓ All jobs have locations");
        }

        // Check 3: Jobs missing URLs
        context.Logger.LogInformation("Check 3: Looking for jobs missing URLs...");
        var jobsMissingUrls = await CheckJobsMissingUrls(conn, context);
        if (jobsMissingUrls > 0)
        {
            var message = $"ISSUE: {jobsMissingUrls} job(s) are missing URLs";
            context.Logger.LogError(message);
            issues.Add(message);
        }
        else
        {
            context.Logger.LogInformation("✓ All jobs have URLs");
        }

        // Check 4: Jobs with embeddings missing exactly 6 centroid assignments
        context.Logger.LogInformation("Check 4: Looking for jobs with incorrect centroid assignment counts...");
        var jobsWithIncorrectCentroids = await CheckCentroidAssignments(conn, context);
        if (jobsWithIncorrectCentroids > 0)
        {
            var message = $"ISSUE: {jobsWithIncorrectCentroids} job(s) with embeddings don't have exactly 6 centroid assignments";
            context.Logger.LogError(message);
            issues.Add(message);
        }
        else
        {
            context.Logger.LogInformation("✓ All jobs with embeddings have exactly 6 centroid assignments");
        }

        // Check 5: Jobs stuck in non-embedded status for > 48 hours
        context.Logger.LogInformation("Check 5: Looking for jobs stuck in processing (status unchanged for 48+ hours)...");
        var stuckJobs = await CheckStuckJobs(conn, context);
        if (stuckJobs > 0)
        {
            var message = $"WARNING: {stuckJobs} job(s) have been stuck in non-embedded status for 48+ hours";
            context.Logger.LogWarning(message);
            warnings.Add(message);
        }
        else
        {
            context.Logger.LogInformation("✓ No jobs stuck in processing");
        }

        totalStopwatch.Stop();

        // Summary
        context.Logger.LogInformation("\n=== Health Check Summary ===");
        context.Logger.LogInformation($"Total issues: {issues.Count}");
        context.Logger.LogInformation($"Total warnings: {warnings.Count}");
        context.Logger.LogInformation($"Execution time: {totalStopwatch.ElapsedMilliseconds}ms");

        // Send SNS alert if there are issues or warnings
        if ((issues.Count > 0 || warnings.Count > 0) && !string.IsNullOrEmpty(_snsTopicArn))
        {
            await SendSnsAlert(issues, warnings, context);
        }

        context.Logger.LogInformation("=== Database Health Check Complete ===");
    }

    private async Task<long> CheckJobsMissingEmbeddings(NpgsqlConnection conn, ILambdaContext context)
    {
        await using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            WHERE NOT EXISTS (SELECT 1 FROM job_embeddings je WHERE je.job_id = j.id)
              AND j.is_valid = true
              AND j.status = 'embedded'", conn);

        return (long)(await cmd.ExecuteScalarAsync() ?? 0L);
    }

    private async Task<long> CheckJobsMissingLocations(NpgsqlConnection conn, ILambdaContext context)
    {
        await using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            WHERE NOT EXISTS (SELECT 1 FROM job_locations jl WHERE jl.job_id = j.id)
              AND j.is_valid = true
              AND j.status = 'embedded'", conn);

        return (long)(await cmd.ExecuteScalarAsync() ?? 0L);
    }

    private async Task<long> CheckJobsMissingUrls(NpgsqlConnection conn, ILambdaContext context)
    {
        await using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            WHERE NOT EXISTS (
                SELECT 1
                FROM job_locations jl
                INNER JOIN job_location_urls jlu ON jl.id = jlu.job_location_id
                WHERE jl.job_id = j.id
            )
            AND j.is_valid = true
            AND j.status = 'embedded'", conn);

        return (long)(await cmd.ExecuteScalarAsync() ?? 0L);
    }

    private async Task<long> CheckCentroidAssignments(NpgsqlConnection conn, ILambdaContext context)
    {
        await using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            WHERE EXISTS (SELECT 1 FROM job_embeddings je WHERE je.job_id = j.id)
              AND j.is_valid = true
              AND (
                SELECT COUNT(*)
                FROM centroid_assignments ca
                WHERE ca.job_id = j.id
              ) != 6", conn);

        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);

        // Get sample details for logging
        if (count > 0)
        {
            await using var detailCmd = new NpgsqlCommand(@"
                SELECT j.id,
                       (SELECT COUNT(*) FROM centroid_assignments ca WHERE ca.job_id = j.id) as centroid_count,
                       j.status
                FROM jobs j
                WHERE EXISTS (SELECT 1 FROM job_embeddings je WHERE je.job_id = j.id)
                  AND j.is_valid = true
                  AND (
                    SELECT COUNT(*)
                    FROM centroid_assignments ca
                    WHERE ca.job_id = j.id
                  ) != 6
                LIMIT 10", conn);

            await using var reader = await detailCmd.ExecuteReaderAsync();
            context.Logger.LogInformation("Sample jobs with incorrect centroid counts:");
            while (await reader.ReadAsync())
            {
                var jobId = reader.GetGuid(0);
                var centroidCount = reader.GetInt64(1);
                var status = reader.GetString(2);
                context.Logger.LogInformation($"  Job {jobId}: {centroidCount} centroids, status={status}");
            }
        }

        return count;
    }

    private async Task<long> CheckStuckJobs(NpgsqlConnection conn, ILambdaContext context)
    {
        await using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs j
            WHERE j.status != 'embedded'
              AND j.status_change_date < NOW() - INTERVAL '48 hours'
              AND j.is_valid = true", conn);

        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);

        // Get sample details for logging
        if (count > 0)
        {
            await using var detailCmd = new NpgsqlCommand(@"
                SELECT j.status, COUNT(*) as count,
                       MIN(j.status_change_date) as oldest_change,
                       MAX(j.status_change_date) as newest_change
                FROM jobs j
                WHERE j.status != 'embedded'
                  AND j.status_change_date < NOW() - INTERVAL '48 hours'
                  AND j.is_valid = true
                GROUP BY j.status
                ORDER BY count DESC", conn);

            await using var reader = await detailCmd.ExecuteReaderAsync();
            context.Logger.LogInformation("Stuck jobs by status:");
            while (await reader.ReadAsync())
            {
                var status = reader.GetString(0);
                var statusCount = reader.GetInt64(1);
                var oldest = reader.GetDateTime(2);
                var newest = reader.GetDateTime(3);
                context.Logger.LogInformation($"  {status}: {statusCount} jobs (oldest: {oldest:yyyy-MM-dd HH:mm}, newest: {newest:yyyy-MM-dd HH:mm})");
            }
        }

        return count;
    }

    private async Task SendSnsAlert(List<string> issues, List<string> warnings, ILambdaContext context)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Job API Database Health Check Alert");
            sb.AppendLine("===================================");
            sb.AppendLine();

            if (issues.Count > 0)
            {
                sb.AppendLine("ISSUES:");
                foreach (var issue in issues)
                {
                    sb.AppendLine($"  • {issue}");
                }
                sb.AppendLine();
            }

            if (warnings.Count > 0)
            {
                sb.AppendLine("WARNINGS:");
                foreach (var warning in warnings)
                {
                    sb.AppendLine($"  • {warning}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            await _snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = _snsTopicArn,
                Subject = $"Job API Health Check: {issues.Count} issue(s), {warnings.Count} warning(s)",
                Message = sb.ToString()
            });

            context.Logger.LogInformation("SNS alert sent successfully");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to send SNS alert: {ex.Message}");
        }
    }
}

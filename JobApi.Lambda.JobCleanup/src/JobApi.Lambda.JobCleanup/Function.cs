using Amazon.Lambda.Core;
using Npgsql;
using JobApi.Common;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.JobCleanup;

public class Function
{
    private const int RetentionDays = 90;

    /// <summary>
    /// Lambda handler for deleting jobs older than 90 days
    /// Triggered twice daily by EventBridge
    /// </summary>
    public async Task FunctionHandler(ILambdaContext context)
    {
        context.Logger.LogInformation("=== Job Cleanup Started ===");
        context.Logger.LogInformation($"Deleting jobs older than {RetentionDays} days...");

        using var conn = new NpgsqlConnection(JobContext.GetConnectionString());
        await conn.OpenAsync();

        var cutoffDate = DateTime.UtcNow.AddDays(-RetentionDays);
        context.Logger.LogInformation($"Cutoff date: {cutoffDate:yyyy-MM-dd HH:mm:ss} UTC");

        // First, count how many jobs will be deleted
        await using (var countCmd = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM jobs
            WHERE date_posted < @cutoffDate", conn))
        {
            countCmd.Parameters.AddWithValue("cutoffDate", cutoffDate);
            var count = (long)(await countCmd.ExecuteScalarAsync() ?? 0L);
            context.Logger.LogInformation($"Found {count} job(s) to delete");

            if (count == 0)
            {
                context.Logger.LogInformation("No jobs to delete");
                context.Logger.LogInformation("=== Job Cleanup Complete ===");
                return;
            }
        }

        // Delete jobs older than retention period
        // Foreign keys with ON DELETE CASCADE will automatically clean up:
        // - job_embeddings
        // - centroid_assignments
        // - job_locations
        await using (var deleteCmd = new NpgsqlCommand(@"
            DELETE FROM jobs
            WHERE date_posted < @cutoffDate", conn))
        {
            deleteCmd.Parameters.AddWithValue("cutoffDate", cutoffDate);
            deleteCmd.CommandTimeout = 600; // 10 minutes timeout for large deletes

            var deletedCount = await deleteCmd.ExecuteNonQueryAsync();
            context.Logger.LogInformation($"Successfully deleted {deletedCount} job(s) and their related data");
        }

        context.Logger.LogInformation("=== Job Cleanup Complete ===");
    }
}

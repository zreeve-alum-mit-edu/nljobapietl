using Amazon.Lambda.Core;
using JobApi.Lambda.Api.Models;
using Npgsql;

namespace JobApi.Lambda.Api.Helpers;

/// <summary>
/// Helper class for logging API audit information to the database
/// </summary>
public class AuditLogger
{
    private readonly string _connectionString;

    public AuditLogger(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Logs a search request audit entry to the database
    /// </summary>
    public async Task LogSearchAudit(
        string endpoint,
        DateTime startTime,
        DateTime endTime,
        int embeddingDurationMs,
        int databaseDurationMs,
        SearchRequest request,
        int numResultsReturned,
        int numJobsFiltered,
        int statusCode,
        string? errorMessage,
        string lambdaRequestId,
        ILambdaContext context)
    {
        try
        {
            var totalDurationMs = (int)(endTime - startTime).TotalMilliseconds;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO api_audit_logs (
                    endpoint,
                    start_time,
                    end_time,
                    total_duration_ms,
                    embedding_duration_ms,
                    database_duration_ms,
                    prompt,
                    num_jobs_requested,
                    city,
                    state,
                    miles,
                    include_onsite,
                    include_hybrid,
                    days_since_posting,
                    num_results_returned,
                    num_jobs_filtered,
                    status_code,
                    error_message,
                    lambda_request_id
                ) VALUES (
                    @endpoint,
                    @startTime,
                    @endTime,
                    @totalDurationMs,
                    @embeddingDurationMs,
                    @databaseDurationMs,
                    @prompt,
                    @numJobsRequested,
                    @city,
                    @state,
                    @miles,
                    @includeOnsite,
                    @includeHybrid,
                    @daysSincePosting,
                    @numResultsReturned,
                    @numJobsFiltered,
                    @statusCode,
                    @errorMessage,
                    @lambdaRequestId
                )";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("endpoint", endpoint);
            cmd.Parameters.AddWithValue("startTime", startTime);
            cmd.Parameters.AddWithValue("endTime", endTime);
            cmd.Parameters.AddWithValue("totalDurationMs", totalDurationMs);
            cmd.Parameters.AddWithValue("embeddingDurationMs", embeddingDurationMs);
            cmd.Parameters.AddWithValue("databaseDurationMs", databaseDurationMs);
            cmd.Parameters.AddWithValue("prompt", request.Prompt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("numJobsRequested", request.NumJobs);
            cmd.Parameters.AddWithValue("city", request.City ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("state", request.State ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("miles", request.Miles);
            cmd.Parameters.AddWithValue("includeOnsite", request.IncludeOnsite);
            cmd.Parameters.AddWithValue("includeHybrid", request.IncludeHybrid);
            cmd.Parameters.AddWithValue("daysSincePosting", request.DaysSincePosting.HasValue ? request.DaysSincePosting.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("numResultsReturned", numResultsReturned);
            cmd.Parameters.AddWithValue("numJobsFiltered", numJobsFiltered);
            cmd.Parameters.AddWithValue("statusCode", statusCode);
            cmd.Parameters.AddWithValue("errorMessage", errorMessage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("lambdaRequestId", lambdaRequestId);

            await cmd.ExecuteNonQueryAsync();

            context.Logger.LogInformation($"Audit log created: endpoint={endpoint}, status={statusCode}, duration={totalDurationMs}ms, results={numResultsReturned}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to write audit log: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a remote search request audit entry to the database
    /// </summary>
    public async Task LogRemoteSearchAudit(
        string endpoint,
        DateTime startTime,
        DateTime endTime,
        int embeddingDurationMs,
        int databaseDurationMs,
        RemoteSearchRequest request,
        int numResultsReturned,
        int numJobsFiltered,
        int statusCode,
        string? errorMessage,
        string lambdaRequestId,
        ILambdaContext context)
    {
        try
        {
            var totalDurationMs = (int)(endTime - startTime).TotalMilliseconds;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO api_audit_logs (
                    endpoint,
                    start_time,
                    end_time,
                    total_duration_ms,
                    embedding_duration_ms,
                    database_duration_ms,
                    prompt,
                    num_jobs_requested,
                    days_since_posting,
                    num_results_returned,
                    num_jobs_filtered,
                    status_code,
                    error_message,
                    lambda_request_id
                ) VALUES (
                    @endpoint,
                    @startTime,
                    @endTime,
                    @totalDurationMs,
                    @embeddingDurationMs,
                    @databaseDurationMs,
                    @prompt,
                    @numJobsRequested,
                    @daysSincePosting,
                    @numResultsReturned,
                    @numJobsFiltered,
                    @statusCode,
                    @errorMessage,
                    @lambdaRequestId
                )";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("endpoint", endpoint);
            cmd.Parameters.AddWithValue("startTime", startTime);
            cmd.Parameters.AddWithValue("endTime", endTime);
            cmd.Parameters.AddWithValue("totalDurationMs", totalDurationMs);
            cmd.Parameters.AddWithValue("embeddingDurationMs", embeddingDurationMs);
            cmd.Parameters.AddWithValue("databaseDurationMs", databaseDurationMs);
            cmd.Parameters.AddWithValue("prompt", request.Prompt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("numJobsRequested", request.NumJobs);
            cmd.Parameters.AddWithValue("daysSincePosting", request.DaysSincePosting.HasValue ? request.DaysSincePosting.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("numResultsReturned", numResultsReturned);
            cmd.Parameters.AddWithValue("numJobsFiltered", numJobsFiltered);
            cmd.Parameters.AddWithValue("statusCode", statusCode);
            cmd.Parameters.AddWithValue("errorMessage", errorMessage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("lambdaRequestId", lambdaRequestId);

            await cmd.ExecuteNonQueryAsync();

            context.Logger.LogInformation($"Audit log created: endpoint={endpoint}, status={statusCode}, duration={totalDurationMs}ms, results={numResultsReturned}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to write audit log: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a validation error audit entry to the database
    /// </summary>
    public async Task LogValidationError(
        string endpoint,
        DateTime startTime,
        string? prompt,
        int? numJobs,
        string? city,
        string? state,
        int? miles,
        bool? includeOnsite,
        bool? includeHybrid,
        int? daysSincePosting,
        int statusCode,
        string errorMessage,
        string lambdaRequestId,
        ILambdaContext context)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var totalDurationMs = (int)(endTime - startTime).TotalMilliseconds;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO api_audit_logs (
                    endpoint,
                    start_time,
                    end_time,
                    total_duration_ms,
                    embedding_duration_ms,
                    database_duration_ms,
                    prompt,
                    num_jobs_requested,
                    city,
                    state,
                    miles,
                    include_onsite,
                    include_hybrid,
                    days_since_posting,
                    num_results_returned,
                    num_jobs_filtered,
                    status_code,
                    error_message,
                    lambda_request_id
                ) VALUES (
                    @endpoint,
                    @startTime,
                    @endTime,
                    @totalDurationMs,
                    0,
                    0,
                    @prompt,
                    @numJobs,
                    @city,
                    @state,
                    @miles,
                    @includeOnsite,
                    @includeHybrid,
                    @daysSincePosting,
                    0,
                    0,
                    @statusCode,
                    @errorMessage,
                    @lambdaRequestId
                )";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("endpoint", endpoint);
            cmd.Parameters.AddWithValue("startTime", startTime);
            cmd.Parameters.AddWithValue("endTime", endTime);
            cmd.Parameters.AddWithValue("totalDurationMs", totalDurationMs);
            cmd.Parameters.AddWithValue("prompt", prompt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("numJobs", numJobs.HasValue ? numJobs.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("city", city ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("state", state ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("miles", miles.HasValue ? miles.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("includeOnsite", includeOnsite.HasValue ? includeOnsite.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("includeHybrid", includeHybrid.HasValue ? includeHybrid.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("daysSincePosting", daysSincePosting.HasValue ? daysSincePosting.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("statusCode", statusCode);
            cmd.Parameters.AddWithValue("errorMessage", errorMessage);
            cmd.Parameters.AddWithValue("lambdaRequestId", lambdaRequestId);

            await cmd.ExecuteNonQueryAsync();

            context.Logger.LogInformation($"Validation error audit log created: endpoint={endpoint}, status={statusCode}, error={errorMessage}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to write validation error audit log: {ex.Message}");
        }
    }
}

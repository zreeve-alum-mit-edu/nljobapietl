using Amazon.Lambda.Core;
using JobApi.Lambda.Api.Helpers;
using JobApi.Lambda.Api.Models;
using Npgsql;
using System.Diagnostics;

namespace JobApi.Lambda.Api.Handlers;

public class RemoteSearchHandler
{
    private readonly string _connectionString;
    private readonly string _openAiApiKey;
    private readonly int _hnswEfSearch;
    private readonly AuditLogger _auditLogger;

    public RemoteSearchHandler()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST not set");
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME not set");
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER not set");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD not set");

        _connectionString = $"Host={host};Database={database};Username={username};Password={password}";
        _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY not set");
        _hnswEfSearch = int.Parse(Environment.GetEnvironmentVariable("HNSW_EF_SEARCH") ?? "200");
        _auditLogger = new AuditLogger(_connectionString);
    }

    /// <summary>
    /// Searches for remote jobs using natural language and date filter
    /// </summary>
    public async Task<SearchResponse> SearchRemote(RemoteSearchRequest request, ILambdaContext context)
    {
        var startTime = DateTime.UtcNow;
        var embeddingDurationMs = 0;
        var databaseDurationMs = 0;
        var numJobsFiltered = 0;
        var statusCode = 200;
        string? errorMessage = null;

        context.Logger.LogInformation($"Starting remote search: prompt='{request.Prompt}', numJobs={request.NumJobs}, daysSince={request.DaysSincePosting}");

        try
        {
            // Step 1: Create embedding from prompt using OpenAI API
            float[] embedding;
            var embeddingStopwatch = Stopwatch.StartNew();
            try
            {
                embedding = await CreateEmbedding(request.Prompt, context);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Failed to create embedding: {ex.Message}");
                errorMessage = $"Unable to create embedding from OpenAI API: {ex.Message}";
                statusCode = 500;
                throw new OpenAIServiceException("Unable to create embedding from OpenAI API. This is a temporary service issue. Please try again in a few moments.");
            }
            finally
            {
                embeddingStopwatch.Stop();
                embeddingDurationMs = (int)embeddingStopwatch.ElapsedMilliseconds;
            }

            // Step 2: Connect to database
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Step 3: **CRITICAL**: SET hnsw.ef_search for vector similarity performance
            await SetHnswEfSearch(connection, context);

            // Step 4: Execute vector similarity search query for remote jobs
            var databaseStopwatch = Stopwatch.StartNew();
            var (jobs, filteredCount) = await SearchRemoteJobs(connection, embedding, request.NumJobs, request.DaysSincePosting, context);
            databaseStopwatch.Stop();
            databaseDurationMs = (int)databaseStopwatch.ElapsedMilliseconds;
            numJobsFiltered = filteredCount;

            context.Logger.LogInformation($"Remote search completed: found {jobs.Count} jobs");

            var response = new SearchResponse
            {
                Jobs = jobs,
                TotalCount = jobs.Count
            };

            // Log audit entry
            await _auditLogger.LogRemoteSearchAudit(
                endpoint: "/search/remote",
                startTime: startTime,
                endTime: DateTime.UtcNow,
                embeddingDurationMs: embeddingDurationMs,
                databaseDurationMs: databaseDurationMs,
                request: request,
                numResultsReturned: jobs.Count,
                numJobsFiltered: numJobsFiltered,
                statusCode: statusCode,
                errorMessage: errorMessage,
                lambdaRequestId: context.AwsRequestId,
                context: context
            );

            return response;
        }
        catch (Exception ex)
        {
            // Log failed request
            if (errorMessage == null)
            {
                errorMessage = ex.Message;
                statusCode = 500;
            }

            await _auditLogger.LogRemoteSearchAudit(
                endpoint: "/search/remote",
                startTime: startTime,
                endTime: DateTime.UtcNow,
                embeddingDurationMs: embeddingDurationMs,
                databaseDurationMs: databaseDurationMs,
                request: request,
                numResultsReturned: 0,
                numJobsFiltered: numJobsFiltered,
                statusCode: statusCode,
                errorMessage: errorMessage,
                lambdaRequestId: context.AwsRequestId,
                context: context
            );

            throw;
        }
    }

    /// <summary>
    /// Creates an embedding vector from text using OpenAI API with retry logic
    /// Retries up to 2 times with 1 second delay between attempts
    /// </summary>
    private async Task<float[]> CreateEmbedding(string text, ILambdaContext context)
    {
        context.Logger.LogInformation($"Creating embedding for prompt: {text.Substring(0, Math.Min(50, text.Length))}...");

        const int maxAttempts = 3; // Initial attempt + 2 retries
        const int retryDelayMs = 1000; // 1 second

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new
                {
                    model = "text-embedding-3-small",
                    input = text,
                    encoding_format = "float"
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                context.Logger.LogInformation($"Calling OpenAI API (attempt {attempt}/{maxAttempts})...");

                var response = await client.PostAsync("https://api.openai.com/v1/embeddings", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseBody);

                    var embedding = result?.Data?.FirstOrDefault()?.Embedding;

                    if (embedding == null || embedding.Length != 1536)
                    {
                        context.Logger.LogError($"Invalid embedding response: expected 1536 dimensions, got {embedding?.Length ?? 0}");
                        throw new Exception("Invalid embedding response from OpenAI");
                    }

                    context.Logger.LogInformation($"Successfully created embedding ({embedding.Length} dimensions)");
                    return embedding;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    context.Logger.LogWarning($"OpenAI API returned {response.StatusCode}: {errorBody}");

                    if (attempt < maxAttempts)
                    {
                        context.Logger.LogInformation($"Retrying in {retryDelayMs}ms...");
                        await Task.Delay(retryDelayMs);
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error calling OpenAI API (attempt {attempt}/{maxAttempts}): {ex.Message}");

                if (attempt < maxAttempts)
                {
                    context.Logger.LogInformation($"Retrying in {retryDelayMs}ms...");
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        context.Logger.LogError($"Failed to create embedding after {maxAttempts} attempts");
        throw new Exception("Failed to create embedding from OpenAI API after multiple retries");
    }

    /// <summary>
    /// Sets the HNSW ef_search parameter for optimal vector search performance
    /// </summary>
    private async Task SetHnswEfSearch(NpgsqlConnection connection, ILambdaContext context)
    {
        context.Logger.LogInformation($"Setting hnsw.ef_search = {_hnswEfSearch}");

        await using var cmd = new NpgsqlCommand($"SET hnsw.ef_search = {_hnswEfSearch}", connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Executes the vector similarity search query for remote jobs only
    /// Returns both the job results and the count of jobs that passed the filter
    /// </summary>
    private async Task<(List<JobResult> jobs, int filteredCount)> SearchRemoteJobs(
        NpgsqlConnection connection,
        float[] embedding,
        int limit,
        int? daysSincePosting,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Executing remote vector search query with limit={limit}, daysSince={daysSincePosting}");

        // Build date filter if specified
        var dateFilter = daysSincePosting.HasValue
            ? $"AND date_posted >= NOW() - INTERVAL '{daysSincePosting.Value} days'"
            : "";

        // Build complete SQL query - filter on job_embeddings.generated_workplace for index usage
        // Include filtered count using window function
        var sql = $@"
            WITH top_jobs AS (
                SELECT
                    je.job_id,
                    (je.embedding <=> @embedding::vector) as similarity_score
                FROM job_embeddings je
                INNER JOIN jobs j ON je.job_id = j.id
                WHERE je.generated_workplace = 'REMOTE'
                    AND je.embedding IS NOT NULL
                    AND j.status = 'embedded'
                    AND j.is_valid = true
                    {dateFilter}
                ORDER BY similarity_score ASC
                LIMIT {limit}
            ),
            total_count AS (
                SELECT COUNT(*) as total
                FROM job_embeddings je
                INNER JOIN jobs j ON je.job_id = j.id
                WHERE je.generated_workplace = 'REMOTE'
                    AND je.embedding IS NOT NULL
                    AND j.status = 'embedded'
                    AND j.is_valid = true
                    {dateFilter}
            )
            SELECT
                j.id,
                j.job_title,
                j.company_name,
                j.job_description,
                j.generated_workplace,
                j.generated_workplace_confidence,
                j.date_posted,
                t.similarity_score,
                (SELECT total FROM total_count) as filtered_count,
                COALESCE(
                    json_agg(
                        json_build_object(
                            'city', jl.generated_city,
                            'state', jl.generated_state,
                            'country', jl.generated_country,
                            'urls', (
                                SELECT COALESCE(json_agg(jlu.url), '[]'::json)
                                FROM job_location_urls jlu
                                WHERE jlu.job_location_id = jl.id
                            )
                        )
                    ) FILTER (WHERE jl.id IS NOT NULL),
                    '[]'::json
                ) as locations
            FROM top_jobs t
            INNER JOIN jobs j ON j.id = t.job_id
            LEFT JOIN job_locations jl ON jl.job_id = j.id
            GROUP BY j.id, j.job_title, j.company_name, j.job_description, j.generated_workplace, j.generated_workplace_confidence, j.date_posted, t.similarity_score
            ORDER BY t.similarity_score ASC";

        context.Logger.LogInformation($"Executing SQL query with filtered count");

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("embedding", embedding);

        var jobs = new List<JobResult>();
        var filteredCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            // Parse locations JSON array
            var locationsJson = reader.GetString(9);
            var locations = System.Text.Json.JsonSerializer.Deserialize<List<JobLocation>>(locationsJson) ?? new List<JobLocation>();

            var job = new JobResult
            {
                Id = reader.GetGuid(0),
                Title = reader.GetString(1),
                Company = reader.GetString(2),
                Description = reader.GetString(3),
                Workplace = reader.GetString(4),
                WorkplaceConfidence = reader.IsDBNull(5) ? null : reader.GetString(5),
                DatePosted = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                Locations = locations
            };

            // Get filtered count from first row
            if (jobs.Count == 0)
            {
                filteredCount = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetInt64(8));
            }

            jobs.Add(job);
        }

        context.Logger.LogInformation($"Query returned {jobs.Count} remote jobs from {filteredCount} filtered jobs");

        return (jobs, filteredCount);
    }
}

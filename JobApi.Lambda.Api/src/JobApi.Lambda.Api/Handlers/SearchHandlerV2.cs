using Amazon.Lambda.Core;
using JobApi.Lambda.Api.Helpers;
using JobApi.Lambda.Api.Models;
using Npgsql;
using Pgvector;
using System.Diagnostics;

namespace JobApi.Lambda.Api.Handlers;

public class SearchHandlerV2
{
    private readonly string _connectionString;
    private readonly string _openAiApiKey;
    private readonly int _topNCentroids;
    private readonly AuditLogger _auditLogger;

    public SearchHandlerV2()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST not set");
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME not set");
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER not set");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD not set");

        _connectionString = $"Host={host};Database={database};Username={username};Password={password}";
        _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY not set");
        _topNCentroids = int.Parse(Environment.GetEnvironmentVariable("TOP_N_CENTROIDS") ?? "50");
        _auditLogger = new AuditLogger(_connectionString);
    }

    /// <summary>
    /// Searches for jobs using natural language and location filter (V2)
    /// </summary>
    public async Task<SearchResponse> Search(SearchRequest request, ILambdaContext context)
    {
        var startTime = DateTime.UtcNow;
        var embeddingDurationMs = 0;
        var databaseDurationMs = 0;
        var numJobsFiltered = 0;
        var statusCode = 200;
        string? errorMessage = null;

        context.Logger.LogInformation($"[V2] Starting search: prompt='{request.Prompt}', numJobs={request.NumJobs}, location={request.City},{request.State}, miles={request.Miles}, onsite={request.IncludeOnsite}, hybrid={request.IncludeHybrid}, daysSince={request.DaysSincePosting}");

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

            // Step 3: Get top N centroids closest to query embedding
            var topCentroidIds = await GetTopCentroidIds(connection, embedding, _topNCentroids, context);

            // Step 4: Get coordinates for location
            var (lat, lon) = await GetCoordinates(connection, request.City, request.State, context);

            // Step 5: Execute vector similarity search query with exact KNN and centroid filtering
            var databaseStopwatch = Stopwatch.StartNew();
            var (jobs, filteredCount) = await SearchJobs(connection, embedding, request, lat, lon, topCentroidIds, context);
            databaseStopwatch.Stop();
            databaseDurationMs = (int)databaseStopwatch.ElapsedMilliseconds;
            numJobsFiltered = filteredCount;

            context.Logger.LogInformation($"[V2] Search completed: found {jobs.Count} jobs");

            var response = new SearchResponse
            {
                Jobs = jobs,
                TotalCount = jobs.Count
            };

            // Log audit entry
            await _auditLogger.LogSearchAudit(
                endpoint: "/search",
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

            await _auditLogger.LogSearchAudit(
                endpoint: "/search",
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
    /// Gets the top N centroid IDs closest to the query embedding using cosine distance
    /// </summary>
    private async Task<List<Guid>> GetTopCentroidIds(
        NpgsqlConnection connection,
        float[] queryEmbedding,
        int topN,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Finding top {topN} centroids closest to query embedding");

        var sql = @"
            SELECT id
            FROM centroids
            ORDER BY centroid <=> @queryEmbedding::vector
            LIMIT @topN";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("queryEmbedding", queryEmbedding);
        cmd.Parameters.AddWithValue("topN", topN);

        var centroidIds = new List<Guid>();

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            centroidIds.Add(reader.GetGuid(0));
        }

        context.Logger.LogInformation($"Found {centroidIds.Count} top centroids");

        return centroidIds;
    }

    /// <summary>
    /// Executes the vector similarity search query using exact KNN with MATERIALIZED CTE
    /// Returns both the job results and the count of jobs that passed the filter
    /// </summary>
    private async Task<(List<JobResult> jobs, int filteredCount)> SearchJobs(
        NpgsqlConnection connection,
        float[] embedding,
        SearchRequest request,
        double lat,
        double lon,
        List<Guid> centroidIds,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Executing exact KNN search with MATERIALIZED CTE");

        // Build workplace IN clause based on includeOnsite and includeHybrid
        var workplaceTypes = new List<string>();
        if (request.IncludeOnsite) workplaceTypes.Add("'ONSITE'");
        if (request.IncludeHybrid) workplaceTypes.Add("'HYBRID'");
        var workplaceInClause = string.Join(",", workplaceTypes);

        // Convert miles to meters
        var distanceMeters = request.Miles * 1609.34;

        // Build date filter if specified
        var dateFilter = request.DaysSincePosting.HasValue
            ? $"AND j.date_posted >= NOW() - INTERVAL '{request.DaysSincePosting.Value} days'"
            : "";

        // Build query using MATERIALIZED CTE to force filter-first execution
        var sql = $@"
            SET LOCAL jit = off;

            WITH base AS MATERIALIZED (
                SELECT DISTINCT j.id
                FROM jobs j
                INNER JOIN job_locations jl ON jl.job_id = j.id
                INNER JOIN centroid_assignments ca ON ca.job_id = j.id
                WHERE ca.centroid_id = ANY(@centroidIds)
                    AND j.is_valid = true
                    AND j.status = 'embedded'
                    AND j.generated_workplace IN ({workplaceInClause})
                    AND jl.gistlocation IS NOT NULL
                    AND ST_DWithin(
                        jl.gistlocation,
                        ST_SetSRID(ST_MakePoint(@lon, @lat), 4326)::geography,
                        @distance
                    )
                    {dateFilter}
            ),
            base_count AS (
                SELECT COUNT(*) as total FROM base
            )
            SELECT
                j.id,
                j.job_title,
                j.company_name,
                j.job_description,
                j.generated_workplace,
                j.generated_workplace_confidence,
                first_loc.generated_city,
                first_loc.generated_state,
                first_url.url as job_url,
                j.date_posted,
                (e.embedding <=> @embedding::vector) AS similarity_score,
                (SELECT total FROM base_count) AS filtered_count
            FROM base b
            JOIN job_embeddings e ON e.job_id = b.id
            JOIN jobs j ON j.id = b.id
            LEFT JOIN LATERAL (
                SELECT generated_city, generated_state, id
                FROM job_locations
                WHERE job_id = j.id
                ORDER BY id
                LIMIT 1
            ) first_loc ON true
            LEFT JOIN LATERAL (
                SELECT url
                FROM job_location_urls
                WHERE job_location_id = first_loc.id
                ORDER BY id
                LIMIT 1
            ) first_url ON true
            WHERE e.embedding IS NOT NULL
            ORDER BY similarity_score, j.id
            LIMIT @limit";

        context.Logger.LogInformation($"Executing SQL query with params: embedding(1536d), lon={lon}, lat={lat}, distance={distanceMeters}m, limit={request.NumJobs}, centroids={centroidIds.Count}");

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("centroidIds", centroidIds.ToArray());
        cmd.Parameters.AddWithValue("embedding", embedding);
        cmd.Parameters.AddWithValue("lon", lon);
        cmd.Parameters.AddWithValue("lat", lat);
        cmd.Parameters.AddWithValue("distance", distanceMeters);
        cmd.Parameters.AddWithValue("limit", request.NumJobs);

        var jobs = new List<JobResult>();
        var filteredCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            // Handle nullable location fields
            var city = reader.IsDBNull(6) ? "" : reader.GetString(6);
            var state = reader.IsDBNull(7) ? "" : reader.GetString(7);
            var location = string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state)
                ? ""
                : $"{city},{state}";

            var job = new JobResult
            {
                Id = reader.GetGuid(0),
                Title = reader.GetString(1),
                Company = reader.GetString(2),
                Description = reader.GetString(3),
                Workplace = reader.GetString(4),
                WorkplaceConfidence = reader.IsDBNull(5) ? null : reader.GetString(5),
                Location = location,
                Url = reader.IsDBNull(8) ? null : reader.GetString(8),
                DatePosted = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            };

            // Get filtered count from first row
            if (jobs.Count == 0)
            {
                filteredCount = reader.IsDBNull(11) ? 0 : Convert.ToInt32(reader.GetInt64(11));
            }

            jobs.Add(job);
        }

        context.Logger.LogInformation($"Query returned {jobs.Count} jobs from {filteredCount} filtered jobs");

        return (jobs, filteredCount);
    }

    /// <summary>
    /// Gets coordinates for a city and state from geolocations table
    /// </summary>
    private async Task<(double lat, double lon)> GetCoordinates(
        NpgsqlConnection connection,
        string city,
        string state,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Looking up coordinates for: {city}, {state}");

        var sql = @"
            SELECT latitude, longitude
            FROM geolocations
            WHERE LOWER(city) = LOWER(@city)
            AND LOWER(state) = LOWER(@state)
            LIMIT 1";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("city", city);
        cmd.Parameters.AddWithValue("state", state);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var lat = reader.GetDouble(0);
            var lon = reader.GetDouble(1);
            context.Logger.LogInformation($"Found coordinates for {city}, {state}: lat={lat}, lon={lon}");
            return (lat, lon);
        }

        // This should never happen because we validate locations in ApiGatewayHandler
        throw new Exception($"Location '{city}, {state}' not found in geolocations table");
    }
}

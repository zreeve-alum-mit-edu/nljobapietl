using Amazon.Lambda.Core;
using JobApi.Lambda.Api.Models;
using Npgsql;
using Pgvector;

namespace JobApi.Lambda.Api.Handlers;

public class SearchHandler
{
    private readonly string _connectionString;
    private readonly string _openAiApiKey;
    private readonly int _hnswEfSearch;

    public SearchHandler()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST not set");
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME not set");
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER not set");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD not set");

        _connectionString = $"Host={host};Database={database};Username={username};Password={password}";
        _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY not set");
        _hnswEfSearch = int.Parse(Environment.GetEnvironmentVariable("HNSW_EF_SEARCH") ?? "200");
    }

    /// <summary>
    /// Searches for jobs using natural language, remote flag, date filter, and location filters
    /// </summary>
    public async Task<SearchResponse> Search(SearchRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Starting search: prompt='{request.Prompt}', numJobs={request.NumJobs}, includeRemote={request.IncludeRemote}, daysSince={request.DaysSincePosting}, filters={request.Filters.Count}");

        // Step 1: Create embedding from prompt using OpenAI API
        float[] embedding;
        try
        {
            embedding = await CreateEmbedding(request.Prompt, context);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to create embedding: {ex.Message}");
            throw new OpenAIServiceException("Unable to create embedding from OpenAI API. This is a temporary service issue. Please try again in a few moments.");
        }

        // Step 2: Connect to database
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Step 3: **CRITICAL**: SET hnsw.ef_search for vector similarity performance
        await SetHnswEfSearch(connection, context);

        // Step 4: Build WHERE clause from filters (async because it looks up coordinates)
        var whereClause = await BuildWhereClause(connection, request, context);

        // Step 5: Execute vector similarity search query
        var jobs = await SearchJobs(connection, embedding, whereClause, request.NumJobs, request.DaysSincePosting, context);

        context.Logger.LogInformation($"Search completed: found {jobs.Count} jobs");

        return new SearchResponse
        {
            Jobs = jobs,
            TotalCount = jobs.Count
        };
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
    /// Builds the WHERE clause from search request filters
    /// </summary>
    private async Task<string> BuildWhereClause(NpgsqlConnection connection, SearchRequest request, ILambdaContext context)
    {
        var workplaceConditions = new List<string>();

        // Include remote jobs if requested
        if (request.IncludeRemote)
        {
            workplaceConditions.Add("generated_workplace = 'REMOTE'");
            context.Logger.LogInformation("Added REMOTE workplace condition");
        }

        // Add location-based filters
        foreach (var filter in request.Filters)
        {
            context.Logger.LogInformation($"Processing filter: location={filter.Location}, onsite={filter.IncludeOnsite}, hybrid={filter.IncludeHybrid}, miles={filter.Miles}");

            // Get coordinates for this location
            var (lat, lon) = await GetCoordinates(connection, filter.Location, context);

            // Build workplace type conditions for this filter
            var workplaceTypes = new List<string>();
            if (filter.IncludeOnsite)
            {
                workplaceTypes.Add("generated_workplace = 'ONSITE'");
            }
            if (filter.IncludeHybrid)
            {
                workplaceTypes.Add("generated_workplace = 'HYBRID'");
            }

            var workplaceTypeClause = string.Join(" OR ", workplaceTypes);

            // Build distance calculation using Haversine formula
            // 6371 = Earth's radius in km, 0.621371 = km to miles conversion
            // Must check for NULL coordinates first
            var distanceCalculation = $@"(
                latitude IS NOT NULL AND
                longitude IS NOT NULL AND
                (
                    6371 * acos(
                        cos(radians({lat})) * cos(radians(latitude)) *
                        cos(radians(longitude) - radians({lon})) +
                        sin(radians({lat})) * sin(radians(latitude))
                    )
                ) * 0.621371 <= {filter.Miles}
            )";

            // Combine workplace types AND distance for this location filter
            workplaceConditions.Add($"(({workplaceTypeClause}) AND {distanceCalculation})");
        }

        // Combine all workplace conditions with OR
        var workplaceWhereClause = string.Join(" OR ", workplaceConditions);

        context.Logger.LogInformation($"Built workplace WHERE clause with {workplaceConditions.Count} conditions");

        return workplaceWhereClause;
    }

    /// <summary>
    /// Executes the vector similarity search query
    /// </summary>
    private async Task<List<JobResult>> SearchJobs(
        NpgsqlConnection connection,
        float[] embedding,
        string whereClause,
        int limit,
        int? daysSincePosting,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Executing vector search query with limit={limit}, daysSince={daysSincePosting}");

        // Build date filter if specified
        var dateFilter = daysSincePosting.HasValue
            ? $"AND date_posted >= NOW() - INTERVAL '{daysSincePosting.Value} days'"
            : "";

        // Build complete SQL query - JOIN with job_embeddings table
        var sql = $@"
            SELECT
                j.id,
                j.job_title,
                j.company_name,
                j.job_description,
                j.generated_workplace,
                j.generated_workplace_confidence,
                j.generated_city,
                j.generated_state,
                j.job_url,
                j.date_posted,
                (je.embedding <=> @embedding::vector) as similarity_score
            FROM jobs j
            INNER JOIN job_embeddings je ON j.id = je.job_id
            WHERE j.status = 'embedded'
                AND j.is_valid = true
                {dateFilter}
                AND ({whereClause})
            ORDER BY similarity_score ASC
            LIMIT {limit}";

        context.Logger.LogInformation($"Executing SQL query: {sql}");

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("embedding", embedding);

        var jobs = new List<JobResult>();

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
                Url = reader.GetString(8),
                DatePosted = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            };

            jobs.Add(job);
        }

        context.Logger.LogInformation($"Query returned {jobs.Count} jobs");

        return jobs;
    }

    /// <summary>
    /// Parses location string (City,State) and gets coordinates from geolocations table
    /// </summary>
    private async Task<(double lat, double lon)> GetCoordinates(
        NpgsqlConnection connection,
        string location,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Looking up coordinates for: {location}");

        // Parse "City,State"
        var parts = location.Split(',');
        var city = parts[0].Trim();
        var state = parts[1].Trim();

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
            context.Logger.LogInformation($"Found coordinates for {location}: lat={lat}, lon={lon}");
            return (lat, lon);
        }

        // This should never happen because we validate locations in ApiGatewayHandler
        throw new Exception($"Location '{location}' not found in geolocations table");
    }
}

/// <summary>
/// Custom exception for OpenAI service failures
/// </summary>
public class OpenAIServiceException : Exception
{
    public OpenAIServiceException(string message) : base(message) { }
}

/// <summary>
/// Response from OpenAI embeddings API
/// </summary>
public class OpenAIEmbeddingResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("object")]
    public string? Object { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public List<OpenAIEmbeddingData>? Data { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("model")]
    public string? Model { get; set; }
}

public class OpenAIEmbeddingData
{
    [System.Text.Json.Serialization.JsonPropertyName("object")]
    public string? Object { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("index")]
    public int Index { get; set; }
}

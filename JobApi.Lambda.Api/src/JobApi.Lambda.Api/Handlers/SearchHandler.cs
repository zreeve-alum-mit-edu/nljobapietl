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
        _hnswEfSearch = int.Parse(Environment.GetEnvironmentVariable("HNSW_EF_SEARCH") ?? "500");
    }

    /// <summary>
    /// Searches for jobs using natural language and filters
    /// </summary>
    public async Task<SearchResponse> Search(SearchRequest request, ILambdaContext context)
    {
        // TODO: Implement search logic
        // 1. Create embedding from prompt using OpenAI API
        // 2. Connect to database
        // 3. **CRITICAL**: SET hnsw.ef_search = {_hnswEfSearch}
        // 4. Build WHERE clause from filters (OR logic)
        // 5. Execute vector similarity query
        // 6. Map results to JobResult objects
        // 7. Return SearchResponse

        throw new NotImplementedException("Search logic not yet implemented");
    }

    /// <summary>
    /// Creates an embedding vector from text using OpenAI API
    /// </summary>
    private async Task<float[]> CreateEmbedding(string text)
    {
        // TODO: Call OpenAI embeddings API
        // Model: text-embedding-3-small
        // Should return 1536-dimensional vector

        throw new NotImplementedException("Embedding creation not yet implemented");
    }

    /// <summary>
    /// Executes the vector similarity search query
    /// </summary>
    private async Task<List<JobResult>> SearchJobs(
        NpgsqlConnection connection,
        float[] embedding,
        List<SearchFilter> filters,
        int limit,
        ILambdaContext context)
    {
        // TODO: Build and execute SQL query
        // 1. Build WHERE clause from filters
        // 2. For remote: generated_workplace = 'REMOTE'
        // 3. For onsite/hybrid: generated_workplace = 'ONSITE'/'HYBRID'
        //    AND calculate_distance(lat, lon, target_lat, target_lon) <= miles
        // 4. Order by embedding <=> vector
        // 5. LIMIT {limit}

        throw new NotImplementedException("Job search query not yet implemented");
    }

    /// <summary>
    /// Parses location string (City,State) and gets coordinates
    /// </summary>
    private async Task<(double lat, double lon)> GetCoordinates(
        NpgsqlConnection connection,
        string location)
    {
        // TODO: Parse "City,State" and look up in geolocations table

        throw new NotImplementedException("Coordinate lookup not yet implemented");
    }
}

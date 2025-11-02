using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using JobApi.Common;
using JobApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace JobApi.ETL;

public class JobSearcher
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run search <query> [num-results] [ef-search]");
            Console.WriteLine("Example: dotnet run search \"senior software engineer\" 50 200");
            return 1;
        }

        var query = args[0];
        var numResults = args.Length > 1 ? int.Parse(args[1]) : 20;
        var efSearch = args.Length > 2 ? int.Parse(args[2]) : 100;

        Console.WriteLine("Loading environment variables...");
        Env.Load();

        Console.WriteLine($"Searching for: {query}");
        Console.WriteLine($"Num results: {numResults}");
        Console.WriteLine($"EF search: {efSearch}");
        Console.WriteLine();

        var results = await Search(query, numResults, efSearch);

        Console.WriteLine($"\n✅ Found {results.Count} results\n");
        Console.WriteLine("Results:");
        Console.WriteLine("========================================");

        foreach (var job in results)
        {
            Console.WriteLine($"\n#{results.IndexOf(job) + 1} - Distance: {job.Distance:F4}");
            Console.WriteLine($"Title: {job.JobTitle}");
            Console.WriteLine($"Company: {job.CompanyName}");
            Console.WriteLine($"Location: {job.Location}, {job.Country}");
            Console.WriteLine($"Type: {job.EmploymentType}");
            Console.WriteLine($"Posted: {job.DatePosted:yyyy-MM-dd}");
            Console.WriteLine($"URL: {job.JobUrl}");
            if (!string.IsNullOrEmpty(job.JobDescription))
            {
                var desc = job.JobDescription.Length > 200
                    ? job.JobDescription.Substring(0, 200) + "..."
                    : job.JobDescription;
                Console.WriteLine($"Description: {desc}");
            }
        }

        return 0;
    }

    public static async Task<List<JobResult>> Search(string query, int numResults = 20, int efSearch = 100)
    {
        // Generate embedding for the query
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("OPENAI_API_KEY not found in environment");
        }

        Console.WriteLine("Generating query embedding...");
        var embedding = await GenerateQueryEmbedding(query, apiKey);

        if (embedding == null)
        {
            throw new Exception("Failed to generate embedding");
        }

        Console.WriteLine($"Embedding generated: {embedding.Length} dimensions");

        // Search database
        Console.WriteLine($"Searching database with ef_search={efSearch}...");
        var results = await SearchJobs(embedding, numResults, efSearch);

        return results;
    }

    private static async Task<float[]?> GenerateQueryEmbedding(string query, string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new
        {
            model = "text-embedding-3-small",
            input = query,
            input_type = "query"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await client.PostAsync("https://api.openai.com/v1/embeddings", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseBody);

            return result?.Data?.FirstOrDefault()?.Embedding;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling OpenAI API: {ex.Message}");
            return null;
        }
    }

    private static async Task<List<JobResult>> SearchJobs(float[] queryEmbedding, int numResults, int efSearch)
    {
        var connectionString = JobContext.GetConnectionString();
        var results = new List<JobResult>();

        // Create data source with pgvector support
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        // Set ef_search parameter for this connection
        await using (var setCmd = new NpgsqlCommand($"SET hnsw.ef_search = {efSearch}", connection))
        {
            await setCmd.ExecuteNonQueryAsync();
        }

        // Perform vector similarity search using raw SQL
        // The <=> operator calculates cosine distance in pgvector
        var sql = @"
            SELECT id, job_title, company_name, location, country, employment_type,
                   date_posted, job_description, job_url, (embedding <=> $1) as distance
            FROM jobs
            WHERE embedding IS NOT NULL
            ORDER BY embedding <=> $1
            LIMIT $2";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
        cmd.Parameters.AddWithValue(numResults);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new JobResult
            {
                Id = reader.GetGuid(0),
                JobTitle = reader.IsDBNull(1) ? null : reader.GetString(1),
                CompanyName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                Country = reader.IsDBNull(4) ? null : reader.GetString(4),
                EmploymentType = reader.IsDBNull(5) ? null : reader.GetString(5),
                DatePosted = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                JobDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                JobUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
                Distance = reader.GetDouble(9)
            });
        }

        return results;
    }
}

public class JobResult
{
    public Guid Id { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public string? EmploymentType { get; set; }
    public DateTime? DatePosted { get; set; }
    public string? JobDescription { get; set; }
    public string? JobUrl { get; set; }
    public double Distance { get; set; }
}

public class OpenAIEmbeddingResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("data")]
    public List<OpenAIEmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

public class OpenAIEmbeddingData
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

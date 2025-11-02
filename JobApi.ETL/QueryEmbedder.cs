using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;

namespace JobApi.ETL;

public class QueryEmbedder
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run embed-query <query-text> <output-file>");
            return 1;
        }

        var query = args[0];
        var outputFile = args[1];

        Console.WriteLine("Loading environment variables...");
        Env.Load();

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: OPENAI_API_KEY not found in environment");
            return 1;
        }

        Console.WriteLine($"Generating embedding for query ({query.Length} characters)...");
        var embedding = await GenerateEmbedding(query, apiKey);

        if (embedding == null)
        {
            Console.WriteLine("Error: Failed to generate embedding");
            return 1;
        }

        Console.WriteLine($"Embedding generated: {embedding.Length} dimensions");
        Console.WriteLine($"Saving to: {outputFile}");

        var result = new
        {
            query = query,
            embedding = embedding,
            model = "text-embedding-3-small",
            dimensions = embedding.Length,
            generated_at = DateTime.UtcNow
        };

        await File.WriteAllTextAsync(outputFile, JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        Console.WriteLine($"✅ Query embedding saved to {outputFile}");
        return 0;
    }

    private static async Task<float[]?> GenerateEmbedding(string text, string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new
        {
            model = "text-embedding-3-small",
            input = text,
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
            var result = JsonSerializer.Deserialize<QueryEmbeddingResponse>(responseBody);

            return result?.Data?.FirstOrDefault()?.Embedding;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling OpenAI API: {ex.Message}");
            return null;
        }
    }
}

public class QueryEmbeddingResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("data")]
    public List<QueryEmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public QueryUsageInfo? Usage { get; set; }
}

public class QueryEmbeddingData
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class QueryUsageInfo
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

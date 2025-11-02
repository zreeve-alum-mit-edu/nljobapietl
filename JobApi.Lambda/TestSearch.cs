using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using JobApi.Lambda.Handlers;
using System.Text.Json;

namespace JobApi.Lambda;

public class TestSearch
{
    public static async Task Main(string[] args)
    {
        // Load environment variables
        var envPath = Path.Combine("..", ".env");
        if (System.IO.File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
        }

        // Create test request
        var request = new SearchRequest
        {
            Prompt = "software engineer",
            Limit = 10,
            Filters = new List<SearchFilter>
            {
                new SearchFilter
                {
                    WorkplaceType = "onsite",
                    Location = "New York City,NY",
                    Miles = 25
                }
            }
        };

        Console.WriteLine("=== Search Request ===");
        Console.WriteLine(JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine();

        // Create handler and execute
        var handler = new SearchHandler();
        var context = new TestLambdaContext();

        try
        {
            var response = await handler.Search(request, context);

            Console.WriteLine("=== Search Response ===");
            Console.WriteLine($"Total Results: {response.TotalCount}");
            Console.WriteLine();

            foreach (var job in response.Jobs)
            {
                Console.WriteLine($"Job: {job.JobTitle}");
                Console.WriteLine($"  Company: {job.CompanyName}");
                Console.WriteLine($"  Workplace: {job.GeneratedWorkplace}");
                Console.WriteLine($"  Location: {job.GeneratedCity}, {job.GeneratedState}");
                Console.WriteLine($"  Similarity: {job.SimilarityScore:F4}");
                Console.WriteLine($"  URL: {job.JobUrl}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== ERROR ===");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}

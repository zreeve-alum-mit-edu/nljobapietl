using System.Text.Json;
using DotNetEnv;
using JobApi.Common;

namespace JobApi.ETL;

public class EmbeddingBatchGenerator
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Loading environment variables...");
        Env.Load();

        var outputFile = args.Length > 0 ? args[0] : "openai_batch_input.jsonl";

        Console.WriteLine($"Generating OpenAI batch file: {outputFile}");
        await GenerateBatchFile(outputFile);
        return 0;
    }

    private static async Task GenerateBatchFile(string outputFile)
    {
        using var db = JobContext.Create();

        // Get all jobs that don't have embeddings yet
        var jobs = db.Jobs
            .Where(j => j.Embedding == null && j.JobTitle != null && j.JobDescription != null)
            .Select(j => new { j.Id, j.JobTitle, j.JobDescription })
            .ToList();

        Console.WriteLine($"Found {jobs.Count} jobs without embeddings");

        if (jobs.Count == 0)
        {
            Console.WriteLine("No jobs to process!");
            return;
        }

        using var writer = new StreamWriter(outputFile);
        var processedCount = 0;

        foreach (var job in jobs)
        {
            // Combine job title and description
            var input = $"{job.JobTitle}\n\n{job.JobDescription}";

            // Truncate if too long (max ~8000 tokens for text-embedding-3-small)
            // Rough estimate: 1 token ≈ 4 chars, so ~32000 chars max
            if (input.Length > 32000)
            {
                input = input.Substring(0, 32000);
            }

            // Create OpenAI batch request format
            var batchRequest = new
            {
                custom_id = $"job-{job.Id}",
                method = "POST",
                url = "/v1/embeddings",
                body = new
                {
                    model = "text-embedding-3-small",
                    input = input,
                    input_type = "document"
                }
            };

            // Write as single-line JSON
            var json = JsonSerializer.Serialize(batchRequest);
            await writer.WriteLineAsync(json);

            processedCount++;
            if (processedCount % 10000 == 0)
            {
                Console.WriteLine($"Generated {processedCount} requests...");
            }
        }

        Console.WriteLine($"\n✅ Batch file created: {outputFile}");
        Console.WriteLine($"   Total requests: {processedCount}");
        Console.WriteLine($"\nNext steps:");
        Console.WriteLine($"1. Upload file to OpenAI:");
        Console.WriteLine($"   curl https://api.openai.com/v1/files \\");
        Console.WriteLine($"     -H \"Authorization: Bearer $OPENAI_API_KEY\" \\");
        Console.WriteLine($"     -F purpose=batch \\");
        Console.WriteLine($"     -F file=@{outputFile}");
        Console.WriteLine();
        Console.WriteLine($"2. Create batch job with the returned file ID");
        Console.WriteLine($"3. Poll for completion");
        Console.WriteLine($"4. Download and process results");
    }
}

// Main entry point - orchestrates ETL pipeline stages
using DotNetEnv;
using JobApi.ETL.Stages;

class Program
{
    private static string _dataRootPath = string.Empty;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== JobApi ETL Pipeline ===\n");

        // Load environment variables from solution root
        Console.WriteLine("Loading environment variables...");
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        if (System.IO.File.Exists(envPath))
        {
            Env.Load(envPath);
        }
        else
        {
            Console.WriteLine($"Warning: .env file not found at {envPath}");
        }

        // Hardcoded data root path
        _dataRootPath = "/mnt/c/GIT/JobApi.New/Data";
        Console.WriteLine($"Data root: {_dataRootPath}\n");

        // Define available stages
        var stages = new Dictionary<string, Func<Task<bool>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["ingest"] = RunIngestStage,
            ["llm-batch"] = RunLlmBatchStage,
            ["llm-submit"] = RunLlmBatchSubmitStage,
            ["llm-check"] = RunLlmBatchCheckStage,
            ["llm-results"] = RunLlmResultsStage,
            ["location-batch"] = RunLocationBatchStage,
            ["location-submit"] = RunLocationBatchSubmitStage,
            ["location-check"] = RunLocationBatchCheckStage,
            ["location-results"] = RunLocationResultsStage,
            ["geocode"] = RunGeocodeStage,
            ["embedding-batch"] = RunEmbeddingBatchStage,
            ["embedding-submit"] = RunEmbeddingBatchSubmitStage,
            ["embedding-check"] = RunEmbeddingBatchCheckStage,
            ["embedding-results"] = RunEmbeddingResultsStage
        };

        // Determine which stages to run
        if (args.Length == 0)
        {
            // No arguments - run all stages in sequence
            Console.WriteLine("No stage specified. Running all stages...\n");
            await RunAllStages();
        }
        else
        {
            // Run specified stages
            foreach (var stageName in args)
            {
                if (stages.TryGetValue(stageName, out var stageFunc))
                {
                    Console.WriteLine($"\n>>> Running stage: {stageName} <<<\n");
                    var success = await stageFunc();

                    if (!success)
                    {
                        Console.WriteLine($"\nERROR: Stage '{stageName}' failed. Stopping execution.");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.WriteLine($"\nERROR: Unknown stage '{stageName}'");
                    Console.WriteLine("\nAvailable stages:");
                    foreach (var stage in stages.Keys.OrderBy(k => k))
                    {
                        Console.WriteLine($"  - {stage}");
                    }
                    Environment.Exit(1);
                }
            }
        }

        Console.WriteLine("\n=== Pipeline Complete ===");
    }

    private static async Task RunAllStages()
    {
        if (!await RunIngestStage()) return;
        if (!await RunLlmBatchStage()) return;
        if (!await RunLlmBatchSubmitStage()) return;
        if (!await RunLlmBatchCheckStage()) return;
        if (!await RunLlmResultsStage()) return;
        if (!await RunLocationBatchStage()) return;
        if (!await RunLocationBatchSubmitStage()) return;
        if (!await RunLocationBatchCheckStage()) return;
        if (!await RunLocationResultsStage()) return;
        if (!await RunGeocodeStage()) return;
        if (!await RunEmbeddingBatchStage()) return;
        if (!await RunEmbeddingBatchSubmitStage()) return;
        if (!await RunEmbeddingBatchCheckStage()) return;
        if (!await RunEmbeddingResultsStage()) return;
    }

    // Stage 1: Ingest
    private static async Task<bool> RunIngestStage()
    {
        Console.WriteLine("=== STAGE 1: INGEST ===");

        var ingestableFolder = Path.Combine(_dataRootPath, "Ingestable");
        var hasFilesToIngest = Directory.Exists(ingestableFolder) &&
                               (Directory.GetFiles(ingestableFolder, "*.jsonl").Length > 0 ||
                                Directory.GetFiles(ingestableFolder, "*.jsonl.gz").Length > 0);

        if (!hasFilesToIngest)
        {
            Console.WriteLine("No files found in Ingestable folder. Skipping ingestion.\n");
            return true;
        }

        Console.WriteLine("Files found in Ingestable folder. Starting ingestion...\n");
        var stage = new IngestStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Ingestion failed.");
            return false;
        }

        Console.WriteLine("\n=== Ingestion Complete ===");
        return true;
    }

    // Stage 2: LLM Batch Generation
    private static async Task<bool> RunLlmBatchStage()
    {
        Console.WriteLine("=== STAGE 2: LLM BATCH GENERATION ===");
        var stage = new LlmBatchStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: LLM batch generation failed.");
            return false;
        }

        return true;
    }

    // Stage 3: LLM Batch Submit
    private static async Task<bool> RunLlmBatchSubmitStage()
    {
        Console.WriteLine("=== STAGE 3: LLM BATCH SUBMIT ===");
        var stage = new LlmBatchSubmitStage();
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: LLM batch submission failed.");
            return false;
        }

        return true;
    }

    // Stage 4: LLM Batch Check
    private static async Task<bool> RunLlmBatchCheckStage()
    {
        Console.WriteLine("=== STAGE 4: LLM BATCH CHECK ===");
        var stage = new LlmBatchCheckStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: LLM batch check failed.");
            return false;
        }

        return true;
    }

    // Stage 5: LLM Results Processing
    private static async Task<bool> RunLlmResultsStage()
    {
        Console.WriteLine("=== STAGE 5: LLM RESULTS PROCESSING ===");
        var stage = new LlmResultsStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: LLM results processing failed.");
            return false;
        }

        return true;
    }

    // Stage 6: Location Batch Generation
    private static async Task<bool> RunLocationBatchStage()
    {
        Console.WriteLine("=== STAGE 6: LOCATION BATCH GENERATION ===");
        var stage = new LocationBatchStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Location batch generation failed.");
            return false;
        }

        return true;
    }

    // Stage 7: Location Batch Submit
    private static async Task<bool> RunLocationBatchSubmitStage()
    {
        Console.WriteLine("=== STAGE 7: LOCATION BATCH SUBMIT ===");
        var stage = new LocationBatchSubmitStage();
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Location batch submission failed.");
            return false;
        }

        return true;
    }

    // Stage 8: Location Batch Check
    private static async Task<bool> RunLocationBatchCheckStage()
    {
        Console.WriteLine("=== STAGE 8: LOCATION BATCH CHECK ===");
        var stage = new LocationBatchCheckStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Location batch check failed.");
            return false;
        }

        return true;
    }

    // Stage 9: Location Results Processing
    private static async Task<bool> RunLocationResultsStage()
    {
        Console.WriteLine("=== STAGE 9: LOCATION RESULTS PROCESSING ===");
        var stage = new LocationResultsStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Location results processing failed.");
            return false;
        }

        return true;
    }

    // Stage 10: Geocode
    private static async Task<bool> RunGeocodeStage()
    {
        Console.WriteLine("=== STAGE 10: GEOCODE ===");
        var stage = new GeocodeStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Geocoding failed.");
            return false;
        }

        return true;
    }

    // Stage 11: Embedding Batch Generation
    private static async Task<bool> RunEmbeddingBatchStage()
    {
        Console.WriteLine("=== STAGE 11: EMBEDDING BATCH GENERATION ===");
        var stage = new EmbeddingBatchStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Embedding batch generation failed.");
            return false;
        }

        return true;
    }

    // Stage 12: Embedding Batch Submit
    private static async Task<bool> RunEmbeddingBatchSubmitStage()
    {
        Console.WriteLine("=== STAGE 12: EMBEDDING BATCH SUBMIT ===");
        var stage = new EmbeddingBatchSubmitStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Embedding batch submission failed.");
            return false;
        }

        return true;
    }

    // Stage 13: Embedding Batch Check
    private static async Task<bool> RunEmbeddingBatchCheckStage()
    {
        Console.WriteLine("=== STAGE 13: EMBEDDING BATCH CHECK ===");
        var stage = new EmbeddingBatchCheckStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Embedding batch check failed.");
            return false;
        }

        return true;
    }

    // Stage 14: Embedding Results Processing
    private static async Task<bool> RunEmbeddingResultsStage()
    {
        Console.WriteLine("=== STAGE 14: EMBEDDING RESULTS PROCESSING ===");
        var stage = new EmbeddingResultsStage(_dataRootPath);
        var success = await stage.ExecuteAsync();

        if (!success)
        {
            Console.WriteLine("\nERROR: Embedding results processing failed.");
            return false;
        }

        return true;
    }
}

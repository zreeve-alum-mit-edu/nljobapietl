// Standalone Location Flex Processor - processes batch files using OpenAI's flex service tier with dynamic rate limiting
// Compile and run: dotnet-script LocationFlexProcessor.cs
// Or: csc LocationFlexProcessor.cs && mono LocationFlexProcessor.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

class LocationFlexProcessor
{
    // Dynamic rate limiting settings
    private static int _currentMaxConcurrency = 1000;
    private const int MinConcurrency = 100;
    private const int MaxConcurrency = 1500;
    private const int SuccessThreshold = 1000; // 100 * 10
    private static int _successfulRequests = 0;
    private static int _activeRequests = 0;
    private static readonly object _concurrencyLock = new object();

    private const int DelayBetweenCallsMs = 0;
    private const string DataRoot = "/mnt/c/GIT/JobApi.New/Data";
    private const string BatchFolder = "/mnt/c/GIT/JobApi.New/Data/locationbatch";
    private const string ResultFolder = "/mnt/c/GIT/JobApi.New/Data/locationresult";
    private const string ErrorFolder = "/mnt/c/GIT/JobApi.New/Data/locationbatcherrored";

    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private static string? _openAiApiKey;
    private static string? _dbConnectionString;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Location Flex Processor (Dynamic Rate Limiting) ===");
        Console.WriteLine("Processing batches with OpenAI flex service tier\n");

        // Load environment variables
        LoadEnvironment();

        // Ensure output folders exist
        Directory.CreateDirectory(ResultFolder);
        Directory.CreateDirectory(ErrorFolder);

        // Get all batch files
        var batchFiles = Directory.GetFiles(BatchFolder, "location_batch_*.jsonl")
            .OrderBy(f => f)
            .ToList();

        if (batchFiles.Count == 0)
        {
            Console.WriteLine("No batch files found!");
            return;
        }

        Console.WriteLine($"Found {batchFiles.Count} batch file(s) to process");
        Console.WriteLine($"Starting concurrency: {_currentMaxConcurrency}\n");

        // Process each batch file sequentially
        int completedBatches = 0;
        int totalBatches = batchFiles.Count;

        foreach (var batchFilePath in batchFiles)
        {
            var fileName = Path.GetFileName(batchFilePath);
            var batchId = ExtractBatchId(fileName);

            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine($"Batch {completedBatches + 1}/{totalBatches}: {fileName}");
            Console.WriteLine($"{'=',-60}");

            var success = await ProcessBatchFile(batchFilePath, batchId, completedBatches + 1, totalBatches);

            if (success)
            {
                completedBatches++;
            }
            else
            {
                Console.WriteLine($"\nERROR: Failed to process batch {fileName}");
                Console.WriteLine("Stopping execution.");
                Environment.Exit(1);
            }
        }

        Console.WriteLine($"\n{'=',-60}");
        Console.WriteLine($"ALL BATCHES COMPLETE!");
        Console.WriteLine($"Successfully processed {completedBatches}/{totalBatches} batches");
        Console.WriteLine($"Final concurrency: {_currentMaxConcurrency}");
        Console.WriteLine($"{'=',-60}");
    }

    private static void LoadEnvironment()
    {
        // Try to load from .env file
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        }

        var dbHost = "";
        var dbName = "";
        var dbUser = "";
        var dbPassword = "";
        var dbPort = "5432";

        if (File.Exists(envPath))
        {
            foreach (var line in File.ReadAllLines(envPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"');

                    if (key == "OPENAI_API_KEY")
                        _openAiApiKey = value;
                    else if (key == "DB_CONNECTION_STRING")
                        _dbConnectionString = value;
                    else if (key == "DB_HOST")
                        dbHost = value;
                    else if (key == "DB_NAME")
                        dbName = value;
                    else if (key == "DB_USER")
                        dbUser = value;
                    else if (key == "DB_PASSWORD")
                        dbPassword = value;
                    else if (key == "DB_PORT")
                        dbPort = value;
                }
            }
        }

        // Fall back to environment variables
        _openAiApiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _dbConnectionString ??= Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        // Build connection string from individual components if not provided
        if (string.IsNullOrEmpty(_dbConnectionString) && !string.IsNullOrEmpty(dbHost))
        {
            _dbConnectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Port={dbPort}";
        }

        if (string.IsNullOrEmpty(_openAiApiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY not found in environment or .env file");
            Environment.Exit(1);
        }

        if (string.IsNullOrEmpty(_dbConnectionString))
        {
            Console.WriteLine("ERROR: DB_CONNECTION_STRING not found in environment or .env file");
            Environment.Exit(1);
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
    }

    private static string ExtractBatchId(string fileName)
    {
        // Extract GUID from location_batch_<guid>_<number>.jsonl or location_batch_consolidated_<guid>.jsonl
        var withoutPrefix = fileName.Replace("location_batch_", "").Replace(".jsonl", "");
        var parts = withoutPrefix.Split('_');

        // If starts with "consolidated", the guid is the next part
        if (parts.Length > 1 && parts[0] == "consolidated")
        {
            return parts[1];
        }

        // Otherwise, guid is the first part
        return parts.Length >= 1 ? parts[0] : Guid.NewGuid().ToString();
    }

    private static async Task<bool> ProcessBatchFile(string batchFilePath, string batchId, int currentBatch, int totalBatches)
    {
        try
        {
            // Update database status to 'processing' (optional - ignore if record doesn't exist)
            await UpdateBatchStatus(batchId, "processing");

            // Read all lines from batch file
            var lines = await File.ReadAllLinesAsync(batchFilePath);
            var totalRequests = lines.Length;

            Console.WriteLine($"Total requests: {totalRequests}");
            Console.WriteLine($"Current concurrency: {_currentMaxConcurrency}\n");

            // Prepare output files
            var resultFilePath = Path.Combine(ResultFolder, $"location_results_{batchId}.jsonl");
            var errorFilePath = Path.Combine(ErrorFolder, $"location_errors_{batchId}.jsonl");

            // Thread-safe counters and writers
            var completed = 0;
            var errors = 0;
            var resultLock = new object();
            var errorLock = new object();

            using var resultWriter = new StreamWriter(resultFilePath, append: false);
            using var errorWriter = new StreamWriter(errorFilePath, append: false);

            var tasks = new List<Task>();

            // Process each line
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Wait for available slot
                while (Interlocked.CompareExchange(ref _activeRequests, 0, 0) >= _currentMaxConcurrency)
                {
                    await Task.Delay(10);
                }

                Interlocked.Increment(ref _activeRequests);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        var batchRequest = JsonSerializer.Deserialize<BatchRequest>(line);
                        if (batchRequest == null)
                        {
                            throw new Exception("Failed to parse batch request");
                        }

                        // Add service_tier to the request body
                        var requestBodyJson = JsonSerializer.Serialize(batchRequest.Body);
                        var requestBodyDict = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBodyJson);

                        if (requestBodyDict != null)
                        {
                            requestBodyDict["service_tier"] = "flex";
                            var modifiedBody = JsonSerializer.Serialize(requestBodyDict);

                            // Send to OpenAI (handles rate limiting internally)
                            var response = await SendToOpenAI(modifiedBody);

                            // Write success result in batch output format
                            var batchResponse = new BatchResponse
                            {
                                Id = $"batch_req_{Guid.NewGuid()}",
                                CustomId = batchRequest.CustomId,
                                Response = new ResponseData
                                {
                                    StatusCode = 200,
                                    Body = response
                                }
                            };

                            lock (resultLock)
                            {
                                resultWriter.WriteLine(JsonSerializer.Serialize(batchResponse));
                                resultWriter.Flush();
                            }

                            Interlocked.Increment(ref completed);

                            // Track successful request for rate limiting
                            OnRequestSuccess();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Write original line to error file
                        lock (errorLock)
                        {
                            errorWriter.WriteLine(line);
                            errorWriter.Flush();
                        }

                        Interlocked.Increment(ref errors);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeRequests);
                    }
                });

                tasks.Add(task);

                // Small delay between submissions
                await Task.Delay(DelayBetweenCallsMs);

                // Show progress every 5000 requests
                if ((i + 1) % 5000 == 0)
                {
                    Console.WriteLine($"Progress: {completed}/{totalRequests} completed ({errors} errors, {_activeRequests} active, concurrency: {_currentMaxConcurrency})");
                }
            }

            // Wait for all tasks to complete
            Console.WriteLine("\nWaiting for remaining requests...");
            await Task.WhenAll(tasks);

            Console.WriteLine($"\nBatch Complete!");
            Console.WriteLine($"  Completed: {completed}/{totalRequests}");
            Console.WriteLine($"  Errors: {errors}");
            Console.WriteLine($"  Success rate: {(completed * 100.0 / totalRequests):F2}%");

            // Update database status to 'completed' (optional - ignore if record doesn't exist)
            await UpdateBatchStatus(batchId, "completed");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR processing batch: {ex.Message}");
            Console.WriteLine(ex.StackTrace);

            // Update database status to 'failed' (optional - ignore if record doesn't exist)
            try
            {
                await UpdateBatchStatus(batchId, "failed", ex.Message);
            }
            catch (Exception dbEx)
            {
                Console.WriteLine($"Failed to update database status: {dbEx.Message}");
            }

            return false;
        }
    }

    private static async Task<Dictionary<string, object>> SendToOpenAI(string requestBodyJson)
    {
        using var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

        var maxRetries = 3;
        var retryDelay = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)
                        ?? throw new Exception("Failed to parse OpenAI response");
                }

                // Rate limit error (429) - trigger backoff
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    OnRateLimitHit();

                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(retryDelay * (i + 1));
                        continue;
                    }
                }

                // Ignore 502/503 errors, just retry
                if (response.StatusCode == HttpStatusCode.BadGateway ||
                    response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(retryDelay * (i + 1));
                        continue;
                    }
                }

                throw new Exception($"OpenAI API error: {response.StatusCode} - {responseContent}");
            }
            catch (HttpRequestException) when (i < maxRetries - 1)
            {
                // Network error - retry
                await Task.Delay(retryDelay * (i + 1));
            }
        }

        throw new Exception("Failed after max retries");
    }

    private static void OnRequestSuccess()
    {
        var successCount = Interlocked.Increment(ref _successfulRequests);

        if (successCount >= SuccessThreshold)
        {
            lock (_concurrencyLock)
            {
                if (_successfulRequests >= SuccessThreshold) // Double-check
                {
                    var oldConcurrency = _currentMaxConcurrency;
                    var newConcurrency = Math.Min((int)Math.Round(_currentMaxConcurrency * 1.2), MaxConcurrency);

                    if (newConcurrency > oldConcurrency)
                    {
                        _currentMaxConcurrency = newConcurrency;
                        var count = _successfulRequests;
                        _successfulRequests = 0;
                        Console.WriteLine($"\n✓ Increased concurrency: {oldConcurrency} → {newConcurrency} (after {count} successful requests)\n");
                    }
                }
            }
        }
    }

    private static void OnRateLimitHit()
    {
        lock (_concurrencyLock)
        {
            var oldConcurrency = _currentMaxConcurrency;
            var newConcurrency = Math.Max((int)Math.Round(_currentMaxConcurrency * 0.7), MinConcurrency);

            if (newConcurrency < oldConcurrency)
            {
                _currentMaxConcurrency = newConcurrency;
                _successfulRequests = 0;
                Console.WriteLine($"\n⚠ Rate limit hit! Decreased concurrency: {oldConcurrency} → {newConcurrency}\n");
            }
        }
    }

    private static async Task UpdateBatchStatus(string batchId, string status, string? errorMessage = null)
    {
        var maxRetries = 5;
        var retryDelay = 2000; // Start with 2 seconds

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var conn = new NpgsqlConnection(_dbConnectionString);
                await conn.OpenAsync();

                var sql = errorMessage == null
                    ? "UPDATE location_batches SET status = @status WHERE id = @id"
                    : "UPDATE location_batches SET status = @status, error_message = @error WHERE id = @id";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", Guid.Parse(batchId));

                if (errorMessage != null)
                {
                    cmd.Parameters.AddWithValue("@error", errorMessage);
                }

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    Console.WriteLine($"Note: No location_batches record found for batch {batchId} (ignoring)");
                }
                return; // Success - exit retry loop
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Database connection error (attempt {i + 1}/{maxRetries}): {ex.Message}");
                Console.WriteLine($"Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff: 2s, 4s, 8s, 16s
            }
            catch (Exception ex)
            {
                // Last attempt failed - just log and continue
                Console.WriteLine($"Note: Failed to update batch status after {maxRetries} attempts: {ex.Message}");
                return; // Don't throw, just return
            }
        }
    }
}

// Data models
class BatchRequest
{
    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = "";

    [JsonPropertyName("method")]
    public string Method { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("body")]
    public Dictionary<string, object> Body { get; set; } = new();
}

class BatchResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = "";

    [JsonPropertyName("response")]
    public ResponseData Response { get; set; } = new();
}

class ResponseData
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("body")]
    public Dictionary<string, object> Body { get; set; } = new();
}

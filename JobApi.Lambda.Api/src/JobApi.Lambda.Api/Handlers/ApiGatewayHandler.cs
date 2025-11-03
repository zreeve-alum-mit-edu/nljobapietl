using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.Api.Handlers;

public class ApiGatewayHandler
{
    private readonly SearchHandler _searchHandler;
    private readonly LocationHandler _locationHandler;
    private readonly HashSet<string> _apiKeys;

    public ApiGatewayHandler()
    {
        _searchHandler = new SearchHandler();
        _locationHandler = new LocationHandler();

        // Support multiple API keys (comma-separated in API_KEYS environment variable)
        var apiKeysEnv = Environment.GetEnvironmentVariable("API_KEYS") ?? "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";
        _apiKeys = new HashSet<string>(apiKeysEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    /// <summary>
    /// Main Lambda handler - routes requests to appropriate handler
    /// </summary>
    public async Task<APIGatewayProxyResponse> HandleRequest(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        // Log the entire request for debugging
        var requestJson = System.Text.Json.JsonSerializer.Serialize(request);
        context.Logger.LogInformation($"Full Request: {requestJson}");

        try
        {
            // Validate API key
            if (!ValidateApiKey(request))
            {
                context.Logger.LogWarning("Invalid or missing API key");
                return CreateResponse(403, new { message = "Forbidden: Invalid or missing API key" });
            }

            // Extract route from API Gateway v2 format (RouteKey) or v1 format (Path + HttpMethod)
            string method = "";
            string path = "";

            if (request.RequestContext?.RouteKey != null)
            {
                // API Gateway v2 (HTTP API) format: "GET /locations/validate"
                var parts = request.RequestContext.RouteKey.Split(' ', 2);
                if (parts.Length == 2)
                {
                    method = parts[0];
                    path = parts[1];
                }
            }
            else
            {
                // API Gateway v1 (REST API) format
                method = request.HttpMethod ?? "";
                path = request.Path ?? request.Resource ?? "";
            }

            context.Logger.LogInformation($"Routing: {method} {path}");

            if (path.EndsWith("/search") && method == "POST")
            {
                return await HandleSearchRequest(request, context);
            }
            else if (path.EndsWith("/locations/validate") && method == "GET")
            {
                return await HandleLocationRequest(request, context);
            }
            else
            {
                context.Logger.LogWarning($"No route matched for {method} {path}");
                return CreateResponse(404, new { message = "Not Found" });
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex.Message}");
            context.Logger.LogError($"Stack trace: {ex.StackTrace}");
            return CreateResponse(500, new { message = "Internal Server Error" });
        }
    }

    /// <summary>
    /// Validates the x-api-key header against the list of valid API keys
    /// </summary>
    private bool ValidateApiKey(APIGatewayProxyRequest request)
    {
        var headers = request.Headers ?? new Dictionary<string, string>();

        if (!headers.TryGetValue("x-api-key", out var providedKey))
        {
            return false;
        }

        return _apiKeys.Contains(providedKey);
    }

    private async Task<APIGatewayProxyResponse> HandleSearchRequest(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            // Deserialize request body
            var searchRequest = System.Text.Json.JsonSerializer.Deserialize<Models.SearchRequest>(request.Body ?? "");

            if (searchRequest == null)
            {
                return CreateResponse(400, new { message = "Request body is missing or invalid JSON. Expected format: {\"prompt\": \"...\", \"numJobs\": 10, \"includeRemote\": true, \"filters\": [...]}" });
            }

            // Validate prompt
            if (string.IsNullOrWhiteSpace(searchRequest.Prompt))
            {
                return CreateResponse(400, new { message = "Prompt is required and cannot be empty. Provide a natural language job description (e.g., 'senior software engineer with Python experience')" });
            }

            if (searchRequest.Prompt.Length < 10)
            {
                return CreateResponse(400, new { message = $"Prompt must be at least 10 characters long (received: {searchRequest.Prompt.Length} characters)" });
            }

            if (searchRequest.Prompt.Length > 20000)
            {
                return CreateResponse(400, new { message = $"Prompt cannot exceed 20,000 characters (received: {searchRequest.Prompt.Length} characters)" });
            }

            // Validate numJobs
            if (searchRequest.NumJobs < 1)
            {
                return CreateResponse(400, new { message = $"numJobs must be at least 1 (received: {searchRequest.NumJobs})" });
            }

            if (searchRequest.NumJobs > 100)
            {
                return CreateResponse(400, new { message = $"numJobs cannot exceed 100 (received: {searchRequest.NumJobs})" });
            }

            // Validate filters (0-10 allowed, can be empty if includeRemote is true)
            if (searchRequest.Filters.Count > 10)
            {
                return CreateResponse(400, new { message = $"Maximum 10 location filters allowed (received: {searchRequest.Filters.Count} filters)" });
            }

            // Must have either remote or at least one location filter
            if (!searchRequest.IncludeRemote && searchRequest.Filters.Count == 0)
            {
                return CreateResponse(400, new { message = "Must either set includeRemote=true or provide at least one location filter. Cannot search with no workplace types selected." });
            }

            // Validate each filter
            for (int i = 0; i < searchRequest.Filters.Count; i++)
            {
                var filter = searchRequest.Filters[i];

                if (!filter.IncludeOnsite && !filter.IncludeHybrid)
                {
                    return CreateResponse(400, new { message = $"Filter #{i + 1}: At least one of 'includeOnsite' or 'includeHybrid' must be true. Current values: includeOnsite={filter.IncludeOnsite}, includeHybrid={filter.IncludeHybrid}" });
                }

                if (string.IsNullOrWhiteSpace(filter.Location))
                {
                    return CreateResponse(400, new { message = $"Filter #{i + 1}: Location is required and must be in 'City,State' format (e.g., 'Austin,TX')" });
                }

                // Validate location format (should be "City,State")
                var parts = filter.Location.Split(',');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    return CreateResponse(400, new { message = $"Filter #{i + 1}: Location must be in 'City,State' format (e.g., 'Austin,TX'). Received: '{filter.Location}'" });
                }

                if (filter.Miles < 1)
                {
                    return CreateResponse(400, new { message = $"Filter #{i + 1}: Miles must be at least 1 (received: {filter.Miles})" });
                }

                if (filter.Miles > 50)
                {
                    return CreateResponse(400, new { message = $"Filter #{i + 1}: Miles cannot exceed 50 (received: {filter.Miles})" });
                }
            }

            // Validate all locations exist in database
            if (searchRequest.Filters.Count > 0)
            {
                var invalidLocation = await ValidateLocationsExist(searchRequest.Filters, context);
                if (invalidLocation != null)
                {
                    return CreateResponse(400, new { message = $"Location '{invalidLocation}' not found in database. Please use the /locations/validate endpoint to verify the location exists and get the correct format (e.g., GET /locations/validate?city=Austin&state=TX)" });
                }
            }

            context.Logger.LogInformation($"Search request validated: prompt='{searchRequest.Prompt}', numJobs={searchRequest.NumJobs}, includeRemote={searchRequest.IncludeRemote}, filters={searchRequest.Filters.Count}");

            // Call search handler
            var response = await _searchHandler.Search(searchRequest, context);

            return CreateResponse(200, response);
        }
        catch (Handlers.OpenAIServiceException ex)
        {
            context.Logger.LogError($"OpenAI service failure: {ex.Message}");
            return CreateResponse(503, new { message = ex.Message });
        }
        catch (System.Text.Json.JsonException ex)
        {
            context.Logger.LogError($"JSON parsing error: {ex.Message}");
            return CreateResponse(400, new { message = $"Invalid JSON in request body: {ex.Message}. Please verify your JSON syntax." });
        }
    }

    private async Task<APIGatewayProxyResponse> HandleLocationRequest(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        // Extract query parameters
        var queryParams = request.QueryStringParameters ?? new Dictionary<string, string>();

        if (!queryParams.TryGetValue("city", out var city) || string.IsNullOrWhiteSpace(city))
        {
            return CreateResponse(400, new { message = "City parameter is required" });
        }

        if (!queryParams.TryGetValue("state", out var state) || string.IsNullOrWhiteSpace(state))
        {
            return CreateResponse(400, new { message = "State parameter is required" });
        }

        // Country is optional, defaults to "US"
        var country = queryParams.TryGetValue("country", out var countryParam) ? countryParam : "US";

        context.Logger.LogInformation($"Validating location: {city}, {state}, {country}");

        var response = await _locationHandler.ValidateLocation(city, state, country, context);

        return CreateResponse(200, response);
    }

    /// <summary>
    /// Validates that all locations in filters exist in the geolocations database
    /// Returns the first invalid location found, or null if all are valid
    /// </summary>
    private async Task<string?> ValidateLocationsExist(List<Models.LocationFilter> filters, ILambdaContext context)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST not set");
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME not set");
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER not set");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD not set");
        var connectionString = $"Host={host};Database={database};Username={username};Password={password}";

        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var filter in filters)
        {
            // Parse location "City,State"
            var parts = filter.Location.Split(',');
            var city = parts[0].Trim();
            var state = parts[1].Trim();

            // Check if location exists in geolocations table
            var sql = @"
                SELECT COUNT(*)
                FROM geolocations
                WHERE LOWER(city) = LOWER(@city)
                AND LOWER(state) = LOWER(@state)";

            await using var cmd = new Npgsql.NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("city", city);
            cmd.Parameters.AddWithValue("state", state);

            var count = (long)(await cmd.ExecuteScalarAsync() ?? 0);

            if (count == 0)
            {
                context.Logger.LogWarning($"Location validation failed: '{filter.Location}' not found in database");
                return filter.Location;
            }
        }

        context.Logger.LogInformation($"All {filters.Count} location(s) validated successfully");
        return null;
    }

    private APIGatewayProxyResponse CreateResponse(int statusCode, object body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            }
        };
    }
}

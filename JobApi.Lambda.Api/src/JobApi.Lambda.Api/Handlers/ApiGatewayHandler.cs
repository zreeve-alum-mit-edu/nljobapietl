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
        // TODO: Implement search request handling
        // 1. Deserialize request body to SearchRequest
        // 2. Validate request (limit 1-100, filters 1-5)
        // 3. Call _searchHandler.Search()
        // 4. Return response

        throw new NotImplementedException("Search handler not yet implemented");
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

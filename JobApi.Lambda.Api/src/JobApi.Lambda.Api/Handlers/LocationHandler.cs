using Amazon.Lambda.Core;
using JobApi.Lambda.Api.Models;
using Npgsql;

namespace JobApi.Lambda.Api.Handlers;

public class LocationHandler
{
    private readonly string _connectionString;

    public LocationHandler()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST not set");
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME not set");
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER not set");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD not set");

        _connectionString = $"Host={host};Database={database};Username={username};Password={password}";
    }

    /// <summary>
    /// Validates a location and provides suggestions if invalid
    /// </summary>
    public async Task<LocationResponse> ValidateLocation(
        string city,
        string state,
        string country = "US",
        ILambdaContext? context = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check for exact match (case-insensitive)
        var exactMatchSql = @"
            SELECT COUNT(*)
            FROM geolocations
            WHERE LOWER(city) = LOWER(@city)
            AND LOWER(state) = LOWER(@state)
            AND (country IS NULL OR LOWER(country) = LOWER(@country))";

        await using (var cmd = new NpgsqlCommand(exactMatchSql, connection))
        {
            cmd.Parameters.AddWithValue("city", city);
            cmd.Parameters.AddWithValue("state", state);
            cmd.Parameters.AddWithValue("country", country);

            var count = (long)(await cmd.ExecuteScalarAsync() ?? 0);

            if (count > 0)
            {
                context?.Logger.LogInformation($"Valid location: {city},{state}");
                return new LocationResponse { Valid = true };
            }
        }

        // Location not found - get suggestions
        context?.Logger.LogInformation($"Invalid location: {city},{state} - finding suggestions");
        var suggestions = await FindSuggestions(connection, city, state);

        return new LocationResponse
        {
            Valid = false,
            Suggestions = suggestions
        };
    }

    /// <summary>
    /// Finds similar city names using trigram similarity (pg_trgm extension)
    /// </summary>
    private async Task<List<string>> FindSuggestions(
        NpgsqlConnection connection,
        string city,
        string state)
    {
        // Use pg_trgm trigram similarity for fuzzy matching
        // This catches typos, misspellings, and partial matches
        var suggestionSql = @"
            SELECT city, state, similarity(city, @city) as score
            FROM geolocations
            WHERE LOWER(state) = LOWER(@state)
              AND similarity(city, @city) > 0.3
            ORDER BY score DESC
            LIMIT 5";

        var suggestions = new List<string>();

        await using (var cmd = new NpgsqlCommand(suggestionSql, connection))
        {
            cmd.Parameters.AddWithValue("city", city);
            cmd.Parameters.AddWithValue("state", state);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var suggestedCity = reader.GetString(0);
                var suggestedState = reader.GetString(1);
                suggestions.Add($"{suggestedCity},{suggestedState}");
            }
        }

        return suggestions;
    }
}

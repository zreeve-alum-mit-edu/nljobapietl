using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class SearchResponse
{
    [JsonPropertyName("jobs")]
    public List<JobResult> Jobs { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}

public class JobResult
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("workplace")]
    public string Workplace { get; set; } = string.Empty;

    [JsonPropertyName("workplaceConfidence")]
    public string? WorkplaceConfidence { get; set; }

    [JsonPropertyName("locations")]
    public List<JobLocation> Locations { get; set; } = new();

    [JsonPropertyName("datePosted")]
    public DateTime? DatePosted { get; set; }
}

public class JobLocation
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; } = new();
}

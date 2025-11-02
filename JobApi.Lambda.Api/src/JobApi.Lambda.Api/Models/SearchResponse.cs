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

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("salary")]
    public string? Salary { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("datePosted")]
    public DateTime? DatePosted { get; set; }
}

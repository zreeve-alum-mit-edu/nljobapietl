using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class SearchRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("numJobs")]
    public int NumJobs { get; set; }

    [JsonPropertyName("includeOnsite")]
    public bool IncludeOnsite { get; set; }

    [JsonPropertyName("includeHybrid")]
    public bool IncludeHybrid { get; set; }

    [JsonPropertyName("daysSincePosting")]
    public int? DaysSincePosting { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("miles")]
    public int Miles { get; set; }
}

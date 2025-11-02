using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class SearchRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("numJobs")]
    public int NumJobs { get; set; }

    [JsonPropertyName("includeRemote")]
    public bool IncludeRemote { get; set; }

    [JsonPropertyName("daysSincePosting")]
    public int? DaysSincePosting { get; set; }

    [JsonPropertyName("filters")]
    public List<LocationFilter> Filters { get; set; } = new();
}

public class LocationFilter
{
    [JsonPropertyName("includeOnsite")]
    public bool IncludeOnsite { get; set; }

    [JsonPropertyName("includeHybrid")]
    public bool IncludeHybrid { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("miles")]
    public int Miles { get; set; }
}

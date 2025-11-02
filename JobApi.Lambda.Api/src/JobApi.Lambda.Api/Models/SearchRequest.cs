using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class SearchRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("filters")]
    public List<SearchFilter> Filters { get; set; } = new();
}

public class SearchFilter
{
    [JsonPropertyName("workplaceType")]
    public string WorkplaceType { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("miles")]
    public int? Miles { get; set; }
}

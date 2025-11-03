using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class RemoteSearchRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("numJobs")]
    public int NumJobs { get; set; }

    [JsonPropertyName("daysSincePosting")]
    public int? DaysSincePosting { get; set; }
}

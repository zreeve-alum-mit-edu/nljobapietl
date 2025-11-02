using System.Text.Json.Serialization;

namespace JobApi.Lambda.Api.Models;

public class LocationResponse
{
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("suggestions")]
    public List<string>? Suggestions { get; set; }
}

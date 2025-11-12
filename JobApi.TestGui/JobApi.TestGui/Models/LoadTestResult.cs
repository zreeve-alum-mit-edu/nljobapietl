namespace JobApi.TestGui.Models;

public class LoadTestResult
{
    public string PromptName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public int NumJobs { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int ResultCount { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int StatusCode { get; set; }

    // Calculated properties for display
    public string TotalTimeMs => $"{TotalTime.TotalMilliseconds:F0} ms";
    public string TotalTimeSec => $"{TotalTime.TotalSeconds:F2} s";
    public string StatusDisplay => Success ? "Success" : "Failed";
}

public class TestPrompt
{
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public int NumJobs { get; set; } = 10;
    public int? DaysSincePosting { get; set; }

    // Location-based search fields (null for remote-only searches)
    public string? City { get; set; }
    public string? State { get; set; }
    public int? Miles { get; set; }
    public bool? IncludeOnsite { get; set; }
    public bool? IncludeHybrid { get; set; }
}

public enum SearchEndpointType
{
    Remote,
    LocationBased
}

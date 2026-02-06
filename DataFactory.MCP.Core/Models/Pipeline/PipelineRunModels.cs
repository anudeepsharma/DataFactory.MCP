using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Request to run a pipeline
/// </summary>
public class RunPipelineRequest
{
    /// <summary>
    /// Parameters to pass to the pipeline run
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Response from running a pipeline
/// </summary>
public class RunPipelineResponse
{
    /// <summary>
    /// The pipeline run ID
    /// </summary>
    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Status of the pipeline run
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Pipeline ID
    /// </summary>
    [JsonPropertyName("pipelineId")]
    public string? PipelineId { get; set; }

    /// <summary>
    /// Workspace ID
    /// </summary>
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>
/// Response for getting pipeline run status
/// </summary>
public class PipelineRunStatus
{
    /// <summary>
    /// The pipeline run ID
    /// </summary>
    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Status of the pipeline run (e.g., "InProgress", "Succeeded", "Failed", "Cancelled")
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Start time of the run
    /// </summary>
    [JsonPropertyName("runStart")]
    public DateTime? RunStart { get; set; }

    /// <summary>
    /// End time of the run
    /// </summary>
    [JsonPropertyName("runEnd")]
    public DateTime? RunEnd { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    [JsonPropertyName("durationInMs")]
    public long? DurationInMs { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Response containing a list of pipelines
/// </summary>
public class ListPipelinesResponse
{
    /// <summary>
    /// List of pipelines
    /// </summary>
    [JsonPropertyName("value")]
    public List<Pipeline> Value { get; set; } = new();

    /// <summary>
    /// Continuation token for pagination
    /// </summary>
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Continuation URI for pagination
    /// </summary>
    [JsonPropertyName("continuationUri")]
    public string? ContinuationUri { get; set; }
}

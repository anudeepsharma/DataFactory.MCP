using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Pipeline definition containing activities and parameters
/// </summary>
public class PipelineDefinition
{
    /// <summary>
    /// List of activities in the pipeline
    /// </summary>
    [JsonPropertyName("activities")]
    public List<PipelineActivity> Activities { get; set; } = new();

    /// <summary>
    /// Pipeline parameters
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Pipeline variables
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// Pipeline annotations
    /// </summary>
    [JsonPropertyName("annotations")]
    public List<object>? Annotations { get; set; }
}

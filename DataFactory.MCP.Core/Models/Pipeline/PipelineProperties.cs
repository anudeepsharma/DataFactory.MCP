using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Properties of a Pipeline
/// </summary>
public class PipelineProperties
{
    /// <summary>
    /// The pipeline definition containing activities
    /// </summary>
    [JsonPropertyName("definition")]
    public PipelineDefinition? Definition { get; set; }
}

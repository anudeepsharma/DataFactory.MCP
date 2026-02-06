using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Request to update a pipeline definition
/// </summary>
public class UpdatePipelineRequest
{
    /// <summary>
    /// The updated pipeline definition
    /// </summary>
    [JsonPropertyName("definition")]
    public PipelineDefinition? Definition { get; set; }
}

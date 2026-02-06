using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Response from creating a pipeline
/// </summary>
public class CreatePipelineResponse
{
    /// <summary>
    /// The created pipeline ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The pipeline display name
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The pipeline description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The item type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "DataPipeline";

    /// <summary>
    /// The workspace ID
    /// </summary>
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// The folder ID
    /// </summary>
    [JsonPropertyName("folderId")]
    public string? FolderId { get; set; }
}

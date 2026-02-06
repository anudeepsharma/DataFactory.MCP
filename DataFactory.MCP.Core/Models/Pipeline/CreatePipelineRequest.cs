using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Request to create a new pipeline
/// </summary>
public class CreatePipelineRequest
{
    /// <summary>
    /// The pipeline display name
    /// </summary>
    [JsonPropertyName("displayName")]
    [Required(ErrorMessage = "DisplayName is required")]
    [MaxLength(256, ErrorMessage = "DisplayName cannot exceed 256 characters")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The pipeline description
    /// </summary>
    [JsonPropertyName("description")]
    [MaxLength(256, ErrorMessage = "Description cannot exceed 256 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The folder ID where the pipeline will be created
    /// </summary>
    [JsonPropertyName("folderId")]
    public string? FolderId { get; set; }

    /// <summary>
    /// The pipeline definition
    /// </summary>
    [JsonPropertyName("definition")]
    public PipelineDefinition? Definition { get; set; }
}

using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Pipeline;

/// <summary>
/// Represents an activity in a pipeline
/// </summary>
public class PipelineActivity
{
    /// <summary>
    /// Activity name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Activity type (e.g., "Copy", "Script", "Dataflow", "ExecutePipeline")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Activity description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Activities that this activity depends on
    /// </summary>
    [JsonPropertyName("dependsOn")]
    public List<ActivityDependency>? DependsOn { get; set; }

    /// <summary>
    /// User properties for the activity
    /// </summary>
    [JsonPropertyName("userProperties")]
    public List<UserProperty>? UserProperties { get; set; }

    /// <summary>
    /// Type-specific properties
    /// </summary>
    [JsonPropertyName("typeProperties")]
    public Dictionary<string, object> TypeProperties { get; set; } = new();

    /// <summary>
    /// Policy configuration for the activity
    /// </summary>
    [JsonPropertyName("policy")]
    public ActivityPolicy? Policy { get; set; }
}

/// <summary>
/// Represents an activity dependency
/// </summary>
public class ActivityDependency
{
    /// <summary>
    /// Name of the activity this depends on
    /// </summary>
    [JsonPropertyName("activity")]
    public string Activity { get; set; } = string.Empty;

    /// <summary>
    /// Dependency conditions (e.g., "Succeeded", "Failed", "Completed", "Skipped")
    /// </summary>
    [JsonPropertyName("dependencyConditions")]
    public List<string> DependencyConditions { get; set; } = new();
}

/// <summary>
/// User property for an activity
/// </summary>
public class UserProperty
{
    /// <summary>
    /// Property name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Property value
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Activity execution policy
/// </summary>
public class ActivityPolicy
{
    /// <summary>
    /// Timeout for the activity
    /// </summary>
    [JsonPropertyName("timeout")]
    public string? Timeout { get; set; }

    /// <summary>
    /// Retry count
    /// </summary>
    [JsonPropertyName("retry")]
    public int? Retry { get; set; }

    /// <summary>
    /// Retry interval in seconds
    /// </summary>
    [JsonPropertyName("retryIntervalInSeconds")]
    public int? RetryIntervalInSeconds { get; set; }

    /// <summary>
    /// Whether retry is exponential
    /// </summary>
    [JsonPropertyName("secureInput")]
    public bool? SecureInput { get; set; }

    /// <summary>
    /// Whether output is secure
    /// </summary>
    [JsonPropertyName("secureOutput")]
    public bool? SecureOutput { get; set; }
}

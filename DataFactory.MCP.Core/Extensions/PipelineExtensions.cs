using DataFactory.MCP.Models.Pipeline;

namespace DataFactory.MCP.Extensions;

/// <summary>
/// Extension methods for Pipeline objects
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Converts a Pipeline object to a formatted info object for display
    /// </summary>
    public static object ToFormattedInfo(this Pipeline pipeline)
    {
        var activities = pipeline.Properties?.Definition?.Activities?
            .Select(a => new { a.Name, a.Type })
            .Cast<object>()
            .ToList() ?? new List<object>();

        return new
        {
            pipeline.Id,
            pipeline.DisplayName,
            pipeline.Description,
            pipeline.Type,
            pipeline.WorkspaceId,
            pipeline.FolderId,
            ActivityCount = pipeline.Properties?.Definition?.Activities?.Count ?? 0,
            Activities = activities
        };
    }

    /// <summary>
    /// Converts a PipelineActivity to a formatted info object
    /// </summary>
    public static object ToFormattedInfo(this PipelineActivity activity)
    {
        return new
        {
            activity.Name,
            activity.Type,
            activity.Description,
            DependsOnCount = activity.DependsOn?.Count ?? 0,
            DependsOn = activity.DependsOn?.Select(d => d.Activity).ToList() ?? new List<string>(),
            HasPolicy = activity.Policy != null
        };
    }

    /// <summary>
    /// Converts a PipelineRunStatus to a formatted info object
    /// </summary>
    public static object ToFormattedInfo(this PipelineRunStatus status)
    {
        var duration = status.DurationInMs.HasValue
            ? TimeSpan.FromMilliseconds(status.DurationInMs.Value)
            : (TimeSpan?)null;

        return new
        {
            status.RunId,
            status.Status,
            RunStart = status.RunStart?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            RunEnd = status.RunEnd?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Duration = duration?.ToString(@"hh\:mm\:ss"),
            status.Message
        };
    }
}

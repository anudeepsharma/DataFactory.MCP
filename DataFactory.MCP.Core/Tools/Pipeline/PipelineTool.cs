using ModelContextProtocol.Server;
using System.ComponentModel;
using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Extensions;
using DataFactory.MCP.Models.Pipeline;

namespace DataFactory.MCP.Tools.Pipeline;

/// <summary>
/// MCP Tool for managing Microsoft Fabric Pipelines.
/// Handles CRUD operations, activity management, and pipeline execution.
/// </summary>
[McpServerToolType]
public class PipelineTool
{
    private readonly IFabricPipelineService _pipelineService;
    private readonly IValidationService _validationService;

    public PipelineTool(
        IFabricPipelineService pipelineService,
        IValidationService validationService)
    {
        _pipelineService = pipelineService;
        _validationService = validationService;
    }

    [McpServerTool, Description(@"Returns a list of Data Pipelines from the specified workspace. This API supports pagination.")]
    public async Task<string> ListPipelinesAsync(
        [Description("The workspace ID to list pipelines from (required)")] string workspaceId,
        [Description("A token for retrieving the next page of results (optional)")] string? continuationToken = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));

            var response = await _pipelineService.ListPipelinesAsync(workspaceId, continuationToken);

            if (!response.Value.Any())
            {
                return $"No pipelines found in workspace '{workspaceId}'.";
            }

            var result = new
            {
                WorkspaceId = workspaceId,
                PipelineCount = response.Value.Count,
                ContinuationToken = response.ContinuationToken,
                ContinuationUri = response.ContinuationUri,
                HasMoreResults = !string.IsNullOrEmpty(response.ContinuationToken),
                Pipelines = response.Value.Select(p => p.ToFormattedInfo())
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("listing pipelines").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Creates a Data Pipeline in the specified workspace. The workspace must be on a supported Fabric capacity.")]
    public async Task<string> CreatePipelineAsync(
        [Description("The workspace ID where the pipeline will be created (required)")] string workspaceId,
        [Description("The Pipeline display name (required)")] string displayName,
        [Description("The Pipeline description (optional, max 256 characters)")] string? description = null,
        [Description("The folder ID where the pipeline will be created (optional, defaults to workspace root)")] string? folderId = null)
    {
        try
        {
            var request = new CreatePipelineRequest
            {
                DisplayName = displayName,
                Description = description,
                FolderId = folderId,
                Definition = new PipelineDefinition()
            };

            var response = await _pipelineService.CreatePipelineAsync(workspaceId, request);

            var result = new
            {
                Success = true,
                Message = $"Pipeline '{displayName}' created successfully",
                PipelineId = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                Type = response.Type,
                WorkspaceId = response.WorkspaceId,
                FolderId = response.FolderId,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403") || ex.Message.Contains("Forbidden"))
        {
            return new HttpRequestException("Access denied or feature not available. The workspace must be on a supported Fabric capacity to create pipelines.")
                .ToHttpError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("creating pipeline").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets a specific pipeline by ID from a workspace.")]
    public async Task<string> GetPipelineAsync(
        [Description("The workspace ID containing the pipeline (required)")] string workspaceId,
        [Description("The pipeline ID to retrieve (required)")] string pipelineId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(pipelineId, nameof(pipelineId));

            var pipeline = await _pipelineService.GetPipelineAsync(workspaceId, pipelineId);

            var result = new
            {
                Success = true,
                Pipeline = pipeline.ToFormattedInfo()
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting pipeline").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Adds or updates an activity in a pipeline. If an activity with the same name exists, it will be updated; otherwise, a new activity will be added.")]
    public async Task<string> AddOrUpdateActivityAsync(
        [Description("The workspace ID containing the pipeline (required)")] string workspaceId,
        [Description("The pipeline ID to update (required)")] string pipelineId,
        [Description("The activity name (required)")] string activityName,
        [Description("The activity type (e.g., 'Copy', 'Script', 'Dataflow', 'ExecutePipeline') (required)")] string activityType,
        [Description("The activity description (optional)")] string? activityDescription = null,
        [Description("JSON string of type properties (optional)")] string? typePropertiesJson = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(pipelineId, nameof(pipelineId));
            _validationService.ValidateRequiredString(activityName, nameof(activityName));
            _validationService.ValidateRequiredString(activityType, nameof(activityType));

            var activity = new PipelineActivity
            {
                Name = activityName,
                Type = activityType,
                Description = activityDescription,
                TypeProperties = new Dictionary<string, object>()
            };

            // Parse type properties if provided
            if (!string.IsNullOrEmpty(typePropertiesJson))
            {
                try
                {
                    var typeProps = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(typePropertiesJson);
                    if (typeProps != null)
                    {
                        activity.TypeProperties = typeProps;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    return new ArgumentException("Invalid JSON format for typeProperties")
                        .ToValidationError().ToMcpJson();
                }
            }

            var success = await _pipelineService.AddOrUpdateActivityAsync(workspaceId, pipelineId, activity);

            var result = new
            {
                Success = success,
                Message = success
                    ? $"Activity '{activityName}' of type '{activityType}' added/updated successfully in pipeline {pipelineId}"
                    : $"Failed to add/update activity '{activityName}' in pipeline {pipelineId}",
                PipelineId = pipelineId,
                ActivityName = activityName,
                ActivityType = activityType
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("adding/updating activity").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Runs a pipeline with optional parameters.")]
    public async Task<string> RunPipelineAsync(
        [Description("The workspace ID containing the pipeline (required)")] string workspaceId,
        [Description("The pipeline ID to run (required)")] string pipelineId,
        [Description("JSON string of parameters to pass to the pipeline (optional)")] string? parametersJson = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(pipelineId, nameof(pipelineId));

            var request = new RunPipelineRequest();

            // Parse parameters if provided
            if (!string.IsNullOrEmpty(parametersJson))
            {
                try
                {
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);
                    if (parameters != null)
                    {
                        request.Parameters = parameters;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    return new ArgumentException("Invalid JSON format for parameters")
                        .ToValidationError().ToMcpJson();
                }
            }

            var runResponse = await _pipelineService.RunPipelineAsync(workspaceId, pipelineId, request);

            var result = new
            {
                Success = true,
                Message = $"Pipeline '{pipelineId}' started successfully",
                RunId = runResponse.RunId,
                Status = runResponse.Status,
                PipelineId = pipelineId,
                WorkspaceId = workspaceId,
                StartedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("running pipeline").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the status of a pipeline run.")]
    public async Task<string> GetPipelineRunStatusAsync(
        [Description("The workspace ID containing the pipeline (required)")] string workspaceId,
        [Description("The pipeline ID (required)")] string pipelineId,
        [Description("The run ID to check status for (required)")] string runId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(pipelineId, nameof(pipelineId));
            _validationService.ValidateRequiredString(runId, nameof(runId));

            var status = await _pipelineService.GetPipelineRunStatusAsync(workspaceId, pipelineId, runId);

            var result = new
            {
                Success = true,
                RunStatus = status.ToFormattedInfo()
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting pipeline run status").ToMcpJson();
        }
    }
}

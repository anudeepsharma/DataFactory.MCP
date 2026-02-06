using DataFactory.MCP.Abstractions;
using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Infrastructure.Http;
using DataFactory.MCP.Models.Pipeline;
using Microsoft.Extensions.Logging;

namespace DataFactory.MCP.Services;

/// <summary>
/// Service for interacting with Microsoft Fabric Pipelines API
/// </summary>
public class FabricPipelineService : FabricServiceBase, IFabricPipelineService
{
    public FabricPipelineService(
        IHttpClientFactory httpClientFactory,
        ILogger<FabricPipelineService> logger,
        IValidationService validationService)
        : base(httpClientFactory, logger, validationService)
    {
    }

    public async Task<ListPipelinesResponse> ListPipelinesAsync(
        string workspaceId,
        string? continuationToken = null)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines")
                .BuildEndpoint();
            Logger.LogInformation("Fetching pipelines from workspace {WorkspaceId}", workspaceId);

            var pipelinesResponse = await GetAsync<ListPipelinesResponse>(endpoint, continuationToken);

            Logger.LogInformation("Successfully retrieved {Count} pipelines from workspace {WorkspaceId}",
                pipelinesResponse?.Value?.Count ?? 0, workspaceId);
            return pipelinesResponse ?? new ListPipelinesResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching pipelines from workspace {WorkspaceId}", workspaceId);
            throw;
        }
    }

    public async Task<CreatePipelineResponse> CreatePipelineAsync(
        string workspaceId,
        CreatePipelineRequest request)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)));
            ValidationService.ValidateAndThrow(request, nameof(request));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines")
                .BuildEndpoint();
            Logger.LogInformation("Creating pipeline '{DisplayName}' in workspace {WorkspaceId}",
                request.DisplayName, workspaceId);

            var createResponse = await PostAsync<CreatePipelineResponse>(endpoint, request);

            Logger.LogInformation("Successfully created pipeline '{DisplayName}' with ID {PipelineId} in workspace {WorkspaceId}",
                request.DisplayName, createResponse?.Id, workspaceId);

            return createResponse ?? new CreatePipelineResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating pipeline '{DisplayName}' in workspace {WorkspaceId}",
                request?.DisplayName, workspaceId);
            throw;
        }
    }

    public async Task<Pipeline> GetPipelineAsync(
        string workspaceId,
        string pipelineId)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)), (pipelineId, nameof(pipelineId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines/{pipelineId}")
                .BuildEndpoint();
            Logger.LogInformation("Fetching pipeline {PipelineId} from workspace {WorkspaceId}",
                pipelineId, workspaceId);

            var pipeline = await GetAsync<Pipeline>(endpoint);

            Logger.LogInformation("Successfully retrieved pipeline {PipelineId} from workspace {WorkspaceId}",
                pipelineId, workspaceId);
            return pipeline ?? throw new InvalidOperationException($"Pipeline {pipelineId} not found");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching pipeline {PipelineId} from workspace {WorkspaceId}",
                pipelineId, workspaceId);
            throw;
        }
    }

    public async Task<bool> UpdatePipelineAsync(
        string workspaceId,
        string pipelineId,
        UpdatePipelineRequest request)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)), (pipelineId, nameof(pipelineId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines/{pipelineId}")
                .BuildEndpoint();
            Logger.LogInformation("Updating pipeline {PipelineId} in workspace {WorkspaceId}",
                pipelineId, workspaceId);

            var success = await PostNoContentAsync(endpoint, request);

            if (success)
            {
                Logger.LogInformation("Successfully updated pipeline {PipelineId} in workspace {WorkspaceId}",
                    pipelineId, workspaceId);
            }

            return success;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating pipeline {PipelineId} in workspace {WorkspaceId}",
                pipelineId, workspaceId);
            throw;
        }
    }

    public async Task<bool> AddOrUpdateActivityAsync(
        string workspaceId,
        string pipelineId,
        PipelineActivity activity)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)), (pipelineId, nameof(pipelineId)));
            ValidationService.ValidateRequiredString(activity.Name, nameof(activity.Name));

            // First, get the current pipeline definition
            var pipeline = await GetPipelineAsync(workspaceId, pipelineId);
            
            if (pipeline.Properties?.Definition == null)
            {
                pipeline.Properties = new PipelineProperties
                {
                    Definition = new PipelineDefinition()
                };
            }

            // Find and update or add the activity
            var existingActivity = pipeline.Properties.Definition.Activities
                .FirstOrDefault(a => a.Name == activity.Name);

            if (existingActivity != null)
            {
                // Update existing activity
                var index = pipeline.Properties.Definition.Activities.IndexOf(existingActivity);
                pipeline.Properties.Definition.Activities[index] = activity;
                Logger.LogInformation("Updating existing activity '{ActivityName}' in pipeline {PipelineId}",
                    activity.Name, pipelineId);
            }
            else
            {
                // Add new activity
                pipeline.Properties.Definition.Activities.Add(activity);
                Logger.LogInformation("Adding new activity '{ActivityName}' to pipeline {PipelineId}",
                    activity.Name, pipelineId);
            }

            // Update the pipeline
            var updateRequest = new UpdatePipelineRequest
            {
                Definition = pipeline.Properties.Definition
            };

            return await UpdatePipelineAsync(workspaceId, pipelineId, updateRequest);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding/updating activity '{ActivityName}' in pipeline {PipelineId}",
                activity?.Name, pipelineId);
            throw;
        }
    }

    public async Task<RunPipelineResponse> RunPipelineAsync(
        string workspaceId,
        string pipelineId,
        RunPipelineRequest? request = null)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)), (pipelineId, nameof(pipelineId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines/{pipelineId}/runs")
                .BuildEndpoint();
            Logger.LogInformation("Running pipeline {PipelineId} in workspace {WorkspaceId}",
                pipelineId, workspaceId);

            var runRequest = request ?? new RunPipelineRequest();
            var runResponse = await PostAsync<RunPipelineResponse>(endpoint, runRequest);

            Logger.LogInformation("Successfully started pipeline {PipelineId} with run ID {RunId}",
                pipelineId, runResponse?.RunId);

            return runResponse ?? throw new InvalidOperationException("Failed to start pipeline run");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error running pipeline {PipelineId} in workspace {WorkspaceId}",
                pipelineId, workspaceId);
            throw;
        }
    }

    public async Task<PipelineRunStatus> GetPipelineRunStatusAsync(
        string workspaceId,
        string pipelineId,
        string runId)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)), (pipelineId, nameof(pipelineId)));
            ValidationService.ValidateRequiredString(runId, nameof(runId));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/dataPipelines/{pipelineId}/runs/{runId}")
                .BuildEndpoint();
            Logger.LogInformation("Fetching status for pipeline run {RunId} of pipeline {PipelineId}",
                runId, pipelineId);

            var status = await GetAsync<PipelineRunStatus>(endpoint);

            Logger.LogInformation("Pipeline run {RunId} status: {Status}", runId, status?.Status);

            return status ?? throw new InvalidOperationException($"Pipeline run {runId} not found");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching status for pipeline run {RunId} of pipeline {PipelineId}",
                runId, pipelineId);
            throw;
        }
    }
}

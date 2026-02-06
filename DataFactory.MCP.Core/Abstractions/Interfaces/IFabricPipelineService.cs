using DataFactory.MCP.Models.Pipeline;

namespace DataFactory.MCP.Abstractions.Interfaces;

/// <summary>
/// Service for interacting with Microsoft Fabric Pipelines API
/// </summary>
public interface IFabricPipelineService
{
    /// <summary>
    /// Lists all pipelines from the specified workspace
    /// </summary>
    /// <param name="workspaceId">The workspace ID to list pipelines from</param>
    /// <param name="continuationToken">A token for retrieving the next page of results</param>
    /// <returns>List of pipelines from the workspace</returns>
    Task<ListPipelinesResponse> ListPipelinesAsync(
        string workspaceId,
        string? continuationToken = null);

    /// <summary>
    /// Creates a new pipeline in the specified workspace
    /// </summary>
    /// <param name="workspaceId">The workspace ID where the pipeline will be created</param>
    /// <param name="request">The create pipeline request</param>
    /// <returns>The created pipeline information</returns>
    Task<CreatePipelineResponse> CreatePipelineAsync(
        string workspaceId,
        CreatePipelineRequest request);

    /// <summary>
    /// Gets a pipeline by ID from the specified workspace
    /// </summary>
    /// <param name="workspaceId">The workspace ID containing the pipeline</param>
    /// <param name="pipelineId">The pipeline ID to retrieve</param>
    /// <returns>The pipeline details</returns>
    Task<Pipeline> GetPipelineAsync(
        string workspaceId,
        string pipelineId);

    /// <summary>
    /// Updates a pipeline's definition
    /// </summary>
    /// <param name="workspaceId">The workspace ID containing the pipeline</param>
    /// <param name="pipelineId">The pipeline ID to update</param>
    /// <param name="request">The update request with new definition</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdatePipelineAsync(
        string workspaceId,
        string pipelineId,
        UpdatePipelineRequest request);

    /// <summary>
    /// Adds or updates an activity in a pipeline
    /// </summary>
    /// <param name="workspaceId">The workspace ID containing the pipeline</param>
    /// <param name="pipelineId">The pipeline ID to update</param>
    /// <param name="activity">The activity to add or update</param>
    /// <returns>True if successful</returns>
    Task<bool> AddOrUpdateActivityAsync(
        string workspaceId,
        string pipelineId,
        PipelineActivity activity);

    /// <summary>
    /// Runs a pipeline
    /// </summary>
    /// <param name="workspaceId">The workspace ID containing the pipeline</param>
    /// <param name="pipelineId">The pipeline ID to run</param>
    /// <param name="request">Optional parameters for the run</param>
    /// <returns>The pipeline run response with run ID</returns>
    Task<RunPipelineResponse> RunPipelineAsync(
        string workspaceId,
        string pipelineId,
        RunPipelineRequest? request = null);

    /// <summary>
    /// Gets the status of a pipeline run
    /// </summary>
    /// <param name="workspaceId">The workspace ID containing the pipeline</param>
    /// <param name="pipelineId">The pipeline ID</param>
    /// <param name="runId">The run ID to check</param>
    /// <returns>The pipeline run status</returns>
    Task<PipelineRunStatus> GetPipelineRunStatusAsync(
        string workspaceId,
        string pipelineId,
        string runId);
}

using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Service for generating project kickstart plans
/// </summary>
public interface IProjectKickstartService
{
    /// <summary>
    /// Generates a comprehensive project kickstart plan based on requirements
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="requirements">Project requirements and goals</param>
    /// <param name="constraints">Any constraints or limitations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated project kickstart plan</returns>
    Task<ProjectKickstartPlan> GenerateKickstartPlanAsync(
        string projectName,
        string requirements,
        string? constraints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a plan based on an existing repository structure
    /// </summary>
    /// <param name="repositoryPath">Path to the existing repository</param>
    /// <param name="enhancementGoals">Goals for enhancing the project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated enhancement plan</returns>
    Task<ProjectKickstartPlan> GenerateEnhancementPlanAsync(
        string repositoryPath,
        string enhancementGoals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates project complexity and timeline
    /// </summary>
    /// <param name="requirements">Project requirements</param>
    /// <param name="teamSize">Size of the development team</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Timeline estimation with milestones</returns>
    Task<ProjectKickstartPlan> EstimateProjectTimelineAsync(
        string requirements,
        int teamSize,
        CancellationToken cancellationToken = default);
}
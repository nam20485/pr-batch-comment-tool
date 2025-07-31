using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Service for AI-powered architecture analysis and recommendations
/// </summary>
public interface IArchitectureAnalyzer
{
    /// <summary>
    /// Analyzes the repository structure and provides architecture recommendations
    /// </summary>
    /// <param name="repositoryPath">Path to the repository</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of architecture recommendations</returns>
    Task<IEnumerable<ArchitectureRecommendation>> AnalyzeArchitectureAsync(
        string repositoryPath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explains why a specific architecture pattern or decision is recommended
    /// </summary>
    /// <param name="codeContext">Code context to analyze</param>
    /// <param name="pattern">Architecture pattern to explain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed explanation of the recommendation</returns>
    Task<ArchitectureRecommendation> ExplainRecommendationAsync(
        string codeContext, 
        string pattern, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes code quality and suggests improvements
    /// </summary>
    /// <param name="codeFiles">Dictionary of file paths and their content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Code quality recommendations</returns>
    Task<IEnumerable<ArchitectureRecommendation>> AnalyzeCodeQualityAsync(
        Dictionary<string, string> codeFiles, 
        CancellationToken cancellationToken = default);
}
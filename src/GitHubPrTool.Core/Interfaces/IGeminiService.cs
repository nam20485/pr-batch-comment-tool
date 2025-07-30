namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for Gemini AI service operations
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Generate an explanation or recommendation for repository architecture
    /// </summary>
    /// <param name="repositoryContext">Repository information and context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated explanation and recommendations</returns>
    Task<string> ExplainRepositoryArchitectureAsync(
        string repositoryContext, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a project kickstart plan based on repository analysis
    /// </summary>
    /// <param name="repositoryName">Name of the repository</param>
    /// <param name="repositoryDescription">Description of the repository</param>
    /// <param name="technologies">Technologies used in the project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated project kickstart plan</returns>
    Task<string> GenerateProjectKickstartPlanAsync(
        string repositoryName,
        string repositoryDescription,
        IEnumerable<string> technologies,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Categorize and analyze PR review comments using AI
    /// </summary>
    /// <param name="comments">Collection of review comments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-powered categorization and insights</returns>
    Task<CommentAnalysisResult> AnalyzeCommentsAsync(
        IEnumerable<Models.Comment> comments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate smart comment suggestions based on code context
    /// </summary>
    /// <param name="codeContext">Code context for generating suggestions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated comment suggestions</returns>
    Task<IEnumerable<string>> GenerateCommentSuggestionsAsync(
        string codeContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the Gemini service is available and properly configured
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is available, false otherwise</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of AI-powered comment analysis
/// </summary>
public class CommentAnalysisResult
{
    /// <summary>
    /// Overall summary of the comment analysis
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Categorized comments grouped by type
    /// </summary>
    public Dictionary<string, List<Models.Comment>> CategorizedComments { get; set; } = new();

    /// <summary>
    /// AI-generated insights about the comments
    /// </summary>
    public List<string> Insights { get; set; } = new();

    /// <summary>
    /// Recommended actions based on comment analysis
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();
}
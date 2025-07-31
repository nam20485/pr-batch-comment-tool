using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Service for AI-powered comment analysis and categorization
/// </summary>
public interface ICommentAnalyzer
{
    /// <summary>
    /// Analyzes a comment and categorizes it using AI
    /// </summary>
    /// <param name="comment">Comment to analyze</param>
    /// <param name="context">Additional context (PR description, code diff, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comment category with confidence scores</returns>
    Task<CommentCategory> CategorizeCommentAsync(
        Comment comment,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes multiple comments in batch
    /// </summary>
    /// <param name="comments">Comments to analyze</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of comment IDs to their categories</returns>
    Task<Dictionary<long, CommentCategory>> CategorizeCommentsAsync(
        IEnumerable<Comment> comments,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates insights about a comment
    /// </summary>
    /// <param name="comment">Comment to analyze</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated insights</returns>
    Task<IEnumerable<AIInsight>> GenerateInsightsAsync(
        Comment comment,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes sentiment of a comment
    /// </summary>
    /// <param name="commentText">Text to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sentiment score (-1.0 to 1.0, negative to positive)</returns>
    Task<double> AnalyzeSentimentAsync(
        string commentText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts key topics and themes from comments
    /// </summary>
    /// <param name="comments">Comments to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of topics with their frequency and relevance</returns>
    Task<Dictionary<string, double>> ExtractTopicsAsync(
        IEnumerable<Comment> comments,
        CancellationToken cancellationToken = default);
}
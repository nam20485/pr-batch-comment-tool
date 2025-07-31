using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Service for generating intelligent comment suggestions
/// </summary>
public interface ICommentSuggestionService
{
    /// <summary>
    /// Generates comment suggestions based on code changes
    /// </summary>
    /// <param name="codeChanges">Code changes to analyze</param>
    /// <param name="existingComments">Existing comments for context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of comment suggestions</returns>
    Task<IEnumerable<CommentSuggestion>> GenerateCodeReviewSuggestionsAsync(
        Dictionary<string, string> codeChanges,
        IEnumerable<Comment>? existingComments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests responses to existing comments
    /// </summary>
    /// <param name="originalComment">Comment to respond to</param>
    /// <param name="context">Additional context</param>
    /// <param name="responseType">Type of response (e.g., "acknowledgment", "clarification", "solution")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Suggested response comments</returns>
    Task<IEnumerable<CommentSuggestion>> SuggestResponsesAsync(
        Comment originalComment,
        string? context = null,
        string? responseType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates improvement suggestions for existing comments
    /// </summary>
    /// <param name="comment">Comment to improve</param>
    /// <param name="improvementGoals">Specific goals (e.g., "clarity", "tone", "actionability")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Improved comment suggestions</returns>
    Task<IEnumerable<CommentSuggestion>> SuggestImprovementsAsync(
        Comment comment,
        IEnumerable<string>? improvementGoals = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates contextual suggestions based on PR content
    /// </summary>
    /// <param name="pullRequest">Pull request to analyze</param>
    /// <param name="suggestionType">Type of suggestions to generate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Contextual comment suggestions</returns>
    Task<IEnumerable<CommentSuggestion>> GenerateContextualSuggestionsAsync(
        PullRequest pullRequest,
        string? suggestionType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides feedback on comment effectiveness
    /// </summary>
    /// <param name="suggestion">Comment suggestion that was used</param>
    /// <param name="feedback">User feedback on the suggestion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the feedback operation</returns>
    Task ProvideFeedbackAsync(
        CommentSuggestion suggestion,
        string feedback,
        CancellationToken cancellationToken = default);
}
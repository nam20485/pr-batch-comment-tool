namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents an AI-generated comment suggestion
/// </summary>
public class CommentSuggestion
{
    /// <summary>
    /// Unique identifier for the suggestion
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Suggested comment text
    /// </summary>
    public string SuggestedText { get; set; } = string.Empty;

    /// <summary>
    /// Context that led to this suggestion
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Type of suggestion (e.g., "improvement", "question", "praise", "concern")
    /// </summary>
    public string SuggestionType { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score from 0.0 to 1.0
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Relevance score from 0.0 to 1.0
    /// </summary>
    public double Relevance { get; set; }

    /// <summary>
    /// Tone of the suggestion (e.g., "constructive", "neutral", "appreciative")
    /// </summary>
    public string Tone { get; set; } = string.Empty;

    /// <summary>
    /// Target file path (if applicable for code review comments)
    /// </summary>
    public string? TargetFile { get; set; }

    /// <summary>
    /// Target line number (if applicable for code review comments)
    /// </summary>
    public int? TargetLine { get; set; }

    /// <summary>
    /// Related code snippet (if applicable)
    /// </summary>
    public string? CodeSnippet { get; set; }

    /// <summary>
    /// Suggested improvements or alternatives
    /// </summary>
    public List<string> Alternatives { get; set; } = new();

    /// <summary>
    /// Tags associated with this suggestion
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether this suggestion has been used
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// User feedback on this suggestion (if any)
    /// </summary>
    public string? UserFeedback { get; set; }

    /// <summary>
    /// When this suggestion was generated
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The AI model/version that generated this suggestion
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;
}
namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents AI-generated insights about comments or code
/// </summary>
public class AIInsight
{
    /// <summary>
    /// Unique identifier for the insight
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of insight (e.g., "sentiment", "complexity", "suggestion")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The main insight content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score from 0.0 to 1.0
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// When this insight was generated
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The AI model/version that generated this insight
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;
}
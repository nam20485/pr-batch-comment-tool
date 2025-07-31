namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents an architecture analysis recommendation
/// </summary>
public class ArchitectureRecommendation
{
    /// <summary>
    /// Unique identifier for the recommendation
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Title of the recommendation
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the recommendation
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category of the recommendation (e.g., "Performance", "Security", "Maintainability")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (1-5, with 5 being highest priority)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Estimated impact of implementing this recommendation
    /// </summary>
    public string Impact { get; set; } = string.Empty;

    /// <summary>
    /// Estimated effort required to implement
    /// </summary>
    public string Effort { get; set; } = string.Empty;

    /// <summary>
    /// Specific code files or areas this recommendation applies to
    /// </summary>
    public List<string> AffectedFiles { get; set; } = new();

    /// <summary>
    /// Step-by-step implementation guide
    /// </summary>
    public List<string> ImplementationSteps { get; set; } = new();

    /// <summary>
    /// Related patterns or best practices
    /// </summary>
    public List<string> RelatedPatterns { get; set; } = new();

    /// <summary>
    /// Code examples or snippets
    /// </summary>
    public string? CodeExample { get; set; }

    /// <summary>
    /// References to documentation or resources
    /// </summary>
    public List<string> References { get; set; } = new();

    /// <summary>
    /// When this recommendation was generated
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The AI model/version that generated this recommendation
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;
}
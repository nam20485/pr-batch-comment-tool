namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents a category assigned to a comment by AI
/// </summary>
public enum CommentCategoryType
{
    /// <summary>
    /// Bug report or issue identification
    /// </summary>
    Bug,
    
    /// <summary>
    /// Feature request or enhancement
    /// </summary>
    Feature,
    
    /// <summary>
    /// Code review feedback
    /// </summary>
    CodeReview,
    
    /// <summary>
    /// Documentation related
    /// </summary>
    Documentation,
    
    /// <summary>
    /// Question or clarification request
    /// </summary>
    Question,
    
    /// <summary>
    /// Performance related comment
    /// </summary>
    Performance,
    
    /// <summary>
    /// Security related comment
    /// </summary>
    Security,
    
    /// <summary>
    /// Testing related comment
    /// </summary>
    Testing,
    
    /// <summary>
    /// General discussion or other
    /// </summary>
    General
}

/// <summary>
/// Represents AI-generated categorization of a comment
/// </summary>
public class CommentCategory
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Primary category type
    /// </summary>
    public CommentCategoryType Category { get; set; }

    /// <summary>
    /// Sub-category or more specific classification
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Confidence score from 0.0 to 1.0
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Priority level (1-5, with 5 being highest)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Sentiment analysis result (-1.0 to 1.0, negative to positive)
    /// </summary>
    public double Sentiment { get; set; }

    /// <summary>
    /// Estimated complexity (1-5, with 5 being most complex)
    /// </summary>
    public int Complexity { get; set; } = 3;

    /// <summary>
    /// Tags associated with this comment
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// When this categorization was generated
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The AI model/version that generated this categorization
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;
}
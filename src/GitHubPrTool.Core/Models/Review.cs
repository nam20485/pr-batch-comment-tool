namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents the state of a review
/// </summary>
public enum ReviewState
{
    /// <summary>
    /// Review is pending (not yet submitted)
    /// </summary>
    Pending,
    
    /// <summary>
    /// Review approved the changes
    /// </summary>
    Approved,
    
    /// <summary>
    /// Review requested changes
    /// </summary>
    ChangesRequested,
    
    /// <summary>
    /// Review provided comments without approval or rejection
    /// </summary>
    Commented,
    
    /// <summary>
    /// Review was dismissed
    /// </summary>
    Dismissed
}

/// <summary>
/// Represents a GitHub pull request review
/// </summary>
public class Review
{
    /// <summary>
    /// GitHub review ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Review body/summary
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Current state of the review
    /// </summary>
    public ReviewState State { get; set; }

    /// <summary>
    /// Author of the review
    /// </summary>
    public User Author { get; set; } = new();

    /// <summary>
    /// Pull request this review belongs to
    /// </summary>
    public PullRequest PullRequest { get; set; } = new();

    /// <summary>
    /// Foreign key to pull request
    /// </summary>
    public long PullRequestId { get; set; }

    /// <summary>
    /// URL to the review on GitHub
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// URL to the pull request diff for this review
    /// </summary>
    public string? PullRequestUrl { get; set; }

    /// <summary>
    /// Commit SHA that this review was based on
    /// </summary>
    public string? CommitId { get; set; }

    /// <summary>
    /// Date when the review was submitted
    /// </summary>
    public DateTimeOffset? SubmittedAt { get; set; }

    /// <summary>
    /// Date when the review was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date when the review was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Collection of comments associated with this review
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
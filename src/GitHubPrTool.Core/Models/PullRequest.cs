namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents the state of a pull request
/// </summary>
public enum PullRequestState
{
    Open,
    Closed,
    Merged
}

/// <summary>
/// Represents a GitHub pull request
/// </summary>
public class PullRequest
{
    /// <summary>
    /// GitHub pull request ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Pull request number
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Pull request title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Pull request body/description
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Current state of the pull request
    /// </summary>
    public PullRequestState State { get; set; }

    /// <summary>
    /// URL to the pull request on GitHub
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// Author of the pull request
    /// </summary>
    public User Author { get; set; } = new();

    /// <summary>
    /// Repository this pull request belongs to
    /// </summary>
    public Repository Repository { get; set; } = new();

    /// <summary>
    /// Foreign key to repository
    /// </summary>
    public long RepositoryId { get; set; }

    /// <summary>
    /// Base branch (target branch)
    /// </summary>
    public string BaseBranch { get; set; } = string.Empty;

    /// <summary>
    /// Head branch (source branch)
    /// </summary>
    public string HeadBranch { get; set; } = string.Empty;

    /// <summary>
    /// Whether the pull request is draft
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Whether the pull request is mergeable
    /// </summary>
    public bool? Mergeable { get; set; }

    /// <summary>
    /// Number of commits in this pull request
    /// </summary>
    public int Commits { get; set; }

    /// <summary>
    /// Number of additions in this pull request
    /// </summary>
    public int Additions { get; set; }

    /// <summary>
    /// Number of deletions in this pull request
    /// </summary>
    public int Deletions { get; set; }

    /// <summary>
    /// Number of changed files in this pull request
    /// </summary>
    public int ChangedFiles { get; set; }

    /// <summary>
    /// Date when the pull request was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date when the pull request was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Date when the pull request was closed (if applicable)
    /// </summary>
    public DateTimeOffset? ClosedAt { get; set; }

    /// <summary>
    /// Date when the pull request was merged (if applicable)
    /// </summary>
    public DateTimeOffset? MergedAt { get; set; }

    /// <summary>
    /// User who merged the pull request (if applicable)
    /// </summary>
    public User? MergedBy { get; set; }

    /// <summary>
    /// Collection of reviews for this pull request
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>
    /// Collection of comments for this pull request
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
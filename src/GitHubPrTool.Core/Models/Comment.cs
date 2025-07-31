namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents the type of comment
/// </summary>
public enum CommentType
{
    /// <summary>
    /// General comment on the pull request
    /// </summary>
    Issue,
    
    /// <summary>
    /// Review comment on specific lines of code
    /// </summary>
    Review,
    
    /// <summary>
    /// Commit comment
    /// </summary>
    Commit
}

/// <summary>
/// Represents a comment on a GitHub pull request
/// </summary>
public class Comment
{
    /// <summary>
    /// GitHub comment ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Comment body/content
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Type of comment
    /// </summary>
    public CommentType Type { get; set; }

    /// <summary>
    /// Author of the comment
    /// </summary>
    public User Author { get; set; } = new();

    /// <summary>
    /// Pull request this comment belongs to
    /// </summary>
    public PullRequest PullRequest { get; set; } = new();

    /// <summary>
    /// Foreign key to pull request
    /// </summary>
    public long PullRequestId { get; set; }

    /// <summary>
    /// URL to the comment on GitHub
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// File path for review comments (if applicable)
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Line number for review comments (if applicable)
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    /// Original line number for review comments (if applicable)
    /// </summary>
    public int? OriginalLine { get; set; }

    /// <summary>
    /// Commit SHA for review comments (if applicable)
    /// </summary>
    public string? CommitId { get; set; }

    /// <summary>
    /// Original commit SHA for review comments (if applicable)
    /// </summary>
    public string? OriginalCommitId { get; set; }

    /// <summary>
    /// Diff hunk for review comments (if applicable)
    /// </summary>
    public string? DiffHunk { get; set; }

    /// <summary>
    /// Position in the diff for review comments (if applicable)
    /// </summary>
    public int? Position { get; set; }

    /// <summary>
    /// Original position in the diff for review comments (if applicable)
    /// </summary>
    public int? OriginalPosition { get; set; }

    /// <summary>
    /// Whether this comment is part of a multi-line review comment
    /// </summary>
    public bool IsMultiLine { get; set; }

    /// <summary>
    /// Start line for multi-line review comments (if applicable)
    /// </summary>
    public int? StartLine { get; set; }

    /// <summary>
    /// Start side for multi-line review comments (if applicable)
    /// </summary>
    public string? StartSide { get; set; }

    /// <summary>
    /// Side of the diff (LEFT or RIGHT) for review comments
    /// </summary>
    public string? Side { get; set; }

    /// <summary>
    /// Review this comment belongs to (if applicable)
    /// </summary>
    public Review? Review { get; set; }

    /// <summary>
    /// Foreign key to review (if applicable)
    /// </summary>
    public long? ReviewId { get; set; }

    /// <summary>
    /// Comment this is a reply to (if applicable)
    /// </summary>
    public Comment? InReplyTo { get; set; }

    /// <summary>
    /// Foreign key to parent comment (if applicable)
    /// </summary>
    public long? InReplyToId { get; set; }

    /// <summary>
    /// Date when the comment was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date when the comment was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Replies to this comment
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    /// <summary>
    /// AI-generated category for this comment
    /// </summary>
    public CommentCategory? AICategory { get; set; }

    /// <summary>
    /// AI-generated insights about this comment
    /// </summary>
    public ICollection<AIInsight> AIInsights { get; set; } = new List<AIInsight>();
}
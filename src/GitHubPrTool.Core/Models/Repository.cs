namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents a GitHub repository
/// </summary>
public class Repository
{
    /// <summary>
    /// GitHub repository ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Repository name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full repository name (owner/repo)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Repository description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL to the repository on GitHub
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// Git clone URL
    /// </summary>
    public string? CloneUrl { get; set; }

    /// <summary>
    /// Repository owner
    /// </summary>
    public User Owner { get; set; } = new();

    /// <summary>
    /// Whether the repository is private
    /// </summary>
    public bool Private { get; set; }

    /// <summary>
    /// Default branch name
    /// </summary>
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// Programming language
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Number of stars
    /// </summary>
    public int StargazersCount { get; set; }

    /// <summary>
    /// Number of forks
    /// </summary>
    public int ForksCount { get; set; }

    /// <summary>
    /// Number of open issues
    /// </summary>
    public int OpenIssuesCount { get; set; }

    /// <summary>
    /// Date when the repository was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date when the repository was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Date when the repository was last pushed to
    /// </summary>
    public DateTimeOffset? PushedAt { get; set; }

    /// <summary>
    /// Collection of pull requests for this repository
    /// </summary>
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
}
namespace GitHubPrTool.Core.Models;

/// <summary>
/// Represents a GitHub user
/// </summary>
public class User
{
    /// <summary>
    /// GitHub user ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// GitHub username
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// URL to the user's avatar image
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// URL to the user's GitHub profile
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// User's bio/description
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Date when the user joined GitHub
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Date when the user last updated their profile
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
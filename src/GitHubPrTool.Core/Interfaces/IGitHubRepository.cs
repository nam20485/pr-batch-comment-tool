using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for GitHub repository operations
/// </summary>
public interface IGitHubRepository
{
    /// <summary>
    /// Get all repositories accessible to the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific repository by owner and name
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository if found, null otherwise</returns>
    Task<Repository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search repositories by query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching repositories</returns>
    Task<IEnumerable<Repository>> SearchRepositoriesAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pull requests for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="state">Pull request state filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pull requests</returns>
    Task<IEnumerable<PullRequest>> GetPullRequestsAsync(long repositoryId, PullRequestState? state = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific pull request
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="number">Pull request number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pull request if found, null otherwise</returns>
    Task<PullRequest?> GetPullRequestAsync(long repositoryId, int number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comments for a pull request
    /// </summary>
    /// <param name="pullRequestId">Pull request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of comments</returns>
    Task<IEnumerable<Comment>> GetCommentsAsync(long pullRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get reviews for a pull request
    /// </summary>
    /// <param name="pullRequestId">Pull request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of reviews</returns>
    Task<IEnumerable<Review>> GetReviewsAsync(long pullRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new review with comments
    /// </summary>
    /// <param name="pullRequestId">Pull request ID</param>
    /// <param name="body">Review body</param>
    /// <param name="comments">Comments to include in the review</param>
    /// <param name="state">Review state</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review</returns>
    Task<Review> CreateReviewAsync(long pullRequestId, string body, IEnumerable<Comment> comments, ReviewState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Duplicate comments to a new review
    /// </summary>
    /// <param name="sourceComments">Comments to duplicate</param>
    /// <param name="targetPullRequestId">Target pull request ID</param>
    /// <param name="reviewBody">Review body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review with duplicated comments</returns>
    Task<Review> DuplicateCommentsAsync(IEnumerable<Comment> sourceComments, long targetPullRequestId, string reviewBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or update a repository in the local cache
    /// </summary>
    /// <param name="repository">Repository to add/update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddRepositoryAsync(Repository repository, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a repository by ID
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository if found, null otherwise</returns>
    Task<Repository?> GetRepositoryByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or update a pull request in the local cache
    /// </summary>
    /// <param name="pullRequest">Pull request to add/update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddPullRequestAsync(PullRequest pullRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or update a comment in the local cache
    /// </summary>
    /// <param name="comment">Comment to add/update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the last sync timestamp for an operation
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="timestamp">Sync timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateLastSyncAsync(string operation, DateTimeOffset timestamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the last sync timestamp for an operation
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Last sync timestamp if available</returns>
    Task<DateTimeOffset?> GetLastSyncAsync(string operation, CancellationToken cancellationToken = default);
}
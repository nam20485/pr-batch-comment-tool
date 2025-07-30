using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for global search functionality across all data types
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Search across all repositories, pull requests, and comments
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="searchOptions">Search options and filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<SearchResults> SearchAsync(string query, SearchOptions? searchOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search repositories only
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching repositories</returns>
    Task<IEnumerable<Repository>> SearchRepositoriesAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search pull requests only
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="repositoryId">Optional repository ID to limit search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching pull requests</returns>
    Task<IEnumerable<PullRequest>> SearchPullRequestsAsync(string query, long? repositoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search comments only
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="repositoryId">Optional repository ID to limit search</param>
    /// <param name="pullRequestId">Optional pull request ID to limit search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching comments</returns>
    Task<IEnumerable<Comment>> SearchCommentsAsync(string query, long? repositoryId = null, long? pullRequestId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get search suggestions based on partial query
    /// </summary>
    /// <param name="partialQuery">Partial search query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search suggestions</returns>
    Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxSuggestions = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Search options for filtering and configuring search behavior
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Include repositories in search results
    /// </summary>
    public bool IncludeRepositories { get; set; } = true;

    /// <summary>
    /// Include pull requests in search results
    /// </summary>
    public bool IncludePullRequests { get; set; } = true;

    /// <summary>
    /// Include comments in search results
    /// </summary>
    public bool IncludeComments { get; set; } = true;

    /// <summary>
    /// Maximum number of results per category
    /// </summary>
    public int MaxResultsPerCategory { get; set; } = 50;

    /// <summary>
    /// Case sensitive search
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Use exact match instead of partial matching
    /// </summary>
    public bool ExactMatch { get; set; } = false;

    /// <summary>
    /// Repository ID to limit search scope (optional)
    /// </summary>
    public long? RepositoryId { get; set; }

    /// <summary>
    /// Date range filter - from date
    /// </summary>
    public DateTimeOffset? DateFrom { get; set; }

    /// <summary>
    /// Date range filter - to date
    /// </summary>
    public DateTimeOffset? DateTo { get; set; }

    /// <summary>
    /// Filter by specific authors
    /// </summary>
    public List<string> Authors { get; set; } = new();
}

/// <summary>
/// Combined search results across all data types
/// </summary>
public class SearchResults
{
    /// <summary>
    /// Matching repositories
    /// </summary>
    public IEnumerable<Repository> Repositories { get; set; } = new List<Repository>();

    /// <summary>
    /// Matching pull requests
    /// </summary>
    public IEnumerable<PullRequest> PullRequests { get; set; } = new List<PullRequest>();

    /// <summary>
    /// Matching comments
    /// </summary>
    public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Total number of results
    /// </summary>
    public int TotalResults => Repositories.Count() + PullRequests.Count() + Comments.Count();

    /// <summary>
    /// Search query that generated these results
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Search execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Whether the search was truncated due to too many results
    /// </summary>
    public bool IsTruncated { get; set; }
}
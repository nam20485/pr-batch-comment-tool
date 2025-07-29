using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Services;

/// <summary>
/// Service for comment-related operations
/// </summary>
public class CommentService
{
    private readonly IGitHubRepository _gitHubRepository;
    private readonly ICacheService _cacheService;

    public CommentService(IGitHubRepository gitHubRepository, ICacheService cacheService)
    {
        _gitHubRepository = gitHubRepository ?? throw new ArgumentNullException(nameof(gitHubRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Filter comments by various criteria
    /// </summary>
    /// <param name="comments">Comments to filter</param>
    /// <param name="filters">Filter criteria</param>
    /// <returns>Filtered comments</returns>
    public IEnumerable<Comment> FilterComments(IEnumerable<Comment> comments, CommentFilter filters)
    {
        ArgumentNullException.ThrowIfNull(comments);
        ArgumentNullException.ThrowIfNull(filters);

        var query = comments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.AuthorLogin))
        {
            query = query.Where(c => c.Author.Login.Contains(filters.AuthorLogin, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.BodyContains))
        {
            query = query.Where(c => c.Body.Contains(filters.BodyContains, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.Type.HasValue)
        {
            query = query.Where(c => c.Type == filters.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.FilePath))
        {
            query = query.Where(c => c.Path != null && c.Path.Contains(filters.FilePath, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.CreatedAfter.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filters.CreatedAfter.Value);
        }

        if (filters.CreatedBefore.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filters.CreatedBefore.Value);
        }

        return query.ToList();
    }

    /// <summary>
    /// Sort comments by specified criteria
    /// </summary>
    /// <param name="comments">Comments to sort</param>
    /// <param name="sortBy">Sort criteria</param>
    /// <param name="descending">Whether to sort in descending order</param>
    /// <returns>Sorted comments</returns>
    public IEnumerable<Comment> SortComments(IEnumerable<Comment> comments, CommentSortBy sortBy, bool descending = false)
    {
        ArgumentNullException.ThrowIfNull(comments);

        var query = comments.AsQueryable();

        query = sortBy switch
        {
            CommentSortBy.CreatedAt => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            CommentSortBy.UpdatedAt => descending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            CommentSortBy.Author => descending ? query.OrderByDescending(c => c.Author.Login) : query.OrderBy(c => c.Author.Login),
            CommentSortBy.Type => descending ? query.OrderByDescending(c => c.Type) : query.OrderBy(c => c.Type),
            CommentSortBy.FilePath => descending ? query.OrderByDescending(c => c.Path ?? string.Empty) : query.OrderBy(c => c.Path ?? string.Empty),
            CommentSortBy.Line => descending ? query.OrderByDescending(c => c.Line ?? 0) : query.OrderBy(c => c.Line ?? 0),
            _ => query
        };

        return query.ToList();
    }

    /// <summary>
    /// Duplicate selected comments to a new review
    /// </summary>
    /// <param name="selectedComments">Comments to duplicate</param>
    /// <param name="targetPullRequestId">Target pull request ID</param>
    /// <param name="reviewBody">Review summary body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review with duplicated comments</returns>
    public async Task<Review> DuplicateCommentsAsync(
        IEnumerable<Comment> selectedComments, 
        long targetPullRequestId, 
        string reviewBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selectedComments);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewBody);

        var commentsToduplicate = selectedComments.ToList();
        if (!commentsToduplicate.Any())
        {
            throw new ArgumentException("At least one comment must be selected for duplication.", nameof(selectedComments));
        }

        // Clear cache for the target pull request to ensure fresh data
        await _cacheService.RemoveByPatternAsync($"pr-{targetPullRequestId}*", cancellationToken);

        return await _gitHubRepository.DuplicateCommentsAsync(commentsToduplicate, targetPullRequestId, reviewBody, cancellationToken);
    }
}

/// <summary>
/// Filter criteria for comments
/// </summary>
public class CommentFilter
{
    /// <summary>
    /// Filter by author login name
    /// </summary>
    public string? AuthorLogin { get; set; }

    /// <summary>
    /// Filter comments containing specific text
    /// </summary>
    public string? BodyContains { get; set; }

    /// <summary>
    /// Filter by comment type
    /// </summary>
    public CommentType? Type { get; set; }

    /// <summary>
    /// Filter by file path (for review comments)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Filter comments created after this date
    /// </summary>
    public DateTimeOffset? CreatedAfter { get; set; }

    /// <summary>
    /// Filter comments created before this date
    /// </summary>
    public DateTimeOffset? CreatedBefore { get; set; }
}

/// <summary>
/// Sort criteria for comments
/// </summary>
public enum CommentSortBy
{
    CreatedAt,
    UpdatedAt,
    Author,
    Type,
    FilePath,
    Line
}
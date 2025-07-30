using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Collections.Concurrent;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// GitHub repository service implementation using Octokit.net
/// </summary>
public class GitHubRepositoryService : IGitHubRepository
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GitHubRepositoryService> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;

    public GitHubRepositoryService(
        IGitHubClient gitHubClient,
        ICacheService cacheService,
        ILogger<GitHubRepositoryService> logger)
    {
        _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rateLimitSemaphore = new SemaphoreSlim(10, 10); // Limit concurrent API calls
    }

    public async Task<IEnumerable<Core.Models.Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "user_repositories";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Core.Models.Repository>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved {Count} repositories from cache", cached.Count);
                return cached;
            }

            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Fetching user repositories from GitHub API");

                var request = new RepositoryRequest
                {
                    Affiliation = RepositoryAffiliation.Owner | RepositoryAffiliation.Collaborator,
                    Sort = RepositorySort.Updated,
                    Direction = SortDirection.Descending
                };

                var octokitRepos = await _gitHubClient.Repository.GetAllForCurrent(request);
                var repositories = octokitRepos.Select(MapToRepository).ToList();

                // Cache for 10 minutes
                await _cacheService.SetAsync(cacheKey, repositories, TimeSpan.FromMinutes(10), cancellationToken);

                _logger.LogInformation("Retrieved {Count} repositories from GitHub API", repositories.Count);
                return repositories;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repositories");
            throw;
        }
    }

    public async Task<Core.Models.Repository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var cacheKey = $"repo_{owner}_{name}";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<Core.Models.Repository>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved repository {Owner}/{Name} from cache", owner, name);
                return cached;
            }

            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Fetching repository {Owner}/{Name} from GitHub API", owner, name);

                var octokitRepo = await _gitHubClient.Repository.Get(owner, name);
                var repository = MapToRepository(octokitRepo);

                // Cache for 5 minutes
                await _cacheService.SetAsync(cacheKey, repository, TimeSpan.FromMinutes(5), cancellationToken);

                _logger.LogDebug("Retrieved repository {Owner}/{Name} from GitHub API", owner, name);
                return repository;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Repository {Owner}/{Name} not found", owner, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repository {Owner}/{Name}", owner, name);
            throw;
        }
    }

    public async Task<IEnumerable<Core.Models.Repository>> SearchRepositoriesAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var cacheKey = $"search_repos_{query.GetHashCode()}";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Core.Models.Repository>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved search results for '{Query}' from cache", query);
                return cached;
            }

            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Searching repositories for query: {Query}", query);

                var searchRequest = new SearchRepositoriesRequest(query);
                searchRequest.SortField = RepoSearchSort.Updated;
                searchRequest.Order = SortDirection.Descending;

                var searchResult = await _gitHubClient.Search.SearchRepo(searchRequest);
                var repositories = searchResult.Items.Select(MapToRepository).ToList();

                // Cache for 2 minutes
                await _cacheService.SetAsync(cacheKey, repositories, TimeSpan.FromMinutes(2), cancellationToken);

                _logger.LogInformation("Found {Count} repositories for query: {Query}", repositories.Count, query);
                return repositories;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repositories for query: {Query}", query);
            throw;
        }
    }

    public async Task<IEnumerable<Core.Models.PullRequest>> GetPullRequestsAsync(long repositoryId, PullRequestState? state = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"repo_{repositoryId}_prs_{state?.ToString() ?? "all"}";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Core.Models.PullRequest>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved pull requests for repository {RepositoryId} from cache", repositoryId);
                return cached;
            }

            // Need to get repository info first to construct owner/name
            var repo = await GetRepositoryByIdAsync(repositoryId, cancellationToken);
            if (repo == null)
            {
                _logger.LogWarning("Repository {RepositoryId} not found", repositoryId);
                return Enumerable.Empty<Core.Models.PullRequest>();
            }

            var ownerName = repo.FullName.Split('/');
            if (ownerName.Length != 2)
            {
                _logger.LogError("Invalid repository full name format: {FullName}", repo.FullName);
                return Enumerable.Empty<Core.Models.PullRequest>();
            }

            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Fetching pull requests for repository {Owner}/{Name}", ownerName[0], ownerName[1]);

                var request = new PullRequestRequest
                {
                    State = state switch
                    {
                        PullRequestState.Open => ItemStateFilter.Open,
                        PullRequestState.Closed => ItemStateFilter.Closed,
                        _ => ItemStateFilter.All
                    }
                };

                var octokitPrs = await _gitHubClient.PullRequest.GetAllForRepository(ownerName[0], ownerName[1], request);
                var pullRequests = octokitPrs.Select(pr => MapToPullRequest(pr, repo)).ToList();

                // Cache for 5 minutes
                await _cacheService.SetAsync(cacheKey, pullRequests, TimeSpan.FromMinutes(5), cancellationToken);

                _logger.LogInformation("Retrieved {Count} pull requests for repository {Owner}/{Name}", pullRequests.Count, ownerName[0], ownerName[1]);
                return pullRequests;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pull requests for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    public async Task<Core.Models.PullRequest?> GetPullRequestAsync(long repositoryId, int number, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"repo_{repositoryId}_pr_{number}";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<Core.Models.PullRequest>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved pull request #{Number} for repository {RepositoryId} from cache", number, repositoryId);
                return cached;
            }

            // Need to get repository info first
            var repo = await GetRepositoryByIdAsync(repositoryId, cancellationToken);
            if (repo == null)
            {
                return null;
            }

            var ownerName = repo.FullName.Split('/');
            if (ownerName.Length != 2)
            {
                return null;
            }

            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Fetching pull request #{Number} for repository {Owner}/{Name}", number, ownerName[0], ownerName[1]);

                var octokitPr = await _gitHubClient.PullRequest.Get(ownerName[0], ownerName[1], number);
                var pullRequest = MapToPullRequest(octokitPr, repo);

                // Cache for 2 minutes
                await _cacheService.SetAsync(cacheKey, pullRequest, TimeSpan.FromMinutes(2), cancellationToken);

                _logger.LogDebug("Retrieved pull request #{Number} for repository {Owner}/{Name}", number, ownerName[0], ownerName[1]);
                return pullRequest;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Pull request #{Number} not found for repository {RepositoryId}", number, repositoryId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pull request #{Number} for repository {RepositoryId}", number, repositoryId);
            throw;
        }
    }

    public async Task<IEnumerable<Core.Models.Comment>> GetCommentsAsync(long pullRequestId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"pr_{pullRequestId}_comments";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Core.Models.Comment>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved comments for pull request {PullRequestId} from cache", pullRequestId);
                return cached;
            }

            // Implementation would require mapping PR ID to owner/name/number
            // For now, return empty list - this would be implemented with proper ID mapping
            _logger.LogWarning("Comment retrieval not yet implemented for pull request {PullRequestId}", pullRequestId);
            return Enumerable.Empty<Core.Models.Comment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for pull request {PullRequestId}", pullRequestId);
            throw;
        }
    }

    public async Task<IEnumerable<Core.Models.Review>> GetReviewsAsync(long pullRequestId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"pr_{pullRequestId}_reviews";

        try
        {
            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Core.Models.Review>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved reviews for pull request {PullRequestId} from cache", pullRequestId);
                return cached;
            }

            // Implementation would require mapping PR ID to owner/name/number
            // For now, return empty list - this would be implemented with proper ID mapping
            _logger.LogWarning("Review retrieval not yet implemented for pull request {PullRequestId}", pullRequestId);
            return Enumerable.Empty<Core.Models.Review>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for pull request {PullRequestId}", pullRequestId);
            throw;
        }
    }

    public Task<Core.Models.Review> CreateReviewAsync(long pullRequestId, string body, IEnumerable<Core.Models.Comment> comments, ReviewState state, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentNullException.ThrowIfNull(comments);

        try
        {
            _logger.LogInformation("Creating review for pull request {PullRequestId}", pullRequestId);

            // Implementation would require mapping PR ID to owner/name/number and creating review
            // For now, throw not implemented
            throw new NotImplementedException("Review creation not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for pull request {PullRequestId}", pullRequestId);
            throw;
        }
    }

    public Task<Core.Models.Review> DuplicateCommentsAsync(IEnumerable<Core.Models.Comment> sourceComments, long targetPullRequestId, string reviewBody, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceComments);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewBody);

        try
        {
            _logger.LogInformation("Duplicating {Count} comments to pull request {PullRequestId}", sourceComments.Count(), targetPullRequestId);

            // Implementation would create a new review with duplicated comments
            // For now, throw not implemented
            throw new NotImplementedException("Comment duplication not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating comments to pull request {PullRequestId}", targetPullRequestId);
            throw;
        }
    }

    public async Task<Core.Models.Repository?> GetRepositoryByIdAsync(long repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting repository by ID {Id}", repositoryId);
            
            // Try cache first
            var cached = await _cacheService.GetAsync<Core.Models.Repository>($"repository_{repositoryId}", cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Retrieved repository {Name} from cache", cached.Name);
                return cached;
            }

            // If not in cache, try to find in repositories list
            var repositories = await GetRepositoriesAsync(cancellationToken);
            var repository = repositories.FirstOrDefault(r => r.Id == repositoryId);
            
            if (repository != null)
            {
                // Cache individual repository
                await _cacheService.SetAsync($"repository_{repositoryId}", repository, TimeSpan.FromHours(1), cancellationToken);
            }
            
            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository by ID {Id}", repositoryId);
            throw;
        }
    }

    private static Core.Models.Repository MapToRepository(Octokit.Repository octokitRepo)
    {
        return new Core.Models.Repository
        {
            Id = octokitRepo.Id,
            Name = octokitRepo.Name,
            FullName = octokitRepo.FullName,
            Description = octokitRepo.Description,
            HtmlUrl = octokitRepo.HtmlUrl,
            CloneUrl = octokitRepo.CloneUrl,
            Owner = new Core.Models.User
            {
                Id = octokitRepo.Owner.Id,
                Login = octokitRepo.Owner.Login,
                AvatarUrl = octokitRepo.Owner.AvatarUrl,
                HtmlUrl = octokitRepo.Owner.HtmlUrl
            },
            Private = octokitRepo.Private,
            DefaultBranch = octokitRepo.DefaultBranch,
            Language = octokitRepo.Language,
            StargazersCount = octokitRepo.StargazersCount,
            ForksCount = octokitRepo.ForksCount,
            OpenIssuesCount = octokitRepo.OpenIssuesCount,
            CreatedAt = octokitRepo.CreatedAt,
            UpdatedAt = octokitRepo.UpdatedAt,
            PushedAt = octokitRepo.PushedAt
        };
    }

    private static Core.Models.PullRequest MapToPullRequest(Octokit.PullRequest octokitPr, Core.Models.Repository repository)
    {
        return new Core.Models.PullRequest
        {
            Id = octokitPr.Id,
            Number = octokitPr.Number,
            Title = octokitPr.Title,
            Body = octokitPr.Body,
            State = octokitPr.State.Value switch
            {
                ItemState.Open => PullRequestState.Open,
                ItemState.Closed => octokitPr.Merged ? PullRequestState.Merged : PullRequestState.Closed,
                _ => PullRequestState.Closed
            },
            HtmlUrl = octokitPr.HtmlUrl,
            Author = new Core.Models.User
            {
                Id = octokitPr.User.Id,
                Login = octokitPr.User.Login,
                AvatarUrl = octokitPr.User.AvatarUrl,
                HtmlUrl = octokitPr.User.HtmlUrl
            },
            Repository = repository,
            RepositoryId = repository.Id,
            BaseBranch = octokitPr.Base.Ref,
            HeadBranch = octokitPr.Head.Ref,
            IsDraft = octokitPr.Draft,
            Mergeable = octokitPr.Mergeable,
            Commits = octokitPr.Commits,
            Additions = octokitPr.Additions,
            Deletions = octokitPr.Deletions,
            ChangedFiles = octokitPr.ChangedFiles,
            CreatedAt = octokitPr.CreatedAt,
            UpdatedAt = octokitPr.UpdatedAt,
            ClosedAt = octokitPr.ClosedAt,
            MergedAt = octokitPr.MergedAt,
            MergedBy = octokitPr.MergedBy != null ? new Core.Models.User
            {
                Id = octokitPr.MergedBy.Id,
                Login = octokitPr.MergedBy.Login,
                AvatarUrl = octokitPr.MergedBy.AvatarUrl,
                HtmlUrl = octokitPr.MergedBy.HtmlUrl
            } : null
        };
    }

    public async Task AddRepositoryAsync(Core.Models.Repository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        try
        {
            _logger.LogDebug("Adding/updating repository {Name} to cache", repository.Name);
            
            // Store in cache with a reasonable expiration
            await _cacheService.SetAsync($"repository_{repository.Id}", repository, TimeSpan.FromHours(1), cancellationToken);
            
            // Also update the repositories list cache
            const string cacheKey = "user_repositories";
            var repositories = await _cacheService.GetAsync<List<Core.Models.Repository>>(cacheKey, cancellationToken) ?? new List<Core.Models.Repository>();
            
            // Remove existing entry if present
            repositories.RemoveAll(r => r.Id == repository.Id);
            
            // Add updated repository
            repositories.Add(repository);
            
            // Update cache
            await _cacheService.SetAsync(cacheKey, repositories, TimeSpan.FromMinutes(30), cancellationToken);
            
            _logger.LogDebug("Repository {Name} added/updated in cache", repository.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding repository {Name} to cache", repository.Name);
            throw;
        }
    }

    public async Task AddPullRequestAsync(Core.Models.PullRequest pullRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pullRequest);

        try
        {
            _logger.LogDebug("Adding/updating pull request #{Number} to cache", pullRequest.Number);
            
            // Store individual PR in cache
            await _cacheService.SetAsync($"pullrequest_{pullRequest.RepositoryId}_{pullRequest.Number}", pullRequest, TimeSpan.FromHours(1), cancellationToken);
            
            // Update the PR list cache for the repository
            var cacheKey = $"pullrequests_{pullRequest.RepositoryId}";
            var pullRequests = await _cacheService.GetAsync<List<Core.Models.PullRequest>>(cacheKey, cancellationToken) ?? new List<Core.Models.PullRequest>();
            
            // Remove existing entry if present
            pullRequests.RemoveAll(pr => pr.Id == pullRequest.Id);
            
            // Add updated pull request
            pullRequests.Add(pullRequest);
            
            // Sort by updated date descending
            pullRequests = pullRequests.OrderByDescending(pr => pr.UpdatedAt).ToList();
            
            // Update cache
            await _cacheService.SetAsync(cacheKey, pullRequests, TimeSpan.FromMinutes(30), cancellationToken);
            
            _logger.LogDebug("Pull request #{Number} added/updated in cache", pullRequest.Number);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding pull request #{Number} to cache", pullRequest.Number);
            throw;
        }
    }

    public async Task AddCommentAsync(Core.Models.Comment comment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comment);

        try
        {
            _logger.LogDebug("Adding/updating comment {Id} to cache", comment.Id);
            
            // Store individual comment in cache
            await _cacheService.SetAsync($"comment_{comment.Id}", comment, TimeSpan.FromHours(1), cancellationToken);
            
            // For now, we'll skip updating the comments list cache since we don't have direct repository/PR access
            // This can be enhanced later when we need to efficiently retrieve comments by PR
            
            _logger.LogDebug("Comment {Id} added/updated in cache", comment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment {Id} to cache", comment.Id);
            throw;
        }
    }

    public async Task UpdateLastSyncAsync(string operation, DateTimeOffset timestamp, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        try
        {
            _logger.LogDebug("Updating last sync timestamp for operation {Operation}", operation);
            
            var cacheKey = $"last_sync_{operation}";
            // Store as string to avoid generic constraint issues
            await _cacheService.SetAsync(cacheKey, timestamp.ToString("O"), TimeSpan.FromDays(7), cancellationToken);
            
            _logger.LogDebug("Last sync timestamp updated for operation {Operation}: {Timestamp}", operation, timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last sync timestamp for operation {Operation}", operation);
            throw;
        }
    }

    public async Task<DateTimeOffset?> GetLastSyncAsync(string operation, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        try
        {
            var cacheKey = $"last_sync_{operation}";
            // Retrieve as string and parse
            var timestampString = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            
            if (string.IsNullOrEmpty(timestampString))
            {
                _logger.LogDebug("No last sync timestamp for operation {Operation}", operation);
                return null;
            }

            if (DateTimeOffset.TryParse(timestampString, out var timestamp))
            {
                _logger.LogDebug("Last sync timestamp for operation {Operation}: {Timestamp}", operation, timestamp);
                return timestamp;
            }

            _logger.LogWarning("Failed to parse timestamp {TimestampString} for operation {Operation}", timestampString, operation);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last sync timestamp for operation {Operation}", operation);
            return null;
        }
    }
}
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Data synchronization service for GitHub API to local cache
/// </summary>
public class DataSyncService : IDataSyncService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly IGitHubRepository _repository;
    private readonly IAuthService _authService;
    private readonly ILogger<DataSyncService> _logger;
    private bool _isSyncInProgress;

    public bool IsSyncInProgress => _isSyncInProgress;
    
    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;

    public DataSyncService(
        IGitHubClient gitHubClient,
        IGitHubRepository repository,
        IAuthService authService,
        ILogger<DataSyncService> logger)
    {
        _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SyncRepositoriesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
        {
            throw new InvalidOperationException("User must be authenticated to sync repositories");
        }

        if (_isSyncInProgress)
        {
            _logger.LogWarning("Sync already in progress, skipping repositories sync");
            return 0;
        }

        try
        {
            _isSyncInProgress = true;
            _logger.LogInformation("Starting repository synchronization");

            // Check if we need to refresh
            if (!forceRefresh)
            {
                var lastSync = await GetLastRepositorySyncAsync(cancellationToken);
                if (lastSync.HasValue && DateTimeOffset.UtcNow - lastSync.Value < TimeSpan.FromMinutes(30))
                {
                    _logger.LogDebug("Repository cache is fresh, skipping sync");
                    return 0;
                }
            }

            OnSyncProgressChanged("Repositories", 0, "Fetching repositories from GitHub...", 0, 0);

            // Get repositories from GitHub API
            var githubRepos = new List<Repository>();
            int currentPage = 1;
            IReadOnlyList<Repository> pageResults;

            do
            {
                var apiOptions = new ApiOptions { PageSize = 100, PageCount = 1, StartPage = currentPage };
                pageResults = await _gitHubClient.Repository.GetAllForCurrent(apiOptions);
                githubRepos.AddRange(pageResults);

                _logger.LogInformation("Retrieved {Count} repositories from GitHub API (Page {Page})", pageResults.Count, currentPage);

                currentPage++;
                cancellationToken.ThrowIfCancellationRequested();
            } while (pageResults.Count > 0);

            _logger.LogInformation("Retrieved a total of {Count} repositories from GitHub API", githubRepos.Count);
            OnSyncProgressChanged("Repositories", 25, "Processing repositories...", 0, githubRepos.Count);

            var syncedCount = 0;
            for (int i = 0; i < githubRepos.Count; i++)
            {
                var githubRepo = githubRepos[i];
                
                // Map GitHub repository to our model
                var repository = MapToRepository(githubRepo);
                
                // Save to local cache
                await _repository.AddRepositoryAsync(repository, cancellationToken);
                syncedCount++;

                // Update progress
                var progress = 25 + (int)((double)(i + 1) / githubRepos.Count * 75);
                OnSyncProgressChanged("Repositories", progress, $"Processed {i + 1}/{githubRepos.Count} repositories", i + 1, githubRepos.Count);

                cancellationToken.ThrowIfCancellationRequested();
            }

            // Update sync timestamp
            await _repository.UpdateLastSyncAsync("repositories", DateTimeOffset.UtcNow, cancellationToken);

            OnSyncProgressChanged("Repositories", 100, $"Synchronized {syncedCount} repositories successfully", syncedCount, githubRepos.Count);
            _logger.LogInformation("Repository synchronization completed. Synced {Count} repositories", syncedCount);

            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during repository synchronization");
            OnSyncProgressChanged("Repositories", 0, $"Error: {ex.Message}", 0, 0);
            throw;
        }
        finally
        {
            _isSyncInProgress = false;
        }
    }

    public async Task<int> SyncPullRequestsAsync(long repositoryId, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
        {
            throw new InvalidOperationException("User must be authenticated to sync pull requests");
        }

        if (_isSyncInProgress)
        {
            _logger.LogWarning("Sync already in progress, skipping pull requests sync");
            return 0;
        }

        try
        {
            _isSyncInProgress = true;
            _logger.LogInformation("Starting pull request synchronization for repository {RepositoryId}", repositoryId);

            // Get repository information
            var repo = await _repository.GetRepositoryByIdAsync(repositoryId, cancellationToken);
            if (repo == null)
            {
                throw new ArgumentException($"Repository with ID {repositoryId} not found", nameof(repositoryId));
            }

            OnSyncProgressChanged("Pull Requests", 0, $"Fetching pull requests for {repo.Name}...", 0, 0);

            // Get pull requests from GitHub API
            var request = new PullRequestRequest
            {
                State = ItemStateFilter.All,
                SortProperty = PullRequestSort.Updated,
                SortDirection = SortDirection.Descending
            };

            var githubPrs = new List<PullRequest>();
            var page = 1;
            const int pageSize = 100;

            while (true)
            {
                var options = new ApiOptions
                {
                    PageSize = pageSize,
                    PageCount = 1,
                    StartPage = page
                };

                var prsPage = await _gitHubClient.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, request, options);
                if (prsPage.Count == 0)
                {
                    break;
                }

                githubPrs.AddRange(prsPage);
                _logger.LogInformation("Retrieved {Count} pull requests from page {Page} for repository {Repository}", prsPage.Count, page, repo.Name);

                page++;

                cancellationToken.ThrowIfCancellationRequested();
            }

            _logger.LogInformation("Retrieved a total of {Count} pull requests for repository {Repository}", githubPrs.Count, repo.Name);
            OnSyncProgressChanged("Pull Requests", 25, "Processing pull requests...", 0, githubPrs.Count);

            var syncedCount = 0;
            for (int i = 0; i < githubPrs.Count; i++)
            {
                var githubPr = githubPrs[i];
                
                // Map GitHub pull request to our model
                var pullRequest = MapToPullRequest(githubPr, repositoryId);
                
                // Save to local cache
                await _repository.AddPullRequestAsync(pullRequest, cancellationToken);
                syncedCount++;

                // Update progress
                var progress = 25 + (int)((double)(i + 1) / githubPrs.Count * 75);
                OnSyncProgressChanged("Pull Requests", progress, $"Processed {i + 1}/{githubPrs.Count} pull requests", i + 1, githubPrs.Count);

                cancellationToken.ThrowIfCancellationRequested();
            }

            // Update sync timestamp
            await _repository.UpdateLastSyncAsync($"pullrequests_{repositoryId}", DateTimeOffset.UtcNow, cancellationToken);

            OnSyncProgressChanged("Pull Requests", 100, $"Synchronized {syncedCount} pull requests successfully", syncedCount, githubPrs.Count);
            _logger.LogInformation("Pull request synchronization completed. Synced {Count} pull requests", syncedCount);

            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pull request synchronization for repository {RepositoryId}", repositoryId);
            OnSyncProgressChanged("Pull Requests", 0, $"Error: {ex.Message}", 0, 0);
            throw;
        }
        finally
        {
            _isSyncInProgress = false;
        }
    }

    public async Task<int> SyncCommentsAsync(long repositoryId, int pullRequestNumber, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
        {
            throw new InvalidOperationException("User must be authenticated to sync comments");
        }

        if (_isSyncInProgress)
        {
            _logger.LogWarning("Sync already in progress, skipping comments sync");
            return 0;
        }

        try
        {
            _isSyncInProgress = true;
            _logger.LogInformation("Starting comment synchronization for repository {RepositoryId}, PR {PullRequestNumber}", repositoryId, pullRequestNumber);

            // Get repository and pull request information
            var repo = await _repository.GetRepositoryByIdAsync(repositoryId, cancellationToken);
            if (repo == null)
            {
                throw new ArgumentException($"Repository with ID {repositoryId} not found", nameof(repositoryId));
            }

            OnSyncProgressChanged("Comments", 0, $"Fetching comments for PR #{pullRequestNumber}...", 0, 0);

            // Get comments from GitHub API (both issue comments and review comments)
            var issueComments = await _gitHubClient.Issue.Comment.GetAllForIssue(repo.Owner.Login, repo.Name, pullRequestNumber);
            var reviewComments = await _gitHubClient.PullRequest.ReviewComment.GetAll(repo.Owner.Login, repo.Name, pullRequestNumber);

            var totalComments = issueComments.Count + reviewComments.Count;
            _logger.LogInformation("Retrieved {IssueComments} issue comments and {ReviewComments} review comments", 
                issueComments.Count, reviewComments.Count);

            OnSyncProgressChanged("Comments", 25, "Processing comments...", 0, totalComments);

            var syncedCount = 0;
            var processedCount = 0;

            // Process issue comments
            foreach (var issueComment in issueComments)
            {
                var comment = MapToComment(issueComment, repositoryId, pullRequestNumber);
                await _repository.AddCommentAsync(comment, cancellationToken);
                syncedCount++;
                processedCount++;

                var progress = 25 + (int)((double)processedCount / totalComments * 75);
                OnSyncProgressChanged("Comments", progress, $"Processed {processedCount}/{totalComments} comments", processedCount, totalComments);

                cancellationToken.ThrowIfCancellationRequested();
            }

            // Process review comments
            foreach (var reviewComment in reviewComments)
            {
                var comment = MapToComment(reviewComment, repositoryId, pullRequestNumber);
                await _repository.AddCommentAsync(comment, cancellationToken);
                syncedCount++;
                processedCount++;

                var progress = 25 + (int)((double)processedCount / totalComments * 75);
                OnSyncProgressChanged("Comments", progress, $"Processed {processedCount}/{totalComments} comments", processedCount, totalComments);

                cancellationToken.ThrowIfCancellationRequested();
            }

            OnSyncProgressChanged("Comments", 100, $"Synchronized {syncedCount} comments successfully", syncedCount, totalComments);
            _logger.LogInformation("Comment synchronization completed. Synced {Count} comments", syncedCount);

            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during comment synchronization for repository {RepositoryId}, PR {PullRequestNumber}", repositoryId, pullRequestNumber);
            OnSyncProgressChanged("Comments", 0, $"Error: {ex.Message}", 0, 0);
            throw;
        }
        finally
        {
            _isSyncInProgress = false;
        }
    }

    public async Task<DateTimeOffset?> GetLastRepositorySyncAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetLastSyncAsync("repositories", cancellationToken);
    }

    public async Task<DateTimeOffset?> GetLastPullRequestSyncAsync(long repositoryId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetLastSyncAsync($"pullrequests_{repositoryId}", cancellationToken);
    }

    private void OnSyncProgressChanged(string operationType, int progressPercentage, string message, int itemsProcessed, int totalItems)
    {
        SyncProgressChanged?.Invoke(this, new SyncProgressEventArgs
        {
            OperationType = operationType,
            ProgressPercentage = progressPercentage,
            Message = message,
            ItemsProcessed = itemsProcessed,
            TotalItems = totalItems
        });
    }

    private static Core.Models.Repository MapToRepository(Octokit.Repository githubRepo)
    {
        return new Core.Models.Repository
        {
            Id = githubRepo.Id,
            Name = githubRepo.Name,
            FullName = githubRepo.FullName,
            Owner = new Core.Models.User
            {
                Id = githubRepo.Owner.Id,
                Login = githubRepo.Owner.Login,
                AvatarUrl = githubRepo.Owner.AvatarUrl,
                HtmlUrl = githubRepo.Owner.HtmlUrl
            },
            Description = githubRepo.Description,
            Private = githubRepo.Private,
            HtmlUrl = githubRepo.HtmlUrl,
            CloneUrl = githubRepo.CloneUrl,
            CreatedAt = githubRepo.CreatedAt,
            UpdatedAt = githubRepo.UpdatedAt,
            PushedAt = githubRepo.PushedAt,
            StargazersCount = githubRepo.StargazersCount,
            Language = githubRepo.Language,
            ForksCount = githubRepo.ForksCount,
            OpenIssuesCount = githubRepo.OpenIssuesCount,
            DefaultBranch = githubRepo.DefaultBranch
        };
    }

    private static Core.Models.PullRequest MapToPullRequest(Octokit.PullRequest githubPr, long repositoryId)
    {
        return new Core.Models.PullRequest
        {
            Id = githubPr.Id,
            Number = githubPr.Number,
            RepositoryId = repositoryId,
            Title = githubPr.Title,
            Body = githubPr.Body,
            State = githubPr.State.Value switch
            {
                ItemState.Open => PullRequestState.Open,
                ItemState.Closed => githubPr.MergedAt.HasValue ? PullRequestState.Merged : PullRequestState.Closed,
                _ => PullRequestState.Closed
            },
            Author = new Core.Models.User
            {
                Id = githubPr.User.Id,
                Login = githubPr.User.Login,
                AvatarUrl = githubPr.User.AvatarUrl,
                HtmlUrl = githubPr.User.HtmlUrl
            },
            BaseBranch = githubPr.Base.Ref,
            HeadBranch = githubPr.Head.Ref,
            IsDraft = githubPr.Draft,
            HtmlUrl = githubPr.HtmlUrl,
            Mergeable = githubPr.Mergeable,
            Commits = githubPr.Commits,
            Additions = githubPr.Additions,
            Deletions = githubPr.Deletions,
            ChangedFiles = githubPr.ChangedFiles,
            CreatedAt = githubPr.CreatedAt,
            UpdatedAt = githubPr.UpdatedAt,
            ClosedAt = githubPr.ClosedAt,
            MergedAt = githubPr.MergedAt
        };
    }

    private static Core.Models.Comment MapToComment(IssueComment issueComment, long repositoryId, int pullRequestNumber)
    {
        return new Core.Models.Comment
        {
            Id = issueComment.Id,
            Body = issueComment.Body,
            Type = CommentType.Issue,
            Author = new Core.Models.User
            {
                Id = issueComment.User.Id,
                Login = issueComment.User.Login,
                AvatarUrl = issueComment.User.AvatarUrl,
                HtmlUrl = issueComment.User.HtmlUrl
            },
            CreatedAt = issueComment.CreatedAt,
            UpdatedAt = issueComment.UpdatedAt ?? issueComment.CreatedAt,
            HtmlUrl = issueComment.HtmlUrl
        };
    }

    private static Core.Models.Comment MapToComment(PullRequestReviewComment reviewComment, long repositoryId, int pullRequestNumber)
    {
        return new Core.Models.Comment
        {
            Id = reviewComment.Id,
            Body = reviewComment.Body,
            Type = CommentType.Review,
            Author = new Core.Models.User
            {
                Id = reviewComment.User.Id,
                Login = reviewComment.User.Login,
                AvatarUrl = reviewComment.User.AvatarUrl,
                HtmlUrl = reviewComment.User.HtmlUrl
            },
            CreatedAt = reviewComment.CreatedAt,
            UpdatedAt = reviewComment.UpdatedAt,
            HtmlUrl = reviewComment.HtmlUrl,
            Path = reviewComment.Path,
            Position = reviewComment.Position,
            DiffHunk = reviewComment.DiffHunk
        };
    }
}
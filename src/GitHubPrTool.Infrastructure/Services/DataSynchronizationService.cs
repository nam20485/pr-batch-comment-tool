using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Service for synchronizing data between GitHub API and local cache
/// </summary>
public class DataSynchronizationService : IDataSynchronizationService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DataSynchronizationService> _logger;
    private readonly SemaphoreSlim _synchronizationSemaphore = new(1, 1);
    
    private bool _isSynchronizationInProgress;

    public bool IsSynchronizationInProgress => _isSynchronizationInProgress;

    public event EventHandler<SynchronizationProgressEventArgs>? SynchronizationProgress;

    public DataSynchronizationService(
        IGitHubClient gitHubClient,
        ICacheService cacheService,
        ILogger<DataSynchronizationService> logger)
    {
        _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SynchronizeRepositoriesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        await _synchronizationSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Starting repository synchronization (forceRefresh: {ForceRefresh})", forceRefresh);
            
            // Check if we need to refresh
            if (!forceRefresh)
            {
                var lastSync = await GetLastSynchronizationTimeAsync("repositories", cancellationToken: cancellationToken);
                if (lastSync.HasValue && DateTime.UtcNow - lastSync.Value < TimeSpan.FromHours(1))
                {
                    _logger.LogDebug("Repositories were synchronized recently, skipping");
                    return 0;
                }
            }

            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = "Fetching repositories from GitHub",
                ProgressPercentage = 0
            });

            // Fetch repositories from GitHub
            var repositories = await _gitHubClient.Repository.GetAllForCurrent();
            var repositoryCount = repositories.Count;

            _logger.LogInformation("Found {Count} repositories to synchronize", repositoryCount);

            var synchronizedCount = 0;
            for (int i = 0; i < repositoryCount; i++)
            {
                var repo = repositories[i];
                
                try
                {
                    // Convert Octokit repository to our domain model
                    var domainRepo = MapToRepository(repo);
                    
                    // Cache the repository
                    var cacheKey = $"repository_{repo.Id}";
                    await _cacheService.SetAsync(cacheKey, domainRepo, TimeSpan.FromHours(6), cancellationToken);
                    
                    synchronizedCount++;

                    OnSynchronizationProgress(new SynchronizationProgressEventArgs
                    {
                        Operation = $"Synchronized repository: {repo.Name}",
                        ProgressPercentage = (int)((double)(i + 1) / repositoryCount * 100),
                        ItemsProcessed = i + 1,
                        TotalItems = repositoryCount
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing repository {RepositoryName}", repo.Name);
                }
            }

            // Update synchronization timestamp
            await _cacheService.SetAsync("sync_repositories_timestamp", DateTime.UtcNow.ToString("O"), TimeSpan.FromDays(30), cancellationToken);
            
            _logger.LogInformation("Repository synchronization completed. Synchronized {Count} repositories", synchronizedCount);
            return synchronizedCount;
        }
        finally
        {
            _synchronizationSemaphore.Release();
        }
    }

    public async Task<int> SynchronizePullRequestsAsync(long repositoryId, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        await _synchronizationSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Starting pull request synchronization for repository {RepositoryId}", repositoryId);

            // Check if we need to refresh
            if (!forceRefresh)
            {
                var lastSync = await GetLastSynchronizationTimeAsync("pull_requests", repositoryId, cancellationToken);
                if (lastSync.HasValue && DateTime.UtcNow - lastSync.Value < TimeSpan.FromMinutes(30))
                {
                    _logger.LogDebug("Pull requests for repository {RepositoryId} were synchronized recently, skipping", repositoryId);
                    return 0;
                }
            }

            // Get repository details first
            var repository = await _gitHubClient.Repository.Get((int)repositoryId);
            
            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = $"Fetching pull requests for {repository.Name}",
                ProgressPercentage = 0
            });

            // Fetch pull requests
            var pullRequests = await _gitHubClient.PullRequest.GetAllForRepository((int)repositoryId);
            var prCount = pullRequests.Count;

            _logger.LogInformation("Found {Count} pull requests to synchronize for repository {RepositoryId}", prCount, repositoryId);

            var synchronizedCount = 0;
            for (int i = 0; i < prCount; i++)
            {
                var pr = pullRequests[i];
                
                try
                {
                    // Convert to domain model
                    var domainPr = MapToPullRequest(pr, repositoryId);
                    
                    // Cache the pull request
                    var cacheKey = $"pullrequest_{pr.Id}";
                    await _cacheService.SetAsync(cacheKey, domainPr, TimeSpan.FromHours(2), cancellationToken);
                    
                    synchronizedCount++;

                    OnSynchronizationProgress(new SynchronizationProgressEventArgs
                    {
                        Operation = $"Synchronized PR: {pr.Title}",
                        ProgressPercentage = (int)((double)(i + 1) / prCount * 100),
                        ItemsProcessed = i + 1,
                        TotalItems = prCount
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing pull request {PrNumber}", pr.Number);
                }
            }

            // Update synchronization timestamp
            await _cacheService.SetAsync($"sync_pullrequests_{repositoryId}_timestamp", DateTime.UtcNow.ToString("O"), TimeSpan.FromDays(30), cancellationToken);
            
            _logger.LogInformation("Pull request synchronization completed. Synchronized {Count} pull requests", synchronizedCount);
            return synchronizedCount;
        }
        finally
        {
            _synchronizationSemaphore.Release();
        }
    }

    public async Task<int> SynchronizeCommentsAsync(int pullRequestId, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        await _synchronizationSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Starting comment synchronization for pull request {PullRequestId}", pullRequestId);

            // Check if we need to refresh
            if (!forceRefresh)
            {
                var lastSync = await GetLastSynchronizationTimeAsync("comments", pullRequestId, cancellationToken);
                if (lastSync.HasValue && DateTime.UtcNow - lastSync.Value < TimeSpan.FromMinutes(15))
                {
                    _logger.LogDebug("Comments for pull request {PullRequestId} were synchronized recently, skipping", pullRequestId);
                    return 0;
                }
            }

            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = $"Fetching comments for PR {pullRequestId}",
                ProgressPercentage = 0
            });

            // Note: We need repository info to get PR details. This is a simplified version.
            // In a real implementation, we'd store the repository ID with the PR.
            var synchronizedCount = 0;

            // For now, we'll skip the actual GitHub API call for comments
            // as it requires more complex repository tracking
            _logger.LogInformation("Comment synchronization placeholder - would sync comments for PR {PullRequestId}", pullRequestId);

            // Update synchronization timestamp
            await _cacheService.SetAsync($"sync_comments_{pullRequestId}_timestamp", DateTime.UtcNow.ToString("O"), TimeSpan.FromDays(30), cancellationToken);
            
            return synchronizedCount;
        }
        finally
        {
            _synchronizationSemaphore.Release();
        }
    }

    public async Task<SynchronizationResult> PerformFullSynchronizationAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new SynchronizationResult();

        try
        {
            _isSynchronizationInProgress = true;
            
            _logger.LogInformation("Starting full synchronization");

            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = "Starting full synchronization",
                ProgressPercentage = 0
            });

            // Step 1: Synchronize repositories (30% of total progress)
            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = "Synchronizing repositories",
                ProgressPercentage = 10
            });

            result.RepositoriesSynchronized = await SynchronizeRepositoriesAsync(forceRefresh: true, cancellationToken);

            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = "Repositories synchronized",
                ProgressPercentage = 30
            });

            // Step 2: Synchronize pull requests for each repository (60% of total progress)
            var repositories = await GetCachedRepositoriesAsync(cancellationToken);
            var totalRepos = repositories.Count;
            
            for (int i = 0; i < totalRepos; i++)
            {
                var repo = repositories[i];
                try
                {
                    var prCount = await SynchronizePullRequestsAsync(repo.Id, forceRefresh: true, cancellationToken);
                    result.PullRequestsSynchronized += prCount;

                    var progressPercentage = 30 + (int)((double)(i + 1) / totalRepos * 60);
                    OnSynchronizationProgress(new SynchronizationProgressEventArgs
                    {
                        Operation = $"Synchronized PRs for {repo.Name}",
                        ProgressPercentage = progressPercentage,
                        ItemsProcessed = i + 1,
                        TotalItems = totalRepos
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing pull requests for repository {RepositoryName}", repo.Name);
                    result.Errors.Add($"Failed to sync PRs for {repo.Name}: {ex.Message}");
                }
            }

            OnSynchronizationProgress(new SynchronizationProgressEventArgs
            {
                Operation = "Full synchronization completed",
                ProgressPercentage = 100
            });

            result.Duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Full synchronization completed in {Duration}. Repositories: {Repos}, PRs: {PRs}",
                result.Duration, result.RepositoriesSynchronized, result.PullRequestsSynchronized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full synchronization");
            result.Errors.Add($"Full synchronization failed: {ex.Message}");
            result.Duration = DateTime.UtcNow - startTime;
        }
        finally
        {
            _isSynchronizationInProgress = false;
        }

        return result;
    }

    public async Task<DateTime?> GetLastSynchronizationTimeAsync(string dataType, long? entityId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = entityId.HasValue 
                ? $"sync_{dataType}_{entityId}_timestamp"
                : $"sync_{dataType}_timestamp";
                
            var timestampString = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (DateTime.TryParse(timestampString, out var timestamp))
            {
                return timestamp;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last synchronization time for {DataType}", dataType);
            return null;
        }
    }

    private async Task<List<Core.Models.Repository>> GetCachedRepositoriesAsync(CancellationToken cancellationToken)
    {
        // This is a simplified implementation. In a real scenario, we'd have a better way to get all cached repositories.
        var repositories = new List<Core.Models.Repository>();
        
        try
        {
            // For now, return an empty list as we'd need a proper caching strategy to track all repository IDs
            _logger.LogWarning("GetCachedRepositoriesAsync is not fully implemented - returning empty list");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached repositories");
        }

        return repositories;
    }

    private Core.Models.Repository MapToRepository(Octokit.Repository octokitRepo)
    {
        return new Core.Models.Repository
        {
            Id = octokitRepo.Id,
            Name = octokitRepo.Name,
            FullName = octokitRepo.FullName,
            Description = octokitRepo.Description ?? string.Empty,
            Private = octokitRepo.Private,
            HtmlUrl = octokitRepo.HtmlUrl,
            CloneUrl = octokitRepo.CloneUrl,
            Language = octokitRepo.Language ?? string.Empty,
            StargazersCount = octokitRepo.StargazersCount,
            ForksCount = octokitRepo.ForksCount,
            CreatedAt = octokitRepo.CreatedAt.UtcDateTime,
            UpdatedAt = octokitRepo.UpdatedAt.UtcDateTime
        };
    }

    private Core.Models.PullRequest MapToPullRequest(Octokit.PullRequest octokitPr, long repositoryId)
    {
        return new Core.Models.PullRequest
        {
            Id = octokitPr.Id,
            Number = octokitPr.Number,
            Title = octokitPr.Title,
            Body = octokitPr.Body ?? string.Empty,
            State = Enum.Parse<Core.Models.PullRequestState>(octokitPr.State.ToString(), true),
            RepositoryId = repositoryId,
            HtmlUrl = octokitPr.HtmlUrl,
            CreatedAt = octokitPr.CreatedAt.UtcDateTime,
            UpdatedAt = octokitPr.UpdatedAt.UtcDateTime
        };
    }

    private void OnSynchronizationProgress(SynchronizationProgressEventArgs args)
    {
        SynchronizationProgress?.Invoke(this, args);
    }
}
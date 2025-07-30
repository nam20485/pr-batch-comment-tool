namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for data synchronization between GitHub API and local cache
/// </summary>
public interface IDataSynchronizationService
{
    /// <summary>
    /// Synchronize repositories from GitHub API to local cache
    /// </summary>
    /// <param name="forceRefresh">Force refresh even if data is recent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of repositories synchronized</returns>
    Task<int> SynchronizeRepositoriesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize pull requests for a specific repository
    /// </summary>
    /// <param name="repositoryId">Repository ID to sync pull requests for</param>
    /// <param name="forceRefresh">Force refresh even if data is recent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of pull requests synchronized</returns>
    Task<int> SynchronizePullRequestsAsync(long repositoryId, bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize comments for a specific pull request
    /// </summary>
    /// <param name="pullRequestId">Pull request ID to sync comments for</param>
    /// <param name="forceRefresh">Force refresh even if data is recent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of comments synchronized</returns>
    Task<int> SynchronizeCommentsAsync(int pullRequestId, bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform a full synchronization of all user data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synchronization statistics</returns>
    Task<SynchronizationResult> PerformFullSynchronizationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the last synchronization time for a specific data type
    /// </summary>
    /// <param name="dataType">Type of data (repositories, pull_requests, comments)</param>
    /// <param name="entityId">Optional entity ID for scoped sync times</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Last synchronization time, or null if never synchronized</returns>
    Task<DateTime?> GetLastSynchronizationTimeAsync(string dataType, long? entityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if synchronization is currently in progress
    /// </summary>
    /// <returns>True if synchronization is running, false otherwise</returns>
    bool IsSynchronizationInProgress { get; }

    /// <summary>
    /// Event raised when synchronization progress changes
    /// </summary>
    event EventHandler<SynchronizationProgressEventArgs>? SynchronizationProgress;
}

/// <summary>
/// Result of a data synchronization operation
/// </summary>
public class SynchronizationResult
{
    /// <summary>
    /// Number of repositories synchronized
    /// </summary>
    public int RepositoriesSynchronized { get; set; }

    /// <summary>
    /// Number of pull requests synchronized
    /// </summary>
    public int PullRequestsSynchronized { get; set; }

    /// <summary>
    /// Number of comments synchronized
    /// </summary>
    public int CommentsSynchronized { get; set; }

    /// <summary>
    /// Total time taken for synchronization
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Any errors encountered during synchronization
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Whether the synchronization completed successfully
    /// </summary>
    public bool IsSuccessful => Errors.Count == 0;
}

/// <summary>
/// Event arguments for synchronization progress updates
/// </summary>
public class SynchronizationProgressEventArgs : EventArgs
{
    /// <summary>
    /// Current operation being performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int ItemsProcessed { get; set; }

    /// <summary>
    /// Total number of items to process
    /// </summary>
    public int TotalItems { get; set; }
}
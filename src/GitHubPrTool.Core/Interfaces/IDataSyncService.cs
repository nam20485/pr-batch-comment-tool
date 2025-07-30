namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for synchronizing GitHub data with local cache
/// </summary>
public interface IDataSyncService
{
    /// <summary>
    /// Synchronize repositories for the authenticated user
    /// </summary>
    /// <param name="forceRefresh">Force refresh from API even if cache is valid</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of repositories synchronized</returns>
    Task<int> SyncRepositoriesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize pull requests for a specific repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="forceRefresh">Force refresh from API even if cache is valid</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of pull requests synchronized</returns>
    Task<int> SyncPullRequestsAsync(long repositoryId, bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize comments for a specific pull request
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="pullRequestNumber">Pull request number</param>
    /// <param name="forceRefresh">Force refresh from API even if cache is valid</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of comments synchronized</returns>
    Task<int> SyncCommentsAsync(long repositoryId, int pullRequestNumber, bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a sync operation is currently in progress
    /// </summary>
    bool IsSyncInProgress { get; }

    /// <summary>
    /// Event fired when sync progress changes
    /// </summary>
    event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;

    /// <summary>
    /// Get the last sync time for repositories
    /// </summary>
    Task<DateTimeOffset?> GetLastRepositorySyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the last sync time for a repository's pull requests
    /// </summary>
    Task<DateTimeOffset?> GetLastPullRequestSyncAsync(long repositoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event arguments for sync progress updates
/// </summary>
public class SyncProgressEventArgs : EventArgs
{
    /// <summary>
    /// Type of operation being synced
    /// </summary>
    public string OperationType { get; init; } = string.Empty;

    /// <summary>
    /// Current progress (0-100)
    /// </summary>
    public int ProgressPercentage { get; init; }

    /// <summary>
    /// Current operation message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int ItemsProcessed { get; init; }

    /// <summary>
    /// Total items to process
    /// </summary>
    public int TotalItems { get; init; }
}
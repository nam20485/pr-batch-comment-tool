namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for monitoring network connectivity status
/// </summary>
public interface INetworkConnectivityService
{
    /// <summary>
    /// Gets a value indicating whether the device is currently connected to the internet
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets a value indicating whether the GitHub API is currently reachable
    /// </summary>
    bool IsGitHubReachable { get; }

    /// <summary>
    /// Event fired when network connectivity status changes
    /// </summary>
    event EventHandler<NetworkConnectivityChangedEventArgs>? ConnectivityChanged;

    /// <summary>
    /// Check connectivity status asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connected to internet and GitHub is reachable</returns>
    Task<bool> CheckConnectivityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start monitoring connectivity changes
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stop monitoring connectivity changes
    /// </summary>
    void StopMonitoring();
}

/// <summary>
/// Event arguments for network connectivity changes
/// </summary>
public class NetworkConnectivityChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets a value indicating whether the device is connected to the internet
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Gets a value indicating whether GitHub is reachable
    /// </summary>
    public bool IsGitHubReachable { get; init; }

    /// <summary>
    /// Gets the previous connectivity status
    /// </summary>
    public bool WasConnected { get; init; }

    /// <summary>
    /// Gets a message describing the connectivity change
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
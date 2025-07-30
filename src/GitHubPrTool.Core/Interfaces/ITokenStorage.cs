namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for secure token storage operations
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Store a token securely with encryption
    /// </summary>
    /// <param name="key">The key to store the token under</param>
    /// <param name="token">The token to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreTokenAsync(string key, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve and decrypt a stored token
    /// </summary>
    /// <param name="key">The key the token was stored under</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The decrypted token, or null if not found</returns>
    Task<string?> RetrieveTokenAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a stored token
    /// </summary>
    /// <param name="key">The key the token was stored under</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a token exists for the given key
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a token exists, false otherwise</returns>
    Task<bool> HasTokenAsync(string key, CancellationToken cancellationToken = default);
}
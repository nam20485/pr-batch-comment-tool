namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for GitHub authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets whether the user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current access token
    /// </summary>
    string? AccessToken { get; }

    /// <summary>
    /// Gets the authenticated user information
    /// </summary>
    Models.User? CurrentUser { get; }

    /// <summary>
    /// Event fired when authentication status changes
    /// </summary>
    event EventHandler<bool> AuthenticationChanged;

    /// <summary>
    /// Initiate GitHub OAuth2 device flow authentication
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device authorization information (user code, verification URI, etc.)</returns>
    Task<DeviceAuthorizationResult> StartAuthenticationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Poll for authentication completion
    /// </summary>
    /// <param name="deviceCode">Device code from StartAuthenticationAsync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication succeeded, false otherwise</returns>
    Task<bool> CompleteAuthenticationAsync(string deviceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sign out the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh the access token if needed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was refreshed or is still valid, false if re-authentication is needed</returns>
    Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load saved authentication state on application startup
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if valid authentication was loaded, false otherwise</returns>
    Task<bool> LoadAuthenticationAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from GitHub device authorization flow
/// </summary>
public record DeviceAuthorizationResult
{
    /// <summary>
    /// Device code for polling
    /// </summary>
    public string DeviceCode { get; init; } = string.Empty;

    /// <summary>
    /// User code to display to the user
    /// </summary>
    public string UserCode { get; init; } = string.Empty;

    /// <summary>
    /// URL where user should enter the user code
    /// </summary>
    public string VerificationUri { get; init; } = string.Empty;

    /// <summary>
    /// Complete verification URL with user code (if available)
    /// </summary>
    public string? VerificationUriComplete { get; init; }

    /// <summary>
    /// Interval in seconds between polling attempts
    /// </summary>
    public int Interval { get; init; }

    /// <summary>
    /// Expiration time for the device code
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }
}
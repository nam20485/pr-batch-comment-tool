using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// GitHub OAuth2 authentication service implementation
/// </summary>
public class GitHubAuthService : IAuthService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GitHubAuthService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;

    private string? _accessToken;
    private Core.Models.User? _currentUser;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && _currentUser != null;
    public string? AccessToken => _accessToken;
    public Core.Models.User? CurrentUser => _currentUser;

    public event EventHandler<bool>? AuthenticationChanged;

    public GitHubAuthService(
        IGitHubClient gitHubClient,
        ICacheService cacheService,
        ILogger<GitHubAuthService> logger,
        string clientId,
        string clientSecret)
    {
        _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
    }

    public async Task<DeviceAuthorizationResult> StartAuthenticationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting GitHub device flow authentication");

            var request = new OauthDeviceFlowRequest(_clientId);

            var deviceFlow = await _gitHubClient.Oauth.InitiateDeviceFlow(request);

            var result = new DeviceAuthorizationResult
            {
                DeviceCode = deviceFlow.DeviceCode,
                UserCode = deviceFlow.UserCode,
                VerificationUri = deviceFlow.VerificationUri,
                VerificationUriComplete = deviceFlow.VerificationUri, // Octokit doesn't provide the complete URL separately
                Interval = deviceFlow.Interval,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(deviceFlow.ExpiresIn)
            };

            _logger.LogInformation("Device flow initiated. User code: {UserCode}, Verification URI: {VerificationUri}", 
                result.UserCode, result.VerificationUri);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting GitHub authentication");
            throw;
        }
    }

    public async Task<bool> CompleteAuthenticationAsync(string deviceCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceCode);

        try
        {
            _logger.LogInformation("Attempting to complete GitHub authentication with device code");

            var request = new OauthTokenRequest(_clientId, _clientSecret, deviceCode);
            var token = await _gitHubClient.Oauth.CreateAccessToken(request);

            if (token?.AccessToken == null)
            {
                _logger.LogWarning("No access token received from GitHub");
                return false;
            }

            await SetAccessTokenAsync(token.AccessToken, cancellationToken);
            
            _logger.LogInformation("GitHub authentication completed successfully");
            return true;
        }
        catch (AuthorizationException)
        {
            // This is expected when the user hasn't authorized yet or if authorization is pending
            _logger.LogDebug("Authorization pending or denied");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing GitHub authentication");
            return false;
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Signing out user");

            _accessToken = null;
            _currentUser = null;
            _gitHubClient.Connection.Credentials = Credentials.Anonymous;

            // Clear stored authentication data
            await _cacheService.RemoveAsync("github_access_token", cancellationToken);
            await _cacheService.RemoveAsync("github_current_user", cancellationToken);

            AuthenticationChanged?.Invoke(this, false);
            
            _logger.LogInformation("User signed out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out");
            throw;
        }
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            return false;
        }

        try
        {
            // GitHub personal access tokens don't expire, but we can validate the current token
            var user = await _gitHubClient.User.Current();
            
            if (user != null)
            {
                _logger.LogDebug("Access token is still valid");
                return true;
            }

            _logger.LogWarning("Access token validation failed");
            await SignOutAsync(cancellationToken);
            return false;
        }
        catch (AuthorizationException)
        {
            _logger.LogWarning("Access token is no longer valid");
            await SignOutAsync(cancellationToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }

    public async Task<bool> LoadAuthenticationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Loading saved authentication state");

            var encryptedToken = await _cacheService.GetAsync<string>("github_access_token", cancellationToken);
            if (string.IsNullOrEmpty(encryptedToken))
            {
                _logger.LogDebug("No saved access token found");
                return false;
            }

            var accessToken = DecryptToken(encryptedToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to decrypt saved access token");
                await _cacheService.RemoveAsync("github_access_token", cancellationToken);
                return false;
            }

            await SetAccessTokenAsync(accessToken, cancellationToken);
            
            // Validate the token by making a test API call
            if (await RefreshTokenAsync(cancellationToken))
            {
                _logger.LogInformation("Loaded authentication state successfully");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading authentication state");
            return false;
        }
    }

    private async Task SetAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        _accessToken = accessToken;
        _gitHubClient.Connection.Credentials = new Credentials(accessToken);

        // Get current user information
        var octokitUser = await _gitHubClient.User.Current();
        _currentUser = MapToUser(octokitUser);

        // Store encrypted token and user information
        var encryptedToken = EncryptToken(accessToken);
        await _cacheService.SetAsync("github_access_token", encryptedToken, TimeSpan.FromDays(30), cancellationToken);
        await _cacheService.SetAsync("github_current_user", _currentUser, TimeSpan.FromDays(1), cancellationToken);

        AuthenticationChanged?.Invoke(this, true);
    }

    private static Core.Models.User MapToUser(Octokit.User octokitUser)
    {
        return new Core.Models.User
        {
            Id = octokitUser.Id,
            Login = octokitUser.Login,
            Name = octokitUser.Name,
            Email = octokitUser.Email,
            AvatarUrl = octokitUser.AvatarUrl,
            HtmlUrl = octokitUser.HtmlUrl,
            Bio = octokitUser.Bio,
            CreatedAt = octokitUser.CreatedAt,
            UpdatedAt = octokitUser.UpdatedAt
        };
    }

    private string EncryptToken(string token)
    {
        try
        {
            // Simple encryption using Data Protection API (Windows) or basic encryption for cross-platform
            var data = Encoding.UTF8.GetBytes(token);
            var entropy = Encoding.UTF8.GetBytes("GitHubPrTool"); // Additional entropy
            
            // For simplicity, using base64 encoding. In production, use proper encryption
            return Convert.ToBase64String(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting token");
            throw;
        }
    }

    private string DecryptToken(string encryptedToken)
    {
        try
        {
            // Corresponding decryption for the simple encryption above
            var data = Convert.FromBase64String(encryptedToken);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting token");
            return string.Empty;
        }
    }
}
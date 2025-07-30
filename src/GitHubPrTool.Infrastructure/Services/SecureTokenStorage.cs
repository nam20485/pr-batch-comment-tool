using GitHubPrTool.Core.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Secure token storage implementation using Data Protection API
/// </summary>
public class SecureTokenStorage : ITokenStorage
{
    private readonly ICacheService _cacheService;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger<SecureTokenStorage> _logger;

    public SecureTokenStorage(
        ICacheService cacheService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SecureTokenStorage> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Create a data protector specifically for tokens
        _dataProtector = dataProtectionProvider.CreateProtector("GitHubPrTool.Tokens");
    }

    public async Task StoreTokenAsync(string key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        try
        {
            _logger.LogDebug("Storing encrypted token for key: {Key}", key);

            // Encrypt the token
            var encryptedToken = _dataProtector.Protect(token);
            
            // Store with longer expiration for tokens
            await _cacheService.SetAsync($"token_{key}", encryptedToken, TimeSpan.FromDays(30), cancellationToken);
            
            _logger.LogInformation("Token stored successfully for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing token for key: {Key}", key);
            throw;
        }
    }

    public async Task<string?> RetrieveTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            _logger.LogDebug("Retrieving token for key: {Key}", key);

            var encryptedToken = await _cacheService.GetAsync<string>($"token_{key}", cancellationToken);
            if (string.IsNullOrEmpty(encryptedToken))
            {
                _logger.LogDebug("No token found for key: {Key}", key);
                return null;
            }

            // Decrypt the token
            var decryptedToken = _dataProtector.Unprotect(encryptedToken);
            
            _logger.LogDebug("Token retrieved successfully for key: {Key}", key);
            return decryptedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving token for key: {Key}", key);
            return null;
        }
    }

    public async Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            _logger.LogDebug("Removing token for key: {Key}", key);
            
            await _cacheService.RemoveAsync($"token_{key}", cancellationToken);
            
            _logger.LogInformation("Token removed successfully for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing token for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> HasTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var encryptedToken = await _cacheService.GetAsync<string>($"token_{key}", cancellationToken);
            return !string.IsNullOrEmpty(encryptedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token existence for key: {Key}", key);
            return false;
        }
    }
}
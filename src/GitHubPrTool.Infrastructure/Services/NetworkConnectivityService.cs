using System.Net.NetworkInformation;
using GitHubPrTool.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Service for monitoring network connectivity and GitHub API availability
/// </summary>
public class NetworkConnectivityService : INetworkConnectivityService, IDisposable
{
    private readonly ILogger<NetworkConnectivityService> _logger;
    private readonly HttpClient _httpClient;
    private bool _isConnected;
    private bool _isGitHubReachable;
    private bool _isMonitoring;
    private bool _disposed;

    private const string GitHubApiPingUrl = "https://api.github.com/zen";
    private const int MonitoringIntervalMs = 30000; // 30 seconds

    public bool IsConnected => _isConnected;
    public bool IsGitHubReachable => _isGitHubReachable;

    public event EventHandler<NetworkConnectivityChangedEventArgs>? ConnectivityChanged;

    public NetworkConnectivityService(ILogger<NetworkConnectivityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await CheckConnectivityAsync(cancellationToken);
    }

    public async Task<bool> CheckConnectivityAsync(CancellationToken cancellationToken = default)
    {
        var wasConnected = _isConnected;
        var wasGitHubReachable = _isGitHubReachable;

        try
        {
            // Check basic internet connectivity
            _isConnected = await CheckInternetConnectivityAsync(cancellationToken);

            // Check GitHub API reachability if internet is available
            if (_isConnected)
            {
                _isGitHubReachable = await CheckGitHubConnectivityAsync(cancellationToken);
            }
            else
            {
                _isGitHubReachable = false;
            }

            // Fire event if status changed
            if (wasConnected != _isConnected || wasGitHubReachable != _isGitHubReachable)
            {
                var message = GetConnectivityMessage();
                _logger.LogInformation("Connectivity status changed: {Message}", message);

                ConnectivityChanged?.Invoke(this, new NetworkConnectivityChangedEventArgs
                {
                    IsConnected = _isConnected,
                    IsGitHubReachable = _isGitHubReachable,
                    WasConnected = wasConnected,
                    Message = message
                });
            }

            return _isConnected && _isGitHubReachable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connectivity");
            _isConnected = false;
            _isGitHubReachable = false;
            return false;
        }
    }

    private async Task<bool> CheckInternetConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check network interfaces
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            // Try to ping a reliable external service
            await _pingSemaphore.WaitAsync(cancellationToken);
            try
            {
                var reply = await _ping.SendPingAsync("8.8.8.8", 5000);
                return reply.Status == IPStatus.Success;
            }
            finally
            {
                _pingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Internet connectivity check failed: {Message}", ex.Message);
            return false;
        }
    }

    private async Task<bool> CheckGitHubConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(GitHubApiPingUrl, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitHub connectivity check failed: {Message}", ex.Message);
            return false;
        }
    }

    private string GetConnectivityMessage()
    {
        return (_isConnected, _isGitHubReachable) switch
        {
            (true, true) => "Connected to internet and GitHub API is reachable",
            (true, false) => "Connected to internet but GitHub API is not reachable",
            (false, false) => "No internet connection",
            _ => "Unknown connectivity status"
        };
    }

    public void StartMonitoring()
    {
        if (_isMonitoring || _disposed)
            return;

        _isMonitoring = true;
        _logger.LogInformation("Starting network connectivity monitoring");

        // Start monitoring timer is not needed since we're using a different pattern
        // The monitoring is handled by periodic checks triggered by the UI or other services
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring || _disposed)
            return;

        _isMonitoring = false;
        _logger.LogInformation("Stopping network connectivity monitoring");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopMonitoring();
        _httpClient?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
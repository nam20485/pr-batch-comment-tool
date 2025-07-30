using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for the main window of the GitHub PR Review Assistant.
/// Handles navigation, authentication status, and main application state.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IDataSyncService _dataSyncService;
    private readonly INetworkConnectivityService _networkConnectivityService;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _connectionStatus = "Offline";

    [ObservableProperty]
    private string? _currentUser;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isContentLoaded;

    [ObservableProperty]
    private object? _currentContent;

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _syncStatus = "";

    [ObservableProperty]
    private int _syncProgress;

    [ObservableProperty]
    private bool _isOnline;

    [ObservableProperty]
    private bool _isGitHubReachable;

    [ObservableProperty]
    private bool _isOfflineMode;

    /// <summary>
    /// Repository list view model for navigation.
    /// </summary>
    public RepositoryListViewModel? RepositoryListViewModel { get; private set; }

    /// <summary>
    /// Pull request list view model for navigation.
    /// </summary>
    public PullRequestListViewModel? PullRequestListViewModel { get; private set; }

    /// <summary>
    /// Pull request detail view model for navigation.
    /// </summary>
    public PullRequestDetailViewModel? PullRequestDetailViewModel { get; private set; }

    /// <summary>
    /// Comment list view model for navigation.
    /// </summary>
    public CommentListViewModel? CommentListViewModel { get; private set; }

    /// <summary>
    /// Global search view model for navigation.
    /// </summary>
    public GlobalSearchViewModel? GlobalSearchViewModel { get; private set; }

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel.
    /// </summary>
    /// <param name="authService">GitHub authentication service.</param>
    /// <param name="dataSyncService">Data synchronization service.</param>
    /// <param name="networkConnectivityService">Network connectivity service.</param>
    /// <param name="repositoryListViewModel">Repository list view model.</param>
    /// <param name="pullRequestListViewModel">Pull request list view model.</param>
    /// <param name="pullRequestDetailViewModel">Pull request detail view model.</param>
    /// <param name="commentListViewModel">Comment list view model.</param>
    /// <param name="globalSearchViewModel">Global search view model.</param>
    /// <param name="logger">Logger for this view model.</param>
    public MainWindowViewModel(
        IAuthService authService, 
        IDataSyncService dataSyncService,
        INetworkConnectivityService networkConnectivityService,
        RepositoryListViewModel repositoryListViewModel,
        PullRequestListViewModel pullRequestListViewModel,
        PullRequestDetailViewModel pullRequestDetailViewModel,
        CommentListViewModel commentListViewModel,
        GlobalSearchViewModel globalSearchViewModel,
        ILogger<MainWindowViewModel> logger)
    {
        _authService = authService;
        _dataSyncService = dataSyncService;
        _networkConnectivityService = networkConnectivityService;
        RepositoryListViewModel = repositoryListViewModel;
        PullRequestListViewModel = pullRequestListViewModel;
        PullRequestDetailViewModel = pullRequestDetailViewModel;
        CommentListViewModel = commentListViewModel;
        GlobalSearchViewModel = globalSearchViewModel;
        _logger = logger;
        
        // Wire up navigation from pull request list to detail view
        if (PullRequestListViewModel != null)
        {
            PullRequestListViewModel.NavigateToPullRequestDetail = NavigateToPullRequestDetailAsync;
        }
        
        // Wire up sync progress events
        _dataSyncService.SyncProgressChanged += OnSyncProgressChanged;
        
        // Wire up network connectivity events
        _networkConnectivityService.ConnectivityChanged += OnConnectivityChanged;
        
        // Initialize authentication status
        UpdateAuthenticationStatus();
        
        // Initialize connectivity monitoring
        _networkConnectivityService.StartMonitoring();
        
        // Initial connectivity check
        _ = Task.Run(async () => await _networkConnectivityService.CheckConnectivityAsync());
    }

    /// <summary>
    /// Command to authenticate with GitHub using OAuth2 device flow.
    /// </summary>
    [RelayCommand]
    private async Task AuthenticateAsync()
    {
        try
        {
            StatusMessage = "Starting GitHub authentication...";
            _logger.LogInformation("Starting GitHub authentication process");

            // Start the authentication process
            var deviceAuth = await _authService.StartAuthenticationAsync();
            StatusMessage = $"Go to {deviceAuth.VerificationUri} and enter code: {deviceAuth.UserCode}";
            
            // Poll for completion
            var success = await _authService.CompleteAuthenticationAsync(deviceAuth.DeviceCode);
            
            if (success)
            {
                StatusMessage = "Successfully authenticated with GitHub!";
                UpdateAuthenticationStatus();
                _logger.LogInformation("GitHub authentication successful");
            }
            else
            {
                StatusMessage = "Authentication failed. Please try again.";
                _logger.LogWarning("GitHub authentication failed");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Authentication error: {ex.Message}";
            _logger.LogError(ex, "Error during GitHub authentication");
        }
    }

    /// <summary>
    /// Command to navigate to the repositories view.
    /// </summary>
    [RelayCommand]
    private void NavigateToRepositories()
    {
        StatusMessage = "Loading repositories...";
        IsContentLoaded = true;
        CurrentContent = RepositoryListViewModel;
        _logger.LogInformation("Navigating to repositories view");
    }

    /// <summary>
    /// Command to navigate to the pull requests view.
    /// </summary>
    [RelayCommand]
    private void NavigateToPullRequests()
    {
        StatusMessage = "Loading pull requests...";
        IsContentLoaded = true;
        CurrentContent = PullRequestListViewModel;
        _logger.LogInformation("Navigating to pull requests view");
    }

    /// <summary>
    /// Command to navigate to the comments view.
    /// </summary>
    [RelayCommand]
    private void NavigateToComments()
    {
        StatusMessage = "Loading comments...";
        IsContentLoaded = true;
        CurrentContent = CommentListViewModel;
        _logger.LogInformation("Navigating to comments view");
    }

    /// <summary>
    /// Command to navigate to global search view.
    /// </summary>
    [RelayCommand]
    private void NavigateToSearch()
    {
        StatusMessage = "Loading search...";
        IsContentLoaded = true;
        CurrentContent = GlobalSearchViewModel;
        _logger.LogInformation("Navigating to global search view");
    }

    /// <summary>
    /// Navigates to the pull request detail view for the specified pull request.
    /// </summary>
    /// <param name="pullRequest">Pull request to show details for.</param>
    public async Task NavigateToPullRequestDetailAsync(PullRequest pullRequest)
    {
        if (PullRequestDetailViewModel == null)
        {
            _logger.LogWarning("PullRequestDetailViewModel is null, cannot navigate to PR detail");
            return;
        }

        StatusMessage = $"Loading details for PR #{pullRequest.Number}...";
        IsContentLoaded = true;
        CurrentContent = PullRequestDetailViewModel;
        
        _logger.LogInformation("Navigating to pull request detail view for PR #{Number}", pullRequest.Number);
        
        // Load the pull request details
        await PullRequestDetailViewModel.LoadPullRequestDetailAsync(pullRequest);
        
        StatusMessage = $"Viewing PR #{pullRequest.Number}: {pullRequest.Title}";
    }

    /// <summary>
    /// Command to open batch comment functionality.
    /// </summary>
    [RelayCommand]
    private void BatchComment()
    {
        StatusMessage = "Opening batch comment tool...";
        _logger.LogInformation("Opening batch comment functionality");
        // TODO: Implement batch comment functionality
    }

    /// <summary>
    /// Command to open application settings.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        StatusMessage = "Opening settings...";
        _logger.LogInformation("Opening application settings");
        // TODO: Implement settings dialog
    }

    /// <summary>
    /// Command to show about dialog.
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        StatusMessage = "Showing about information...";
        _logger.LogInformation("Showing about dialog");
        // TODO: Implement about dialog
    }

    /// <summary>
    /// Command to exit the application.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        _logger.LogInformation("Application exit requested");
        
        // Clean up network monitoring
        _networkConnectivityService.StopMonitoring();
        
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Command to manually refresh data from GitHub.
    /// </summary>
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        if (!IsAuthenticated)
        {
            StatusMessage = "Please authenticate first";
            return;
        }

        if (IsSyncing)
        {
            _logger.LogWarning("Sync already in progress");
            return;
        }

        try
        {
            _logger.LogInformation("Starting manual data refresh");
            
            // Refresh repositories first
            await _dataSyncService.SyncRepositoriesAsync(forceRefresh: true);
            
            StatusMessage = "Data refresh completed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Data refresh failed: {ex.Message}";
            _logger.LogError(ex, "Error during manual data refresh");
        }
    }

    /// <summary>
    /// Command to toggle offline mode.
    /// </summary>
    [RelayCommand]
    private void ToggleOfflineMode()
    {
        IsOfflineMode = !IsOfflineMode;
        StatusMessage = IsOfflineMode ? "Offline mode enabled" : "Offline mode disabled";
        _logger.LogInformation("Offline mode {State}", IsOfflineMode ? "enabled" : "disabled");
    }

    /// <summary>
    /// Event handler for sync progress changes.
    /// </summary>
    private void OnSyncProgressChanged(object? sender, SyncProgressEventArgs e)
    {
        IsSyncing = e.ProgressPercentage < 100;
        SyncProgress = e.ProgressPercentage;
        SyncStatus = e.Message;
        
        if (IsSyncing)
        {
            StatusMessage = $"Syncing: {e.Message}";
        }
        else if (e.ProgressPercentage == 100)
        {
            StatusMessage = "Sync completed successfully";
        }
    }

    /// <summary>
    /// Event handler for network connectivity changes.
    /// </summary>
    private void OnConnectivityChanged(object? sender, NetworkConnectivityChangedEventArgs e)
    {
        IsOnline = e.IsConnected;
        IsGitHubReachable = e.IsGitHubReachable;
        
        ConnectionStatus = (e.IsConnected, e.IsGitHubReachable) switch
        {
            (true, true) => "Online",
            (true, false) => "Limited",
            (false, false) => "Offline",
            _ => "Unknown"
        };

        if (!e.WasConnected && e.IsConnected)
        {
            StatusMessage = "Connection restored - " + e.Message;
        }
        else if (e.WasConnected && !e.IsConnected)
        {
            StatusMessage = "Connection lost - " + e.Message;
        }

        _logger.LogInformation("Connectivity changed: {Message}", e.Message);
    }

    /// <summary>
    /// Updates the authentication status and related properties.
    /// </summary>
    private async void UpdateAuthenticationStatus()
    {
        try
        {
            // Load authentication state on startup
            await _authService.LoadAuthenticationAsync();
            
            IsAuthenticated = _authService.IsAuthenticated;
            
            if (IsAuthenticated)
            {
                CurrentUser = _authService.CurrentUser?.Login;
                StatusMessage = $"Connected as {CurrentUser}";
                
                // Update connection status based on current connectivity
                if (IsOnline && IsGitHubReachable)
                {
                    ConnectionStatus = "Online";
                }
                else
                {
                    ConnectionStatus = IsOnline ? "Limited" : "Offline";
                }
            }
            else
            {
                CurrentUser = null;
                ConnectionStatus = "Offline";
                StatusMessage = "Not authenticated";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating authentication status");
            IsAuthenticated = false;
            CurrentUser = null;
            ConnectionStatus = "Error";
            StatusMessage = "Authentication check failed";
        }
    }
}
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
    /// Initializes a new instance of the MainWindowViewModel.
    /// </summary>
    /// <param name="authService">GitHub authentication service.</param>
    /// <param name="repositoryListViewModel">Repository list view model.</param>
    /// <param name="pullRequestListViewModel">Pull request list view model.</param>
    /// <param name="pullRequestDetailViewModel">Pull request detail view model.</param>
    /// <param name="logger">Logger for this view model.</param>
    public MainWindowViewModel(
        IAuthService authService, 
        RepositoryListViewModel repositoryListViewModel,
        PullRequestListViewModel pullRequestListViewModel,
        PullRequestDetailViewModel pullRequestDetailViewModel,
        ILogger<MainWindowViewModel> logger)
    {
        _authService = authService;
        RepositoryListViewModel = repositoryListViewModel;
        PullRequestListViewModel = pullRequestListViewModel;
        PullRequestDetailViewModel = pullRequestDetailViewModel;
        _logger = logger;
        
        // Wire up navigation from pull request list to detail view
        if (PullRequestListViewModel != null)
        {
            PullRequestListViewModel.NavigateToPullRequestDetail = NavigateToPullRequestDetailAsync;
        }
        
        // Initialize authentication status
        UpdateAuthenticationStatus();
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
        _logger.LogInformation("Navigating to comments view");
        // TODO: Implement navigation to comments view
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
        System.Environment.Exit(0);
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
                ConnectionStatus = "Online";
                StatusMessage = $"Connected as {CurrentUser}";
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
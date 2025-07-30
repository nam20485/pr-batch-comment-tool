using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for the pull request list view with sorting capabilities.
/// </summary>
public partial class PullRequestListViewModel : ObservableObject
{
    private readonly IGitHubRepository _gitHubRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly IAuthService _authService;
    private readonly ILogger<PullRequestListViewModel> _logger;
    private Repository? _currentRepository;
    private List<PullRequest> _allPullRequests = new();

    [ObservableProperty]
    private ObservableCollection<PullRequest> _pullRequests = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _selectedStateFilter = "All";

    [ObservableProperty]
    private string _sortBy = "Updated";

    [ObservableProperty]
    private bool _sortDescending = true;

    [ObservableProperty]
    private PullRequest? _selectedPullRequest;

    [ObservableProperty]
    private string _repositoryName = "All Repositories";

    /// <summary>
    /// Available state filters for pull requests.
    /// </summary>
    public ObservableCollection<string> StateFilters { get; } = new() { "All", "Open", "Closed", "Merged" };

    /// <summary>
    /// Available sort options for pull requests.
    /// </summary>
    public ObservableCollection<string> SortOptions { get; } = new() { "Updated", "Created", "Number", "Title" };

    /// <summary>
    /// Initializes a new instance of the PullRequestListViewModel.
    /// </summary>
    /// <param name="gitHubRepository">GitHub repository service.</param>
    /// <param name="dataSyncService">Data synchronization service.</param>
    /// <param name="authService">Authentication service.</param>
    /// <param name="logger">Logger for this view model.</param>
    public PullRequestListViewModel(
        IGitHubRepository gitHubRepository,
        IDataSyncService dataSyncService,
        IAuthService authService,
        ILogger<PullRequestListViewModel> logger)
    {
        _gitHubRepository = gitHubRepository;
        _dataSyncService = dataSyncService;
        _authService = authService;
        _logger = logger;

        // Subscribe to property changes for filtering and sorting
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(SearchText) or nameof(SelectedStateFilter) or 
                nameof(SortBy) or nameof(SortDescending))
            {
                FilterAndSortPullRequests();
            }
        };
    }

    /// <summary>
    /// Sets the current repository to load pull requests for.
    /// </summary>
    /// <param name="repository">Repository to load pull requests for.</param>
    public void SetRepository(Repository repository)
    {
        _currentRepository = repository;
        RepositoryName = repository.Name;
        _logger.LogInformation("Repository set to: {Repository}", repository.FullName);
    }

    /// <summary>
    /// Command to load pull requests from cache.
    /// </summary>
    [RelayCommand]
    private async Task LoadPullRequestsAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            StatusMessage = "Please authenticate with GitHub first";
            return;
        }

        if (_currentRepository == null)
        {
            StatusMessage = "Please select a repository first";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Loading pull requests for {_currentRepository.Name}...";
            _logger.LogInformation("Loading pull requests for repository: {Repository}", _currentRepository.FullName);

            // Load from local cache
            var pullRequests = await _gitHubRepository.GetPullRequestsAsync(_currentRepository.Id);
            _allPullRequests = pullRequests.ToList();

            FilterAndSortPullRequests();

            StatusMessage = $"Loaded {_allPullRequests.Count} pull requests";
            _logger.LogInformation("Successfully loaded {Count} pull requests", _allPullRequests.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading pull requests: {ex.Message}";
            _logger.LogError(ex, "Error loading pull requests for repository: {Repository}", _currentRepository?.FullName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to sync pull requests from GitHub API.
    /// </summary>
    [RelayCommand]
    private async Task SyncPullRequestsAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            StatusMessage = "Please authenticate with GitHub first";
            return;
        }

        if (_currentRepository == null)
        {
            StatusMessage = "Please select a repository first";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Syncing pull requests for {_currentRepository.Name} from GitHub...";
            _logger.LogInformation("Starting pull request synchronization for repository: {Repository}", _currentRepository.FullName);

            var syncedCount = await _dataSyncService.SyncPullRequestsAsync(_currentRepository.Id, forceRefresh: true);

            // Reload after sync
            await LoadPullRequestsAsync();

            StatusMessage = $"Synced {syncedCount} pull requests from GitHub";
            _logger.LogInformation("Successfully synced {Count} pull requests", syncedCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error syncing pull requests: {ex.Message}";
            _logger.LogError(ex, "Error during pull request synchronization for repository: {Repository}", _currentRepository?.FullName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to view comments for the selected pull request.
    /// </summary>
    [RelayCommand]
    private void ViewComments()
    {
        if (SelectedPullRequest == null)
        {
            StatusMessage = "Please select a pull request first";
            return;
        }

        _logger.LogInformation("Navigating to comments for PR: {PRNumber}", SelectedPullRequest.Number);
        StatusMessage = $"Loading comments for PR #{SelectedPullRequest.Number}...";
        
        // TODO: Navigate to comments view with selected pull request
        // This will be implemented when we add navigation service
    }

    /// <summary>
    /// Command to open the pull request in browser.
    /// </summary>
    [RelayCommand]
    private void OpenInBrowser()
    {
        if (SelectedPullRequest?.HtmlUrl == null)
        {
            StatusMessage = "Cannot open pull request - URL not available";
            return;
        }

        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = SelectedPullRequest.HtmlUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processInfo);
            
            StatusMessage = $"Opened PR #{SelectedPullRequest.Number} in browser";
            _logger.LogInformation("Opened PR in browser: {URL}", SelectedPullRequest.HtmlUrl);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening browser: {ex.Message}";
            _logger.LogError(ex, "Error opening PR in browser: {URL}", SelectedPullRequest.HtmlUrl);
        }
    }

    /// <summary>
    /// Filters and sorts the pull requests based on current criteria.
    /// </summary>
    private void FilterAndSortPullRequests()
    {
        var filtered = _allPullRequests.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(pr => 
                pr.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                pr.Number.ToString().Contains(SearchText) ||
                (pr.Body != null && pr.Body.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                pr.Author.Login.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply state filter
        if (SelectedStateFilter != "All")
        {
            if (Enum.TryParse<PullRequestState>(SelectedStateFilter, out var state))
            {
                filtered = filtered.Where(pr => pr.State == state);
            }
        }

        // Apply sorting
        filtered = SortBy switch
        {
            "Updated" => SortDescending 
                ? filtered.OrderByDescending(pr => pr.UpdatedAt)
                : filtered.OrderBy(pr => pr.UpdatedAt),
            "Created" => SortDescending
                ? filtered.OrderByDescending(pr => pr.CreatedAt)
                : filtered.OrderBy(pr => pr.CreatedAt),
            "Number" => SortDescending
                ? filtered.OrderByDescending(pr => pr.Number)
                : filtered.OrderBy(pr => pr.Number),
            "Title" => SortDescending
                ? filtered.OrderByDescending(pr => pr.Title)
                : filtered.OrderBy(pr => pr.Title),
            _ => filtered.OrderByDescending(pr => pr.UpdatedAt)
        };

        // Update the observable collection
        PullRequests.Clear();
        foreach (var pr in filtered)
        {
            PullRequests.Add(pr);
        }

        _logger.LogDebug("Applied filters and sorting: {FilteredCount} out of {TotalCount} pull requests", 
            PullRequests.Count, _allPullRequests.Count);
    }
}
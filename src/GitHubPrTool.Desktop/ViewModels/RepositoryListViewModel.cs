using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for the repository list view with search and filtering capabilities.
/// </summary>
public partial class RepositoryListViewModel : ObservableObject
{
    private readonly IGitHubRepository _gitHubRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly IAuthService _authService;
    private readonly ILogger<RepositoryListViewModel> _logger;
    private List<Repository> _allRepositories = new();

    [ObservableProperty]
    private ObservableCollection<Repository> _repositories = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _selectedLanguageFilter = "All";

    [ObservableProperty]
    private bool _showPrivateOnly;

    [ObservableProperty]
    private bool _showPublicOnly;

    [ObservableProperty]
    private Repository? _selectedRepository;

    /// <summary>
    /// Available language filters for repositories.
    /// </summary>
    public ObservableCollection<string> LanguageFilters { get; } = new() { "All" };

    /// <summary>
    /// Initializes a new instance of the RepositoryListViewModel.
    /// </summary>
    /// <param name="gitHubRepository">GitHub repository service.</param>
    /// <param name="dataSyncService">Data synchronization service.</param>
    /// <param name="authService">Authentication service.</param>
    /// <param name="logger">Logger for this view model.</param>
    public RepositoryListViewModel(
        IGitHubRepository gitHubRepository,
        IDataSyncService dataSyncService,
        IAuthService authService,
        ILogger<RepositoryListViewModel> logger)
    {
        _gitHubRepository = gitHubRepository;
        _dataSyncService = dataSyncService;
        _authService = authService;
        _logger = logger;

        // Subscribe to property changes for filtering
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(SearchText) or nameof(SelectedLanguageFilter) or 
                nameof(ShowPrivateOnly) or nameof(ShowPublicOnly))
            {
                FilterRepositories();
            }
        };
    }

    /// <summary>
    /// Command to load repositories from cache and optionally sync from GitHub.
    /// </summary>
    [RelayCommand]
    private async Task LoadRepositoriesAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            StatusMessage = "Please authenticate with GitHub first";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Loading repositories...";
            _logger.LogInformation("Loading repositories from cache");

            // Load from local cache first
            var repositories = await _gitHubRepository.GetRepositoriesAsync();
            _allRepositories = repositories.ToList();

            UpdateLanguageFilters();
            FilterRepositories();

            StatusMessage = $"Loaded {_allRepositories.Count} repositories";
            _logger.LogInformation("Successfully loaded {Count} repositories from cache", _allRepositories.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading repositories: {ex.Message}";
            _logger.LogError(ex, "Error loading repositories from cache");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to sync repositories from GitHub API.
    /// </summary>
    [RelayCommand]
    private async Task SyncRepositoriesAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            StatusMessage = "Please authenticate with GitHub first";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Syncing repositories from GitHub...";
            _logger.LogInformation("Starting repository synchronization");

            var syncedCount = await _dataSyncService.SyncRepositoriesAsync(forceRefresh: true);

            // Reload after sync
            await LoadRepositoriesAsync();

            StatusMessage = $"Synced {syncedCount} repositories from GitHub";
            _logger.LogInformation("Successfully synced {Count} repositories", syncedCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error syncing repositories: {ex.Message}";
            _logger.LogError(ex, "Error during repository synchronization");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to search repositories.
    /// </summary>
    [RelayCommand]
    private async Task SearchRepositoriesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilterRepositories();
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Searching for '{SearchText}'...";
            _logger.LogInformation("Searching repositories with query: {Query}", SearchText);

            var searchResults = await _gitHubRepository.SearchRepositoriesAsync(SearchText);
            _allRepositories = searchResults.ToList();

            UpdateLanguageFilters();
            FilterRepositories();

            StatusMessage = $"Found {_allRepositories.Count} repositories matching '{SearchText}'";
            _logger.LogInformation("Search completed with {Count} results", _allRepositories.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error searching repositories: {ex.Message}";
            _logger.LogError(ex, "Error during repository search");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to view pull requests for the selected repository.
    /// </summary>
    [RelayCommand]
    private void ViewPullRequests()
    {
        if (SelectedRepository == null)
        {
            StatusMessage = "Please select a repository first";
            return;
        }

        _logger.LogInformation("Navigating to pull requests for repository: {Repository}", SelectedRepository.FullName);
        StatusMessage = $"Loading pull requests for {SelectedRepository.Name}...";
        
        // TODO: Navigate to pull requests view with selected repository
        // This will be implemented when we add navigation service
    }

    /// <summary>
    /// Filters the repositories based on current filter criteria.
    /// </summary>
    private void FilterRepositories()
    {
        var filtered = _allRepositories.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(r => 
                r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (r.Description != null && r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        // Apply language filter
        if (SelectedLanguageFilter != "All")
        {
            filtered = filtered.Where(r => 
                string.Equals(r.Language, SelectedLanguageFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Apply privacy filters
        if (ShowPrivateOnly)
        {
            filtered = filtered.Where(r => r.Private);
        }
        else if (ShowPublicOnly)
        {
            filtered = filtered.Where(r => !r.Private);
        }

        // Update the observable collection
        Repositories.Clear();
        foreach (var repo in filtered.OrderBy(r => r.Name))
        {
            Repositories.Add(repo);
        }

        _logger.LogDebug("Applied filters: {FilteredCount} out of {TotalCount} repositories", 
            Repositories.Count, _allRepositories.Count);
    }

    /// <summary>
    /// Updates the available language filters based on current repositories.
    /// </summary>
    private void UpdateLanguageFilters()
    {
        var languages = _allRepositories
            .Where(r => !string.IsNullOrWhiteSpace(r.Language))
            .Select(r => r.Language!)
            .Distinct()
            .OrderBy(l => l)
            .ToList();

        LanguageFilters.Clear();
        LanguageFilters.Add("All");
        foreach (var language in languages)
        {
            LanguageFilters.Add(language);
        }

        // Reset to "All" if current selection is no longer available
        if (!LanguageFilters.Contains(SelectedLanguageFilter))
        {
            SelectedLanguageFilter = "All";
        }
    }
}
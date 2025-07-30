using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for global search functionality across all data types
/// </summary>
public partial class GlobalSearchViewModel : ObservableObject
{
    private readonly ISearchService _searchService;
    private readonly ILogger<GlobalSearchViewModel> _logger;
    private SearchResults? _lastSearchResults;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _statusMessage = "Enter a search query to begin";

    [ObservableProperty]
    private bool _includeRepositories = true;

    [ObservableProperty]
    private bool _includePullRequests = true;

    [ObservableProperty]
    private bool _includeComments = true;

    [ObservableProperty]
    private bool _caseSensitive = false;

    [ObservableProperty]
    private bool _exactMatch = false;

    [ObservableProperty]
    private int _maxResults = 50;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    [ObservableProperty]
    private string _selectedAuthors = string.Empty;

    [ObservableProperty]
    private long _executionTimeMs;

    [ObservableProperty]
    private int _totalResults;

    [ObservableProperty]
    private bool _isTruncated;

    [ObservableProperty]
    private string _selectedTab = "All";

    /// <summary>
    /// Repository search results
    /// </summary>
    public ObservableCollection<Repository> RepositoryResults { get; } = new();

    /// <summary>
    /// Pull request search results
    /// </summary>
    public ObservableCollection<PullRequest> PullRequestResults { get; } = new();

    /// <summary>
    /// Comment search results
    /// </summary>
    public ObservableCollection<Comment> CommentResults { get; } = new();

    /// <summary>
    /// Search suggestions
    /// </summary>
    public ObservableCollection<string> SearchSuggestions { get; } = new();

    /// <summary>
    /// Available search tabs
    /// </summary>
    public ObservableCollection<string> SearchTabs { get; } = new() 
    { 
        "All", "Repositories", "Pull Requests", "Comments" 
    };

    /// <summary>
    /// Event fired when user wants to navigate to a repository
    /// </summary>
    public Action<Repository>? NavigateToRepository { get; set; }

    /// <summary>
    /// Event fired when user wants to navigate to a pull request
    /// </summary>
    public Action<PullRequest>? NavigateToPullRequest { get; set; }

    /// <summary>
    /// Event fired when user wants to navigate to a comment
    /// </summary>
    public Action<Comment>? NavigateToComment { get; set; }

    /// <summary>
    /// Initializes a new instance of the GlobalSearchViewModel
    /// </summary>
    /// <param name="searchService">Search service</param>
    /// <param name="logger">Logger</param>
    public GlobalSearchViewModel(ISearchService searchService, ILogger<GlobalSearchViewModel> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set up property change handlers
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SearchQuery))
            {
                _ = SafeExecuteAsync(LoadSearchSuggestionsAsync);
            }
        };
    }

    /// <summary>
    /// Command to perform search
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            StatusMessage = "Please enter a search query";
            return;
        }

        if (IsSearching)
        {
            _logger.LogWarning("Search already in progress");
            return;
        }

        IsSearching = true;
        StatusMessage = $"Searching for '{SearchQuery}'...";

        try
        {
            _logger.LogInformation("Starting search for query: '{Query}'", SearchQuery);

            var searchOptions = new SearchOptions
            {
                IncludeRepositories = IncludeRepositories,
                IncludePullRequests = IncludePullRequests,
                IncludeComments = IncludeComments,
                MaxResultsPerCategory = MaxResults,
                CaseSensitive = CaseSensitive,
                ExactMatch = ExactMatch,
                DateFrom = DateFrom?.ToUniversalTime(),
                DateTo = DateTo?.ToUniversalTime()
            };

            // Parse authors filter
            if (!string.IsNullOrWhiteSpace(SelectedAuthors))
            {
                searchOptions.Authors = SelectedAuthors
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }

            _lastSearchResults = await _searchService.SearchAsync(SearchQuery, searchOptions);

            // Update results
            UpdateSearchResults(_lastSearchResults);

            ExecutionTimeMs = _lastSearchResults.ExecutionTimeMs;
            TotalResults = _lastSearchResults.TotalResults;
            IsTruncated = _lastSearchResults.IsTruncated;

            StatusMessage = $"Found {TotalResults} results in {ExecutionTimeMs}ms";
            if (IsTruncated)
            {
                StatusMessage += " (results truncated)";
            }

            _logger.LogInformation("Search completed for query '{Query}'. Found {TotalResults} results in {ExecutionTimeMs}ms",
                SearchQuery, TotalResults, ExecutionTimeMs);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
            _logger.LogError(ex, "Error performing search for query '{Query}'", SearchQuery);
        }
        finally
        {
            IsSearching = false;
        }
    }

    /// <summary>
    /// Command to clear search results and filters
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        RepositoryResults.Clear();
        PullRequestResults.Clear();
        CommentResults.Clear();
        SearchSuggestions.Clear();
        
        TotalResults = 0;
        ExecutionTimeMs = 0;
        IsTruncated = false;
        
        StatusMessage = "Enter a search query to begin";
        _lastSearchResults = null;

        _logger.LogInformation("Search cleared");
    }

    /// <summary>
    /// Command to reset search filters to defaults
    /// </summary>
    [RelayCommand]
    private void ResetFilters()
    {
        IncludeRepositories = true;
        IncludePullRequests = true;
        IncludeComments = true;
        CaseSensitive = false;
        ExactMatch = false;
        MaxResults = 50;
        DateFrom = null;
        DateTo = null;
        SelectedAuthors = string.Empty;

        StatusMessage = "Search filters reset to defaults";
        _logger.LogInformation("Search filters reset");
    }

    /// <summary>
    /// Command to navigate to repository details
    /// </summary>
    /// <param name="repository">Repository to navigate to</param>
    [RelayCommand]
    private void NavigateToRepositoryCommand(Repository repository)
    {
        _logger.LogInformation("Navigating to repository: {Repository}", repository.FullName);
        NavigateToRepository?.Invoke(repository);
    }

    /// <summary>
    /// Command to navigate to pull request details
    /// </summary>
    /// <param name="pullRequest">Pull request to navigate to</param>
    [RelayCommand]
    private void NavigateToPullRequestCommand(PullRequest pullRequest)
    {
        _logger.LogInformation("Navigating to pull request: {Repository}#{Number}", pullRequest.Repository?.FullName, pullRequest.Number);
        NavigateToPullRequest?.Invoke(pullRequest);
    }

    /// <summary>
    /// Command to navigate to comment details
    /// </summary>
    /// <param name="comment">Comment to navigate to</param>
    [RelayCommand]
    private void NavigateToCommentCommand(Comment comment)
    {
        _logger.LogInformation("Navigating to comment: {CommentId}", comment.Id);
        NavigateToComment?.Invoke(comment);
    }

    /// <summary>
    /// Command to apply suggested search term
    /// </summary>
    /// <param name="suggestion">Search suggestion to apply</param>
    [RelayCommand]
    private void ApplySuggestion(string suggestion)
    {
        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            SearchQuery = suggestion;
            SearchSuggestions.Clear();
            _logger.LogDebug("Applied search suggestion: '{Suggestion}'", suggestion);
        }
    }

    /// <summary>
    /// Load search suggestions based on current query
    /// </summary>
    private async Task LoadSearchSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2)
        {
            SearchSuggestions.Clear();
            return;
        }

        try
        {
            var suggestions = await _searchService.GetSearchSuggestionsAsync(SearchQuery, 10);
            
            SearchSuggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                SearchSuggestions.Add(suggestion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading search suggestions for query '{Query}'", SearchQuery);
        }
    }

    /// <summary>
    /// Update search results based on selected tab
    /// </summary>
    /// <param name="searchResults">Search results to display</param>
    private void UpdateSearchResults(SearchResults searchResults)
    {
        RepositoryResults.Clear();
        PullRequestResults.Clear();
        CommentResults.Clear();

        if (IncludeRepositories)
        {
            foreach (var repo in searchResults.Repositories)
            {
                RepositoryResults.Add(repo);
            }
        }

        if (IncludePullRequests)
        {
            foreach (var pr in searchResults.PullRequests)
            {
                PullRequestResults.Add(pr);
            }
        }

        if (IncludeComments)
        {
            foreach (var comment in searchResults.Comments)
            {
                CommentResults.Add(comment);
            }
        }
    }

    /// <summary>
    /// Filter results based on selected tab
    /// </summary>
    partial void OnSelectedTabChanged(string value)
    {
        if (_lastSearchResults == null) return;

        // This would typically filter the display results based on tab
        // For now, the tab selection affects what's shown in the UI binding
        _logger.LogDebug("Search tab changed to: {Tab}", value);
    }
}
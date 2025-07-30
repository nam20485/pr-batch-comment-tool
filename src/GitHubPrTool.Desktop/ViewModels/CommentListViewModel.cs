using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for the comment list view with advanced filtering capabilities.
/// </summary>
public partial class CommentListViewModel : ObservableObject
{
    private readonly IGitHubRepository _gitHubRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly ILogger<CommentListViewModel> _logger;
    private PullRequest? _currentPullRequest;
    private List<Comment> _allComments = new();

    [ObservableProperty]
    private ObservableCollection<Comment> _comments = new();

    [ObservableProperty]
    private ObservableCollection<Comment> _selectedComments = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _selectedTypeFilter = "All";

    [ObservableProperty]
    private string _selectedAuthorFilter = "All";

    [ObservableProperty]
    private string _sortBy = "Created";

    [ObservableProperty]
    private bool _sortDescending = false;

    [ObservableProperty]
    private bool _showSelection = false;

    [ObservableProperty]
    private string _pullRequestTitle = "No Pull Request Selected";

    [ObservableProperty]
    private DateTime? _dateFromFilter;

    [ObservableProperty]
    private DateTime? _dateToFilter;

    /// <summary>
    /// Available comment type filters.
    /// </summary>
    public ObservableCollection<string> AvailableTypeFilters { get; } = new() 
    { 
        "All", "Issue", "Review", "Commit" 
    };

    /// <summary>
    /// Available sort options.
    /// </summary>
    public ObservableCollection<string> AvailableSortOptions { get; } = new() 
    { 
        "Created", "Updated", "Author", "Type" 
    };

    /// <summary>
    /// Available authors for filtering (dynamically populated).
    /// </summary>
    public ObservableCollection<string> AvailableAuthors { get; } = new() { "All" };

    /// <summary>
    /// Initializes a new instance of the CommentListViewModel.
    /// </summary>
    /// <param name="gitHubRepository">GitHub repository service.</param>
    /// <param name="dataSyncService">Data synchronization service.</param>
    /// <param name="logger">Logger for this view model.</param>
    public CommentListViewModel(
        IGitHubRepository gitHubRepository,
        IDataSyncService dataSyncService,
        ILogger<CommentListViewModel> logger)
    {
        _gitHubRepository = gitHubRepository;
        _dataSyncService = dataSyncService;
        _logger = logger;

        // Set up property change handlers for filtering
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SearchText) 
                or nameof(SelectedTypeFilter) 
                or nameof(SelectedAuthorFilter) 
                or nameof(SortBy) 
                or nameof(SortDescending)
                or nameof(DateFromFilter)
                or nameof(DateToFilter))
            {
                FilterAndSortComments();
            }
        };
    }

    /// <summary>
    /// Loads comments for the specified pull request.
    /// </summary>
    /// <param name="pullRequest">Pull request to load comments for.</param>
    public async Task LoadCommentsAsync(PullRequest pullRequest)
    {
        if (pullRequest == null)
        {
            _logger.LogWarning("Attempted to load comments for null pull request");
            return;
        }

        _currentPullRequest = pullRequest;
        PullRequestTitle = $"PR #{pullRequest.Number}: {pullRequest.Title}";
        
        IsLoading = true;
        StatusMessage = $"Loading comments for PR #{pullRequest.Number}...";

        try
        {
            _logger.LogInformation("Loading comments for PR #{Number} in repository {Repository}", 
                pullRequest.Number, pullRequest.Repository.FullName);

            var comments = await _gitHubRepository.GetCommentsAsync(pullRequest.Id);
            
            _allComments = comments.ToList();
            UpdateAvailableAuthors();
            FilterAndSortComments();

            StatusMessage = $"Loaded {_allComments.Count} comments for PR #{pullRequest.Number}";
            _logger.LogInformation("Successfully loaded {Count} comments for PR", _allComments.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading comments: {ex.Message}";
            _logger.LogError(ex, "Error loading comments for PR #{Number}", pullRequest.Number);
            _allComments.Clear();
            Comments.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Updates the available authors list based on loaded comments.
    /// </summary>
    private void UpdateAvailableAuthors()
    {
        var authors = _allComments
            .Select(c => c.Author.Login)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        AvailableAuthors.Clear();
        AvailableAuthors.Add("All");
        foreach (var author in authors)
        {
            AvailableAuthors.Add(author);
        }

        // Reset author filter if current selection is no longer available
        if (!AvailableAuthors.Contains(SelectedAuthorFilter))
        {
            SelectedAuthorFilter = "All";
        }
    }

    /// <summary>
    /// Filters and sorts comments based on current criteria.
    /// </summary>
    private void FilterAndSortComments()
    {
        var filtered = _allComments.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c => 
                c.Body.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Author.Login.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply type filter
        if (SelectedTypeFilter != "All" && Enum.TryParse<CommentType>(SelectedTypeFilter, out var type))
        {
            filtered = filtered.Where(c => c.Type == type);
        }

        // Apply author filter
        if (SelectedAuthorFilter != "All")
        {
            filtered = filtered.Where(c => c.Author.Login == SelectedAuthorFilter);
        }

        // Apply date filters
        if (DateFromFilter.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt >= DateFromFilter.Value);
        }

        if (DateToFilter.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt <= DateToFilter.Value.AddDays(1));
        }

        // Apply sorting
        filtered = SortBy switch
        {
            "Created" => SortDescending 
                ? filtered.OrderByDescending(c => c.CreatedAt)
                : filtered.OrderBy(c => c.CreatedAt),
            "Updated" => SortDescending
                ? filtered.OrderByDescending(c => c.UpdatedAt)
                : filtered.OrderBy(c => c.UpdatedAt),
            "Author" => SortDescending
                ? filtered.OrderByDescending(c => c.Author.Login)
                : filtered.OrderBy(c => c.Author.Login),
            "Type" => SortDescending
                ? filtered.OrderByDescending(c => c.Type.ToString())
                : filtered.OrderBy(c => c.Type.ToString()),
            _ => filtered.OrderBy(c => c.CreatedAt)
        };

        // Update the observable collection
        Comments.Clear();
        foreach (var comment in filtered)
        {
            Comments.Add(comment);
        }

        UpdateSelectionStatus();
    }

    /// <summary>
    /// Command to refresh comments data.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_currentPullRequest == null) return;

        IsLoading = true;
        StatusMessage = "Refreshing comments...";

        try
        {
            // Sync comments for this pull request
            await _dataSyncService.SyncCommentsAsync(
                _currentPullRequest.RepositoryId, 
                _currentPullRequest.Number, 
                forceRefresh: true);
            
            // Reload comments
            await LoadCommentsAsync(_currentPullRequest);
            
            StatusMessage = "Comments refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing comments: {ex.Message}";
            _logger.LogError(ex, "Error refreshing comments");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to clear all filters.
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedTypeFilter = "All";
        SelectedAuthorFilter = "All";
        DateFromFilter = null;
        DateToFilter = null;
        SortBy = "Created";
        SortDescending = false;
        
        StatusMessage = "Filters cleared";
        _logger.LogInformation("All comment filters cleared");
    }

    /// <summary>
    /// Command to toggle comment selection mode.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectionMode()
    {
        ShowSelection = !ShowSelection;
        
        if (!ShowSelection)
        {
            SelectedComments.Clear();
        }
        
        StatusMessage = ShowSelection ? "Selection mode enabled" : "Selection mode disabled";
        _logger.LogInformation("Comment selection mode: {Enabled}", ShowSelection);
    }

    /// <summary>
    /// Command to select all visible comments.
    /// </summary>
    [RelayCommand]
    private void SelectAllComments()
    {
        SelectedComments.Clear();
        foreach (var comment in Comments)
        {
            SelectedComments.Add(comment);
        }
        
        UpdateSelectionStatus();
        _logger.LogInformation("Selected all {Count} visible comments", Comments.Count);
    }

    /// <summary>
    /// Command to clear all selected comments.
    /// </summary>
    [RelayCommand]
    private void ClearSelection()
    {
        SelectedComments.Clear();
        UpdateSelectionStatus();
        _logger.LogInformation("Cleared comment selection");
    }

    /// <summary>
    /// Toggles selection state of a comment.
    /// </summary>
    /// <param name="comment">Comment to toggle selection for.</param>
    public void ToggleCommentSelection(Comment comment)
    {
        if (SelectedComments.Contains(comment))
        {
            SelectedComments.Remove(comment);
        }
        else
        {
            SelectedComments.Add(comment);
        }
        
        UpdateSelectionStatus();
        _logger.LogDebug("Toggled selection for comment {Id}", comment.Id);
    }

    /// <summary>
    /// Command to toggle selection state of a comment.
    /// </summary>
    /// <param name="comment">Comment to toggle selection for.</param>
    [RelayCommand]
    private void ToggleCommentSelectionCommand(Comment comment)
    {
        ToggleCommentSelection(comment);
    }

    /// <summary>
    /// Checks if a comment is currently selected.
    /// </summary>
    /// <param name="comment">Comment to check.</param>
    /// <returns>True if the comment is selected.</returns>
    public bool IsCommentSelected(Comment comment)
    {
        return SelectedComments.Contains(comment);
    }

    /// <summary>
    /// Command to duplicate selected comments (placeholder for batch operations).
    /// </summary>
    [RelayCommand]
    private void DuplicateSelectedComments()
    {
        if (SelectedComments.Count == 0)
        {
            StatusMessage = "No comments selected for duplication";
            return;
        }

        StatusMessage = $"Duplicating {SelectedComments.Count} selected comments...";
        _logger.LogInformation("Duplicating {Count} selected comments", SelectedComments.Count);
        
        foreach (var comment in SelectedComments)
        {
            var duplicatedComment = new Comment
            {
                Id = Random.Shared.NextInt64(1000000, 9999999), // Generate a unique ID for the duplicated comment
                Author = comment.Author,
                Body = comment.Body,
                Type = comment.Type,
                PullRequest = comment.PullRequest,
                PullRequestId = comment.PullRequestId,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
            Comments.Add(duplicatedComment);
        }

        StatusMessage = $"{SelectedComments.Count} comments duplicated successfully.";
        _logger.LogInformation("{Count} comments duplicated successfully", SelectedComments.Count);
    }

    /// <summary>
    /// Updates the status message with selection information.
    /// </summary>
    private void UpdateSelectionStatus()
    {
        if (ShowSelection && SelectedComments.Count > 0)
        {
            StatusMessage = $"{SelectedComments.Count} of {Comments.Count} comments selected";
        }
        else if (Comments.Count > 0)
        {
            StatusMessage = $"Showing {Comments.Count} of {_allComments.Count} comments";
        }
    }

    /// <summary>
    /// Clears the current comment data.
    /// </summary>
    public void Clear()
    {
        _currentPullRequest = null;
        _allComments.Clear();
        Comments.Clear();
        SelectedComments.Clear();
        PullRequestTitle = "No Pull Request Selected";
        AvailableAuthors.Clear();
        AvailableAuthors.Add("All");
        ClearFilters();
        StatusMessage = "Ready";
        _logger.LogDebug("Cleared comment list view");
    }
}
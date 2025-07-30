using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for the detailed pull request view with comprehensive metadata display.
/// </summary>
public partial class PullRequestDetailViewModel : ObservableObject
{
    private readonly IGitHubRepository _gitHubRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly ILogger<PullRequestDetailViewModel> _logger;

    [ObservableProperty]
    private PullRequest? _pullRequest;

    [ObservableProperty]
    private ObservableCollection<Comment> _comments = new();

    [ObservableProperty]
    private ObservableCollection<Review> _reviews = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _selectedTab = "Overview";

    [ObservableProperty]
    private bool _showComments = true;

    [ObservableProperty]
    private bool _showReviews = true;

    /// <summary>
    /// Available tabs for the detail view.
    /// </summary>
    public ObservableCollection<string> AvailableTabs { get; } = new() { "Overview", "Comments", "Reviews", "Files" };

    /// <summary>
    /// Initializes a new instance of the PullRequestDetailViewModel.
    /// </summary>
    /// <param name="gitHubRepository">GitHub repository service.</param>
    /// <param name="dataSyncService">Data synchronization service.</param>
    /// <param name="logger">Logger for this view model.</param>
    public PullRequestDetailViewModel(
        IGitHubRepository gitHubRepository,
        IDataSyncService dataSyncService,
        ILogger<PullRequestDetailViewModel> logger)
    {
        _gitHubRepository = gitHubRepository;
        _dataSyncService = dataSyncService;
        _logger = logger;
    }

    /// <summary>
    /// Loads detailed information for the specified pull request.
    /// </summary>
    /// <param name="pullRequest">Pull request to load details for.</param>
    public async Task LoadPullRequestDetailAsync(PullRequest pullRequest)
    {
        if (pullRequest == null)
        {
            _logger.LogWarning("Attempted to load details for null pull request");
            return;
        }

        IsLoading = true;
        StatusMessage = $"Loading details for PR #{pullRequest.Number}...";

        try
        {
            _logger.LogInformation("Loading details for PR #{Number} in repository {Repository}", 
                pullRequest.Number, pullRequest.Repository.FullName);

            // Load the full PR details
            var detailedPr = await _gitHubRepository.GetPullRequestAsync(
                pullRequest.RepositoryId, pullRequest.Number);

            if (detailedPr != null)
            {
                PullRequest = detailedPr;
            }
            else
            {
                PullRequest = pullRequest; // Fall back to the provided PR
                _logger.LogWarning("Could not load detailed PR information, using cached data");
            }

            // Load comments and reviews in parallel
            var commentsTask = LoadCommentsAsync(pullRequest.Id);
            var reviewsTask = LoadReviewsAsync(pullRequest.Id);

            await Task.WhenAll(commentsTask, reviewsTask);

            StatusMessage = $"Loaded PR #{pullRequest.Number} with {Comments.Count} comments and {Reviews.Count} reviews";
            _logger.LogInformation("Successfully loaded PR details: {CommentCount} comments, {ReviewCount} reviews",
                Comments.Count, Reviews.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading PR details: {ex.Message}";
            _logger.LogError(ex, "Error loading details for PR #{Number}", pullRequest.Number);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads comments for the current pull request.
    /// </summary>
    private async Task LoadCommentsAsync(long pullRequestId)
    {
        try
        {
            var comments = await _gitHubRepository.GetCommentsAsync(pullRequestId);
            
            Comments.Clear();
            foreach (var comment in comments.OrderBy(c => c.CreatedAt))
            {
                Comments.Add(comment);
            }

            _logger.LogDebug("Loaded {Count} comments for PR", Comments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comments for PR {Id}", pullRequestId);
        }
    }

    /// <summary>
    /// Loads reviews for the current pull request.
    /// </summary>
    private async Task LoadReviewsAsync(long pullRequestId)
    {
        try
        {
            var reviews = await _gitHubRepository.GetReviewsAsync(pullRequestId);
            
            Reviews.Clear();
            foreach (var review in reviews.OrderBy(r => r.SubmittedAt))
            {
                Reviews.Add(review);
            }

            _logger.LogDebug("Loaded {Count} reviews for PR", Reviews.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews for PR {Id}", pullRequestId);
        }
    }

    /// <summary>
    /// Command to refresh the pull request data.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (PullRequest == null) return;

        IsLoading = true;
        StatusMessage = "Refreshing pull request data...";

        try
        {
            // Sync the pull requests for this repository
            await _dataSyncService.SyncPullRequestsAsync(PullRequest.RepositoryId, forceRefresh: true);
            
            // Reload the current PR
            await LoadPullRequestDetailAsync(PullRequest);
            
            StatusMessage = "Pull request data refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing data: {ex.Message}";
            _logger.LogError(ex, "Error refreshing PR data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to open the pull request in browser.
    /// </summary>
    [RelayCommand]
    private void OpenInBrowser()
    {
        if (PullRequest?.HtmlUrl == null)
        {
            StatusMessage = "Cannot open pull request - URL not available";
            return;
        }

        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = PullRequest.HtmlUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processInfo);
            
            StatusMessage = $"Opened PR #{PullRequest.Number} in browser";
            _logger.LogInformation("Opened PR in browser: {URL}", PullRequest.HtmlUrl);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening browser: {ex.Message}";
            _logger.LogError(ex, "Error opening PR in browser: {URL}", PullRequest.HtmlUrl);
        }
    }

    /// <summary>
    /// Command to view comments for this pull request.
    /// </summary>
    [RelayCommand]
    private void ViewComments()
    {
        SelectedTab = "Comments";
        StatusMessage = $"Viewing {Comments.Count} comments";
    }

    /// <summary>
    /// Command to view reviews for this pull request.
    /// </summary>
    [RelayCommand]
    private void ViewReviews()
    {
        SelectedTab = "Reviews";
        StatusMessage = $"Viewing {Reviews.Count} reviews";
    }

    /// <summary>
    /// Clears the current pull request data.
    /// </summary>
    public void Clear()
    {
        PullRequest = null;
        Comments.Clear();
        Reviews.Clear();
        SelectedTab = "Overview";
        StatusMessage = "Ready";
        _logger.LogDebug("Cleared pull request detail view");
    }
}
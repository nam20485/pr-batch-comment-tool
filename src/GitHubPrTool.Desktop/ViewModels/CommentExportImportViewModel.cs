using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// ViewModel for comment export and import functionality
/// </summary>
public partial class CommentExportImportViewModel : ObservableObject
{
    private readonly ICommentExportImportService _exportImportService;
    private readonly IGitHubRepository _repository;
    private readonly ILogger<CommentExportImportViewModel> _logger;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private bool _isImporting;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private ExportFormat _selectedExportFormat = ExportFormat.Json;

    [ObservableProperty]
    private bool _includeMetadata = true;

    [ObservableProperty]
    private string _exportFilePath = "";

    [ObservableProperty]
    private string _importFilePath = "";

    [ObservableProperty]
    private bool _validateOnImport = true;

    [ObservableProperty]
    private long _selectedRepositoryId;

    [ObservableProperty]
    private long _selectedPullRequestId;

    [ObservableProperty]
    private int _exportedCount;

    [ObservableProperty]
    private int _importedCount;

    [ObservableProperty]
    private int _failedCount;

    /// <summary>
    /// Available export formats
    /// </summary>
    public ObservableCollection<ExportFormat> AvailableFormats { get; }

    /// <summary>
    /// Available repositories for selection
    /// </summary>
    public ObservableCollection<Repository> AvailableRepositories { get; } = new();

    /// <summary>
    /// Available pull requests for the selected repository
    /// </summary>
    public ObservableCollection<PullRequest> AvailablePullRequests { get; } = new();

    /// <summary>
    /// Comments available for export
    /// </summary>
    public ObservableCollection<Comment> AvailableComments { get; } = new();

    /// <summary>
    /// Selected comments for export
    /// </summary>
    public ObservableCollection<Comment> SelectedComments { get; } = new();

    /// <summary>
    /// Import validation results
    /// </summary>
    public ObservableCollection<string> ValidationErrors { get; } = new();

    /// <summary>
    /// Import validation warnings
    /// </summary>
    public ObservableCollection<string> ValidationWarnings { get; } = new();

    /// <summary>
    /// Import messages
    /// </summary>
    public ObservableCollection<string> ImportMessages { get; } = new();

    /// <summary>
    /// Initializes a new instance of the CommentExportImportViewModel
    /// </summary>
    /// <param name="exportImportService">Export/import service</param>
    /// <param name="repository">GitHub repository service</param>
    /// <param name="logger">Logger</param>
    public CommentExportImportViewModel(
        ICommentExportImportService exportImportService,
        IGitHubRepository repository,
        ILogger<CommentExportImportViewModel> logger)
    {
        _exportImportService = exportImportService ?? throw new ArgumentNullException(nameof(exportImportService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        AvailableFormats = new ObservableCollection<ExportFormat>(_exportImportService.GetSupportedFormats());

        // Load initial data
        _ = Task.Run(LoadRepositoriesAsync);

        // Set up property change handlers
        PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedRepositoryId))
            {
                await LoadPullRequestsAsync();
            }
            else if (e.PropertyName == nameof(SelectedPullRequestId))
            {
                await LoadCommentsAsync();
            }
        };
    }

    /// <summary>
    /// Command to browse for export file path
    /// </summary>
    [RelayCommand]
    private void BrowseExportFile()
    {
        try
        {
            var extension = SelectedExportFormat == ExportFormat.Json ? ".json" : ".csv";
            var filter = SelectedExportFormat == ExportFormat.Json ? "JSON files (*.json)|*.json" : "CSV files (*.csv)|*.csv";
            
            // For now, use a simple file name generation
            var fileName = $"comments_export_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", fileName);
            
            ExportFilePath = defaultPath;
            StatusMessage = $"Export file path set to: {ExportFilePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error setting export file path: {ex.Message}";
            _logger.LogError(ex, "Error browsing for export file path");
        }
    }

    /// <summary>
    /// Command to browse for import file path
    /// </summary>
    [RelayCommand]
    private void BrowseImportFile()
    {
        try
        {
            // For now, use a simple path - in a real app, this would use a file dialog
            ImportFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "comments_import.json");
            StatusMessage = $"Import file path set to: {ImportFilePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error setting import file path: {ex.Message}";
            _logger.LogError(ex, "Error browsing for import file path");
        }
    }

    /// <summary>
    /// Command to export selected comments
    /// </summary>
    [RelayCommand]
    private async Task ExportCommentsAsync()
    {
        if (SelectedComments.Count == 0)
        {
            StatusMessage = "Please select comments to export";
            return;
        }

        if (string.IsNullOrWhiteSpace(ExportFilePath))
        {
            StatusMessage = "Please specify an export file path";
            return;
        }

        IsExporting = true;
        StatusMessage = "Exporting comments...";

        try
        {
            _logger.LogInformation("Starting export of {Count} comments to {FilePath}", SelectedComments.Count, ExportFilePath);

            await _exportImportService.ExportToFileAsync(
                SelectedComments,
                ExportFilePath,
                SelectedExportFormat,
                IncludeMetadata);

            ExportedCount = SelectedComments.Count;
            StatusMessage = $"Successfully exported {ExportedCount} comments to {ExportFilePath}";
            
            _logger.LogInformation("Export completed successfully. Exported {Count} comments", ExportedCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            _logger.LogError(ex, "Error exporting comments");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Command to import comments from file
    /// </summary>
    [RelayCommand]
    private async Task ImportCommentsAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportFilePath))
        {
            StatusMessage = "Please specify an import file path";
            return;
        }

        if (!File.Exists(ImportFilePath))
        {
            StatusMessage = "Import file does not exist";
            return;
        }

        IsImporting = true;
        StatusMessage = "Importing comments...";
        
        // Clear previous results
        ValidationErrors.Clear();
        ValidationWarnings.Clear();
        ImportMessages.Clear();

        try
        {
            _logger.LogInformation("Starting import from {FilePath}", ImportFilePath);

            var result = await _exportImportService.ImportFromFileAsync(
                ImportFilePath,
                validateData: ValidateOnImport);

            ImportedCount = result.ImportedCount;
            FailedCount = result.FailedCount;

            // Update collections with results
            foreach (var error in result.ValidationResult.Errors)
            {
                ValidationErrors.Add(error);
            }

            foreach (var warning in result.ValidationResult.Warnings)
            {
                ValidationWarnings.Add(warning);
            }

            foreach (var message in result.Messages)
            {
                ImportMessages.Add(message);
            }

            if (result.IsSuccess)
            {
                StatusMessage = $"Successfully imported {ImportedCount} comments";
                _logger.LogInformation("Import completed successfully. Imported {Count} comments", ImportedCount);
            }
            else
            {
                StatusMessage = $"Import completed with errors. Imported: {ImportedCount}, Failed: {FailedCount}";
                _logger.LogWarning("Import completed with errors. Imported: {ImportedCount}, Failed: {FailedCount}", ImportedCount, FailedCount);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import failed: {ex.Message}";
            _logger.LogError(ex, "Error importing comments");
        }
        finally
        {
            IsImporting = false;
        }
    }

    /// <summary>
    /// Command to select all available comments
    /// </summary>
    [RelayCommand]
    private void SelectAllComments()
    {
        SelectedComments.Clear();
        foreach (var comment in AvailableComments)
        {
            SelectedComments.Add(comment);
        }
        StatusMessage = $"Selected all {SelectedComments.Count} comments";
    }

    /// <summary>
    /// Command to clear selected comments
    /// </summary>
    [RelayCommand]
    private void ClearSelectedComments()
    {
        SelectedComments.Clear();
        StatusMessage = "Cleared comment selection";
    }

    /// <summary>
    /// Toggle comment selection
    /// </summary>
    /// <param name="comment">Comment to toggle</param>
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
        
        StatusMessage = $"Selected {SelectedComments.Count} of {AvailableComments.Count} comments";
    }

    /// <summary>
    /// Load available repositories
    /// </summary>
    private async Task LoadRepositoriesAsync()
    {
        try
        {
            var repositories = await _repository.GetRepositoriesAsync();
            
            AvailableRepositories.Clear();
            foreach (var repo in repositories)
            {
                AvailableRepositories.Add(repo);
            }

            StatusMessage = $"Loaded {AvailableRepositories.Count} repositories";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading repositories: {ex.Message}";
            _logger.LogError(ex, "Error loading repositories");
        }
    }

    /// <summary>
    /// Load pull requests for selected repository
    /// </summary>
    private async Task LoadPullRequestsAsync()
    {
        if (SelectedRepositoryId == 0)
        {
            AvailablePullRequests.Clear();
            return;
        }

        try
        {
            var pullRequests = await _repository.GetPullRequestsAsync(SelectedRepositoryId);
            
            AvailablePullRequests.Clear();
            foreach (var pr in pullRequests)
            {
                AvailablePullRequests.Add(pr);
            }

            StatusMessage = $"Loaded {AvailablePullRequests.Count} pull requests";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading pull requests: {ex.Message}";
            _logger.LogError(ex, "Error loading pull requests");
        }
    }

    /// <summary>
    /// Load comments for selected pull request
    /// </summary>
    private async Task LoadCommentsAsync()
    {
        if (SelectedPullRequestId == 0)
        {
            AvailableComments.Clear();
            return;
        }

        try
        {
            var comments = await _repository.GetCommentsAsync(SelectedPullRequestId);
            
            AvailableComments.Clear();
            foreach (var comment in comments)
            {
                AvailableComments.Add(comment);
            }

            StatusMessage = $"Loaded {AvailableComments.Count} comments";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading comments: {ex.Message}";
            _logger.LogError(ex, "Error loading comments");
        }
    }
}
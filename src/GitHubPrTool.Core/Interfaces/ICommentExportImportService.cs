using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for exporting and importing comments in various formats
/// </summary>
public interface ICommentExportImportService
{
    /// <summary>
    /// Export comments to JSON format
    /// </summary>
    /// <param name="comments">Comments to export</param>
    /// <param name="includeMetadata">Include additional metadata in export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON string representation of comments</returns>
    Task<string> ExportToJsonAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export comments to CSV format
    /// </summary>
    /// <param name="comments">Comments to export</param>
    /// <param name="includeMetadata">Include additional metadata in export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV string representation of comments</returns>
    Task<string> ExportToCsvAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export comments to a file
    /// </summary>
    /// <param name="comments">Comments to export</param>
    /// <param name="filePath">File path for export</param>
    /// <param name="format">Export format</param>
    /// <param name="includeMetadata">Include additional metadata in export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportToFileAsync(IEnumerable<Comment> comments, string filePath, ExportFormat format, bool includeMetadata = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import comments from JSON format
    /// </summary>
    /// <param name="jsonData">JSON string containing comments</param>
    /// <param name="validateData">Validate imported data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Imported comments and validation results</returns>
    Task<ImportResult> ImportFromJsonAsync(string jsonData, bool validateData = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import comments from CSV format
    /// </summary>
    /// <param name="csvData">CSV string containing comments</param>
    /// <param name="validateData">Validate imported data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Imported comments and validation results</returns>
    Task<ImportResult> ImportFromCsvAsync(string csvData, bool validateData = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import comments from a file
    /// </summary>
    /// <param name="filePath">File path to import from</param>
    /// <param name="format">Import format (auto-detected if not specified)</param>
    /// <param name="validateData">Validate imported data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Imported comments and validation results</returns>
    Task<ImportResult> ImportFromFileAsync(string filePath, ExportFormat? format = null, bool validateData = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate comment data for import
    /// </summary>
    /// <param name="comments">Comments to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation results</returns>
    Task<ValidationResult> ValidateCommentsAsync(IEnumerable<Comment> comments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported export formats
    /// </summary>
    /// <returns>List of supported export formats</returns>
    IEnumerable<ExportFormat> GetSupportedFormats();
}

/// <summary>
/// Export/Import format options
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON format
    /// </summary>
    Json,

    /// <summary>
    /// CSV format
    /// </summary>
    Csv
}

/// <summary>
/// Result of import operation
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Successfully imported comments
    /// </summary>
    public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Number of comments imported
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Number of comments that failed to import
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Validation result
    /// </summary>
    public ValidationResult ValidationResult { get; set; } = new();

    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool IsSuccess => FailedCount == 0 && ValidationResult.IsValid;

    /// <summary>
    /// Import operation messages and errors
    /// </summary>
    public List<string> Messages { get; set; } = new();
}

/// <summary>
/// Result of validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Number of items validated
    /// </summary>
    public int ValidatedCount { get; set; }
}
using System.Globalization;
using System.Text;
using System.Text.Json;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Service for exporting and importing comments in various formats
/// </summary>
public class CommentExportImportService : ICommentExportImportService
{
    private readonly ILogger<CommentExportImportService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CommentExportImportService(ILogger<CommentExportImportService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public Task<string> ExportToJsonAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting {Count} comments to JSON format", comments.Count());

            var exportData = new CommentExportData
            {
                ExportedAt = DateTimeOffset.UtcNow,
                ExportedBy = Environment.UserName,
                Version = "1.0",
                IncludeMetadata = includeMetadata,
                Comments = comments.Select(c => MapToExportComment(c, includeMetadata)).ToList()
            };

            var json = JsonSerializer.Serialize(exportData, _jsonOptions);
            _logger.LogInformation("Successfully exported {Count} comments to JSON", comments.Count());

            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting comments to JSON");
            throw;
        }
    }

    public Task<string> ExportToCsvAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting {Count} comments to CSV format", comments.Count());

            var csv = new StringBuilder();

            // Add header
            var headers = new List<string>
            {
                "Id", "Body", "Author", "Type", "CreatedAt", "UpdatedAt", "HtmlUrl"
            };

            if (includeMetadata)
            {
                headers.AddRange(new[] { "PullRequestId", "Path", "Position", "DiffHunk" });
            }

            csv.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

            // Add data rows
            foreach (var comment in comments)
            {
                var values = new List<string>
                {
                    comment.Id.ToString(),
                    EscapeCsvValue(comment.Body),
                    EscapeCsvValue(comment.Author.Login),
                    comment.Type.ToString(),
                    comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    comment.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    EscapeCsvValue(comment.HtmlUrl)
                };

                if (includeMetadata)
                {
                    values.AddRange(new[]
                    {
                        comment.PullRequestId.ToString(),
                        EscapeCsvValue(comment.Path),
                        comment.Position?.ToString() ?? "",
                        EscapeCsvValue(comment.DiffHunk)
                    });
                }

                csv.AppendLine(string.Join(",", values));
            }

            _logger.LogInformation("Successfully exported {Count} comments to CSV", comments.Count());
            return Task.FromResult(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting comments to CSV");
            throw;
        }
    }

    public async Task ExportToFileAsync(IEnumerable<Comment> comments, string filePath, ExportFormat format, bool includeMetadata = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting {Count} comments to file: {FilePath} (Format: {Format})", comments.Count(), filePath, format);

            string content = format switch
            {
                ExportFormat.Json => await ExportToJsonAsync(comments, includeMetadata, cancellationToken),
                ExportFormat.Csv => await ExportToCsvAsync(comments, includeMetadata, cancellationToken),
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken);
            _logger.LogInformation("Successfully exported comments to file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting comments to file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<ImportResult> ImportFromJsonAsync(string jsonData, bool validateData = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Importing comments from JSON data");

            var importData = JsonSerializer.Deserialize<CommentExportData>(jsonData, _jsonOptions);
            if (importData?.Comments == null)
            {
                return new ImportResult
                {
                    ValidationResult = new ValidationResult
                    {
                        IsValid = false,
                        Errors = { "Invalid JSON format or no comments found" }
                    },
                    Messages = { "Failed to parse JSON data" }
                };
            }

            var comments = importData.Comments.Select(MapFromExportComment).ToList();
            var validationResult = validateData ? await ValidateCommentsAsync(comments, cancellationToken) : new ValidationResult { IsValid = true };

            var result = new ImportResult
            {
                Comments = comments,
                ImportedCount = comments.Count,
                FailedCount = validationResult.IsValid ? 0 : validationResult.Errors.Count,
                ValidationResult = validationResult,
                Messages = { $"Imported {comments.Count} comments from JSON" }
            };

            _logger.LogInformation("Successfully imported {Count} comments from JSON", comments.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing comments from JSON");
            return new ImportResult
            {
                ValidationResult = new ValidationResult
                {
                    IsValid = false,
                    Errors = { $"Import failed: {ex.Message}" }
                },
                Messages = { $"Error importing from JSON: {ex.Message}" }
            };
        }
    }

    public async Task<ImportResult> ImportFromCsvAsync(string csvData, bool validateData = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Importing comments from CSV data");

            using var reader = new StringReader(csvData);
            var headerLine = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrEmpty(headerLine))
            {
                return new ImportResult
                {
                    ValidationResult = new ValidationResult
                    {
                        IsValid = false,
                        Errors = { "CSV data must contain at least a header row." }
                    },
                    Messages = { "Invalid CSV format: Missing header." }
                };
            }

            var headers = ParseCsvLine(headerLine);
            var comments = new List<Comment>();
            var errors = new List<string>();
            var lineNumber = 1;

            while (reader.Peek() != -1)
            {
                lineNumber++;
                try
                {
                    var values = await ParseCsvRecordAsync(reader, cancellationToken);
                    if (values.Any())
                    {
                        var comment = ParseCsvComment(headers, values);
                        comments.Add(comment);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error parsing record near line {lineNumber}: {ex.Message}");
                    // Skip to the next line to attempt recovery
                    await reader.ReadLineAsync(cancellationToken);
                }
            }

            var validationResult = validateData ? await ValidateCommentsAsync(comments, cancellationToken) : new ValidationResult { IsValid = true };

            var result = new ImportResult
            {
                Comments = comments,
                ImportedCount = comments.Count,
                FailedCount = errors.Count + (validationResult.IsValid ? 0 : validationResult.Errors.Count),
                ValidationResult = validationResult,
                Messages = { $"Imported {comments.Count} comments from CSV" }
            };

            if (errors.Any())
            {
                result.Messages.AddRange(errors);
            }

            _logger.LogInformation("Successfully imported {Count} comments from CSV", comments.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing comments from CSV");
            return new ImportResult
            {
                ValidationResult = new ValidationResult
                {
                    IsValid = false,
                    Errors = { $"Import failed: {ex.Message}" }
                },
                Messages = { $"Error importing from CSV: {ex.Message}" }
            };
        }
    }

    public async Task<ImportResult> ImportFromFileAsync(string filePath, ExportFormat? format = null, bool validateData = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Importing comments from file: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                return new ImportResult
                {
                    ValidationResult = new ValidationResult
                    {
                        IsValid = false,
                        Errors = { $"File not found: {filePath}" }
                    },
                    Messages = { "File does not exist" }
                };
            }

            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

            // Auto-detect format if not specified
            var detectedFormat = format ?? DetectFormat(filePath, content);

            var result = detectedFormat switch
            {
                ExportFormat.Json => await ImportFromJsonAsync(content, validateData, cancellationToken),
                ExportFormat.Csv => await ImportFromCsvAsync(content, validateData, cancellationToken),
                _ => throw new ArgumentException($"Unsupported import format: {detectedFormat}")
            };

            result.Messages.Insert(0, $"Imported from file: {filePath}");
            _logger.LogInformation("Successfully imported comments from file: {FilePath}", filePath);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing comments from file: {FilePath}", filePath);
            return new ImportResult
            {
                ValidationResult = new ValidationResult
                {
                    IsValid = false,
                    Errors = { $"Import failed: {ex.Message}" }
                },
                Messages = { $"Error importing from file: {ex.Message}" }
            };
        }
    }

    public Task<ValidationResult> ValidateCommentsAsync(IEnumerable<Comment> comments, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();
        var commentList = comments.ToList();
        result.ValidatedCount = commentList.Count;

        foreach (var comment in commentList)
        {
            // Validate required fields
            if (comment.Id <= 0)
                result.Errors.Add($"Comment has invalid ID: {comment.Id}");

            if (string.IsNullOrWhiteSpace(comment.Body))
                result.Errors.Add($"Comment {comment.Id} has empty body");

            // Validate author - check ID if available, otherwise check Login
            if (comment.Author == null)
            {
                result.Errors.Add($"Comment {comment.Id} has no author");
            }
            else if (comment.Author.Id > 0)
            {
                // If ID is set, it should be valid (this handles JSON imports with full user data)
                // No additional validation needed as ID > 0 is already valid
            }
            else if (string.IsNullOrWhiteSpace(comment.Author.Login))
            {
                // If ID is not set or is 0, Login must be present (this handles CSV imports)
                result.Errors.Add($"Comment {comment.Id} has invalid author - both ID and Login are missing");
            }

            if (comment.CreatedAt == default)
                result.Errors.Add($"Comment {comment.Id} has invalid creation date");

            // Validate enum values
            if (!Enum.IsDefined(typeof(CommentType), comment.Type))
                result.Errors.Add($"Comment {comment.Id} has invalid type: {comment.Type}");

            // Warnings for optional fields
            if (string.IsNullOrWhiteSpace(comment.HtmlUrl))
                result.Warnings.Add($"Comment {comment.Id} has no HTML URL");
        }

        result.IsValid = !result.Errors.Any();
        return Task.FromResult(result);
    }

    public IEnumerable<ExportFormat> GetSupportedFormats()
    {
        return Enum.GetValues<ExportFormat>();
    }

    private static ExportFormat DetectFormat(string filePath, string content)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".json" => ExportFormat.Json,
            ".csv" => ExportFormat.Csv,
            _ => content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[")
                ? ExportFormat.Json
                : ExportFormat.Csv
        };
    }

    private static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static async Task<List<string>> ParseCsvRecordAsync(StringReader reader, CancellationToken cancellationToken)
    {
        var line = await reader.ReadLineAsync(cancellationToken);
        if (line == null)
        {
            return new List<string>();
        }

        var builder = new StringBuilder(line);

        // If the line is part of a multi-line field, it will have an odd number of quotes.
        // Keep reading lines until the quote count is even.
        while (builder.ToString().Count(c => c == '"') % 2 != 0)
        {
            var nextLine = await reader.ReadLineAsync(cancellationToken);
            if (nextLine == null)
            {
                // Reached end of stream inside a quoted field, which is a format error.
                // Return what we have, and let the parser logic handle the malformed line.
                break;
            }
            builder.Append('\n').Append(nextLine);
        }

        return ParseCsvLine(builder.ToString());
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private static Comment ParseCsvComment(List<string> headers, List<string> values)
    {
        var comment = new Comment();

        for (int i = 0; i < Math.Min(headers.Count, values.Count); i++)
        {
            var header = headers[i].Trim();
            var value = values[i].Trim();

            switch (header.ToLowerInvariant())
            {
                case "id":
                    if (long.TryParse(value, out var id)) comment.Id = id;
                    break;
                case "body":
                    comment.Body = value;
                    break;
                case "author":
                    comment.Author = new User { Login = value };
                    break;
                case "type":
                    if (Enum.TryParse<CommentType>(value, out var type)) comment.Type = type;
                    break;
                case "createdat":
                    if (DateTimeOffset.TryParse(value, out var createdAt)) comment.CreatedAt = createdAt;
                    break;
                case "updatedat":
                    if (DateTimeOffset.TryParse(value, out var updatedAt)) comment.UpdatedAt = updatedAt;
                    break;
                case "htmlurl":
                    comment.HtmlUrl = value;
                    break;
                case "pullrequestid":
                    if (long.TryParse(value, out var prId)) comment.PullRequestId = prId;
                    break;
                case "path":
                    comment.Path = value;
                    break;
                case "position":
                    if (int.TryParse(value, out var position)) comment.Position = position;
                    break;
                case "diffhunk":
                    comment.DiffHunk = value;
                    break;
            }
        }

        return comment;
    }

    private static ExportComment MapToExportComment(Comment comment, bool includeMetadata)
    {
        var exportComment = new ExportComment
        {
            Id = comment.Id,
            Body = comment.Body,
            Author = comment.Author.Login,
            Type = comment.Type.ToString(),
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            HtmlUrl = comment.HtmlUrl
        };

        if (includeMetadata)
        {
            exportComment.PullRequestId = comment.PullRequestId;
            exportComment.Path = comment.Path;
            exportComment.Position = comment.Position;
            exportComment.DiffHunk = comment.DiffHunk;
        }

        return exportComment;
    }

    private static Comment MapFromExportComment(ExportComment exportComment)
    {
        return new Comment
        {
            Id = exportComment.Id,
            Body = exportComment.Body ?? "",
            Author = new User { Login = exportComment.Author ?? "Unknown" },
            Type = Enum.TryParse<CommentType>(exportComment.Type, out var type) ? type : CommentType.Issue,
            CreatedAt = exportComment.CreatedAt,
            UpdatedAt = exportComment.UpdatedAt,
            HtmlUrl = exportComment.HtmlUrl,
            PullRequestId = exportComment.PullRequestId ?? 0,
            Path = exportComment.Path,
            Position = exportComment.Position,
            DiffHunk = exportComment.DiffHunk
        };
    }

    private class CommentExportData
    {
        public DateTimeOffset ExportedAt { get; set; }
        public string ExportedBy { get; set; } = "";
        public string Version { get; set; } = "";
        public bool IncludeMetadata { get; set; }
        public List<ExportComment> Comments { get; set; } = new();
    }

    private class ExportComment
    {
        public long Id { get; set; }
        public string? Body { get; set; }
        public string? Author { get; set; }
        public string? Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? HtmlUrl { get; set; }
        public long? PullRequestId { get; set; }
        public string? Path { get; set; }
        public int? Position { get; set; }
        public string? DiffHunk { get; set; }
    }
}


// using System.Globalization;
// using System.Text;
// using System.Text.Json;
// using GitHubPrTool.Core.Interfaces;
// using GitHubPrTool.Core.Models;
// using Microsoft.Extensions.Logging;

// namespace GitHubPrTool.Infrastructure.Services;

// /// <summary>
// /// Service for exporting and importing comments in various formats
// /// </summary>
// public class CommentExportImportService : ICommentExportImportService
// {
//     private readonly ILogger<CommentExportImportService> _logger;
//     private readonly JsonSerializerOptions _jsonOptions;

//     public CommentExportImportService(ILogger<CommentExportImportService> logger)
//     {
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _jsonOptions = new JsonSerializerOptions
//         {
//             WriteIndented = true,
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//         };
//     }

//     public Task<string> ExportToJsonAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Exporting {Count} comments to JSON format", comments.Count());

//             var exportData = new CommentExportData
//             {
//                 ExportedAt = DateTimeOffset.UtcNow,
//                 ExportedBy = Environment.UserName,
//                 Version = "1.0",
//                 IncludeMetadata = includeMetadata,
//                 Comments = comments.Select(c => MapToExportComment(c, includeMetadata)).ToList()
//             };

//             var json = JsonSerializer.Serialize(exportData, _jsonOptions);
//             _logger.LogInformation("Successfully exported {Count} comments to JSON", comments.Count());

//             return Task.FromResult(json);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error exporting comments to JSON");
//             throw;
//         }
//     }

//     public Task<string> ExportToCsvAsync(IEnumerable<Comment> comments, bool includeMetadata = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Exporting {Count} comments to CSV format", comments.Count());

//             var csv = new StringBuilder();
            
//             // Add header
//             var headers = new List<string>
//             {
//                 "Id", "Body", "Author", "Type", "CreatedAt", "UpdatedAt", "HtmlUrl"
//             };

//             if (includeMetadata)
//             {
//                 headers.AddRange(new[] { "PullRequestId", "Path", "Position", "DiffHunk" });
//             }

//             csv.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

//             // Add data rows
//             foreach (var comment in comments)
//             {
//                 var values = new List<string>
//                 {
//                     comment.Id.ToString(),
//                     EscapeCsvValue(comment.Body),
//                     EscapeCsvValue(comment.Author.Login),
//                     comment.Type.ToString(),
//                     comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
//                     comment.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
//                     EscapeCsvValue(comment.HtmlUrl)
//                 };

//                 if (includeMetadata)
//                 {
//                     values.AddRange(new[]
//                     {
//                         comment.PullRequestId.ToString(),
//                         EscapeCsvValue(comment.Path),
//                         comment.Position?.ToString() ?? "",
//                         EscapeCsvValue(comment.DiffHunk)
//                     });
//                 }

//                 csv.AppendLine(string.Join(",", values));
//             }

//             _logger.LogInformation("Successfully exported {Count} comments to CSV", comments.Count());
//             return Task.FromResult(csv.ToString());
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error exporting comments to CSV");
//             throw;
//         }
//     }

//     public async Task ExportToFileAsync(IEnumerable<Comment> comments, string filePath, ExportFormat format, bool includeMetadata = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Exporting {Count} comments to file: {FilePath} (Format: {Format})", comments.Count(), filePath, format);

//             string content = format switch
//             {
//                 ExportFormat.Json => await ExportToJsonAsync(comments, includeMetadata, cancellationToken),
//                 ExportFormat.Csv => await ExportToCsvAsync(comments, includeMetadata, cancellationToken),
//                 _ => throw new ArgumentException($"Unsupported export format: {format}")
//             };

//             await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken);
//             _logger.LogInformation("Successfully exported comments to file: {FilePath}", filePath);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error exporting comments to file: {FilePath}", filePath);
//             throw;
//         }
//     }

//     public async Task<ImportResult> ImportFromJsonAsync(string jsonData, bool validateData = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Importing comments from JSON data");

//             var importData = JsonSerializer.Deserialize<CommentExportData>(jsonData, _jsonOptions);
//             if (importData?.Comments == null)
//             {
//                 return new ImportResult
//                 {
//                     ValidationResult = new ValidationResult
//                     {
//                         IsValid = false,
//                         Errors = { "Invalid JSON format or no comments found" }
//                     },
//                     Messages = { "Failed to parse JSON data" }
//                 };
//             }

//             var comments = importData.Comments.Select(MapFromExportComment).ToList();
//             var validationResult = validateData ? await ValidateCommentsAsync(comments, cancellationToken) : new ValidationResult { IsValid = true };

//             var result = new ImportResult
//             {
//                 Comments = comments,
//                 ImportedCount = comments.Count,
//                 FailedCount = validationResult.IsValid ? 0 : validationResult.Errors.Count,
//                 ValidationResult = validationResult,
//                 Messages = { $"Imported {comments.Count} comments from JSON" }
//             };

//             _logger.LogInformation("Successfully imported {Count} comments from JSON", comments.Count);
//             return result;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error importing comments from JSON");
//             return new ImportResult
//             {
//                 ValidationResult = new ValidationResult
//                 {
//                     IsValid = false,
//                     Errors = { $"Import failed: {ex.Message}" }
//                 },
//                 Messages = { $"Error importing from JSON: {ex.Message}" }
//             };
//         }
//     }

//     public async Task<ImportResult> ImportFromCsvAsync(string csvData, bool validateData = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Importing comments from CSV data");

//             var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
//             if (lines.Length < 2)
//             {
//                 return new ImportResult
//                 {
//                     ValidationResult = new ValidationResult
//                     {
//                         IsValid = false,
//                         Errors = { "CSV data must contain at least a header and one data row" }
//                     },
//                     Messages = { "Invalid CSV format" }
//                 };
//             }

//             var headers = ParseCsvLine(lines[0]);
//             var comments = new List<Comment>();
//             var errors = new List<string>();

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 try
//                 {
//                     var values = ParseCsvLine(lines[i]);
//                     var comment = ParseCsvComment(headers, values);
//                     comments.Add(comment);
//                 }
//                 catch (Exception ex)
//                 {
//                     errors.Add($"Error parsing line {i + 1}: {ex.Message}");
//                 }
//             }

//             var validationResult = validateData ? await ValidateCommentsAsync(comments, cancellationToken) : new ValidationResult { IsValid = true };
            
//             var result = new ImportResult
//             {
//                 Comments = comments,
//                 ImportedCount = comments.Count,
//                 FailedCount = errors.Count + (validationResult.IsValid ? 0 : validationResult.Errors.Count),
//                 ValidationResult = validationResult,
//                 Messages = { $"Imported {comments.Count} comments from CSV" }
//             };

//             if (errors.Any())
//             {
//                 result.Messages.AddRange(errors);
//             }

//             _logger.LogInformation("Successfully imported {Count} comments from CSV", comments.Count);
//             return result;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error importing comments from CSV");
//             return new ImportResult
//             {
//                 ValidationResult = new ValidationResult
//                 {
//                     IsValid = false,
//                     Errors = { $"Import failed: {ex.Message}" }
//                 },
//                 Messages = { $"Error importing from CSV: {ex.Message}" }
//             };
//         }
//     }

//     public async Task<ImportResult> ImportFromFileAsync(string filePath, ExportFormat? format = null, bool validateData = true, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             _logger.LogInformation("Importing comments from file: {FilePath}", filePath);

//             if (!File.Exists(filePath))
//             {
//                 return new ImportResult
//                 {
//                     ValidationResult = new ValidationResult
//                     {
//                         IsValid = false,
//                         Errors = { $"File not found: {filePath}" }
//                     },
//                     Messages = { "File does not exist" }
//                 };
//             }

//             var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            
//             // Auto-detect format if not specified
//             var detectedFormat = format ?? DetectFormat(filePath, content);

//             var result = detectedFormat switch
//             {
//                 ExportFormat.Json => await ImportFromJsonAsync(content, validateData, cancellationToken),
//                 ExportFormat.Csv => await ImportFromCsvAsync(content, validateData, cancellationToken),
//                 _ => throw new ArgumentException($"Unsupported import format: {detectedFormat}")
//             };

//             result.Messages.Insert(0, $"Imported from file: {filePath}");
//             _logger.LogInformation("Successfully imported comments from file: {FilePath}", filePath);

//             return result;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error importing comments from file: {FilePath}", filePath);
//             return new ImportResult
//             {
//                 ValidationResult = new ValidationResult
//                 {
//                     IsValid = false,
//                     Errors = { $"Import failed: {ex.Message}" }
//                 },
//                 Messages = { $"Error importing from file: {ex.Message}" }
//             };
//         }
//     }

//     public Task<ValidationResult> ValidateCommentsAsync(IEnumerable<Comment> comments, CancellationToken cancellationToken = default)
//     {
//         var result = new ValidationResult();
//         var commentList = comments.ToList();
//         result.ValidatedCount = commentList.Count;

//         foreach (var comment in commentList)
//         {
//             // Validate required fields
//             if (comment.Id <= 0)
//                 result.Errors.Add($"Comment has invalid ID: {comment.Id}");

//             if (string.IsNullOrWhiteSpace(comment.Body))
//                 result.Errors.Add($"Comment {comment.Id} has empty body");

//             if (comment.Author?.Id <= 0)
//                 result.Errors.Add($"Comment {comment.Id} has invalid author");

//             if (comment.CreatedAt == default)
//                 result.Errors.Add($"Comment {comment.Id} has invalid creation date");

//             // Validate enum values
//             if (!Enum.IsDefined(typeof(CommentType), comment.Type))
//                 result.Errors.Add($"Comment {comment.Id} has invalid type: {comment.Type}");

//             // Warnings for optional fields
//             if (string.IsNullOrWhiteSpace(comment.HtmlUrl))
//                 result.Warnings.Add($"Comment {comment.Id} has no HTML URL");
//         }

//         result.IsValid = !result.Errors.Any();
//         return Task.FromResult(result);
//     }

//     public IEnumerable<ExportFormat> GetSupportedFormats()
//     {
//         return Enum.GetValues<ExportFormat>();
//     }

//     private static ExportFormat DetectFormat(string filePath, string content)
//     {
//         var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
//         return extension switch
//         {
//             ".json" => ExportFormat.Json,
//             ".csv" => ExportFormat.Csv,
//             _ => content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[") 
//                 ? ExportFormat.Json 
//                 : ExportFormat.Csv
//         };
//     }

//     private static string EscapeCsvValue(string? value)
//     {
//         if (value == null)
//             return "";
//         if (string.IsNullOrEmpty(value))
//             return "";

//         if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
//         {
//             return "\"" + value.Replace("\"", "\"\"") + "\"";
//         }

//         return value;
//     }

//     private static List<string> ParseCsvLine(string line)
//     {
//         var values = new List<string>();
//         var current = new StringBuilder();
//         bool inQuotes = false;

//         for (int i = 0; i < line.Length; i++)
//         {
//             char c = line[i];

//             if (c == '"')
//             {
//                 if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
//                 {
//                     current.Append('"');
//                     i++; // Skip next quote
//                 }
//                 else
//                 {
//                     inQuotes = !inQuotes;
//                 }
//             }
//             else if (c == ',' && !inQuotes)
//             {
//                 values.Add(current.ToString());
//                 current.Clear();
//             }
//             else
//             {
//                 current.Append(c);
//             }
//         }

//         values.Add(current.ToString());
//         return values;
//     }

//     private static Comment ParseCsvComment(List<string> headers, List<string> values)
//     {
//         var comment = new Comment();

//         for (int i = 0; i < Math.Min(headers.Count, values.Count); i++)
//         {
//             var header = headers[i].Trim();
//             var value = values[i].Trim();

//             switch (header.ToLowerInvariant())
//             {
//                 case "id":
//                     if (long.TryParse(value, out var id)) comment.Id = id;
//                     break;
//                 case "body":
//                     comment.Body = value;
//                     break;
//                 case "author":
//                     comment.Author = new User { Login = value };
//                     break;
//                 case "type":
//                     if (Enum.TryParse<CommentType>(value, out var type)) comment.Type = type;
//                     break;
//                 case "createdat":
//                     if (DateTimeOffset.TryParse(value, out var createdAt)) comment.CreatedAt = createdAt;
//                     break;
//                 case "updatedat":
//                     if (DateTimeOffset.TryParse(value, out var updatedAt)) comment.UpdatedAt = updatedAt;
//                     break;
//                 case "htmlurl":
//                     comment.HtmlUrl = value;
//                     break;
//                 case "pullrequestid":
//                     if (long.TryParse(value, out var prId)) comment.PullRequestId = prId;
//                     break;
//                 case "path":
//                     comment.Path = value;
//                     break;
//                 case "position":
//                     if (int.TryParse(value, out var position)) comment.Position = position;
//                     break;
//                 case "diffhunk":
//                     comment.DiffHunk = value;
//                     break;
//             }
//         }

//         return comment;
//     }

//     private static ExportComment MapToExportComment(Comment comment, bool includeMetadata)
//     {
//         var exportComment = new ExportComment
//         {
//             Id = comment.Id,
//             Body = comment.Body,
//             Author = comment.Author.Login,
//             Type = comment.Type.ToString(),
//             CreatedAt = comment.CreatedAt,
//             UpdatedAt = comment.UpdatedAt,
//             HtmlUrl = comment.HtmlUrl
//         };

//         if (includeMetadata)
//         {
//             exportComment.PullRequestId = comment.PullRequestId;
//             exportComment.Path = comment.Path;
//             exportComment.Position = comment.Position;
//             exportComment.DiffHunk = comment.DiffHunk;
//         }

//         return exportComment;
//     }

//     private static Comment MapFromExportComment(ExportComment exportComment)
//     {
//         return new Comment
//         {
//             Id = exportComment.Id,
//             Body = exportComment.Body ?? "",
//             Author = new User { Login = exportComment.Author ?? "Unknown" },
//             Type = Enum.TryParse<CommentType>(exportComment.Type, out var type) ? type : CommentType.Issue,
//             CreatedAt = exportComment.CreatedAt,
//             UpdatedAt = exportComment.UpdatedAt,
//             HtmlUrl = exportComment.HtmlUrl,
//             PullRequestId = exportComment.PullRequestId ?? 0,
//             Path = exportComment.Path,
//             Position = exportComment.Position,
//             DiffHunk = exportComment.DiffHunk
//         };
//     }

//     private class CommentExportData
//     {
//         public DateTimeOffset ExportedAt { get; set; }
//         public string ExportedBy { get; set; } = "";
//         public string Version { get; set; } = "";
//         public bool IncludeMetadata { get; set; }
//         public List<ExportComment> Comments { get; set; } = new();
//     }

//     private class ExportComment
//     {
//         public long Id { get; set; }
//         public string? Body { get; set; }
//         public string? Author { get; set; }
//         public string? Type { get; set; }
//         public DateTimeOffset CreatedAt { get; set; }
//         public DateTimeOffset UpdatedAt { get; set; }
//         public string? HtmlUrl { get; set; }
//         public long? PullRequestId { get; set; }
//         public string? Path { get; set; }
//         public int? Position { get; set; }
//         public string? DiffHunk { get; set; }
//     }
// }
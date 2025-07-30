using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Infrastructure.Tests;

/// <summary>
/// Tests for CommentExportImportService functionality
/// </summary>
public class CommentExportImportServiceTests
{
    private readonly Mock<ILogger<CommentExportImportService>> _mockLogger;
    private readonly CommentExportImportService _exportImportService;

    public CommentExportImportServiceTests()
    {
        _mockLogger = new Mock<ILogger<CommentExportImportService>>();
        _exportImportService = new CommentExportImportService(_mockLogger.Object);
    }

    [Fact]
    public async Task ExportToJsonAsync_WithValidComments_ReturnsJsonString()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Body = "Test comment",
                Author = new User { Login = "testuser" },
                Type = CommentType.Issue,
                PullRequestId = 123,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        var result = await _exportImportService.ExportToJsonAsync(comments);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Test comment", result);
        Assert.Contains("testuser", result);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithValidComments_ReturnsCsvString()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Body = "Test comment",
                Author = new User { Login = "testuser" },
                Type = CommentType.Issue,
                PullRequestId = 123,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        var result = await _exportImportService.ExportToCsvAsync(comments);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Id,Body,Author", result);
        Assert.Contains("Test comment", result);
        Assert.Contains("testuser", result);
    }

    [Fact]
    public async Task ValidateCommentsAsync_WithValidComments_ReturnsValidResult()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Body = "Test comment",
                Author = new User { Id = 1, Login = "testuser" },
                Type = CommentType.Issue,
                PullRequestId = 123,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        var result = await _exportImportService.ValidateCommentsAsync(comments);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.ValidatedCount);
    }

    [Fact]
    public async Task ValidateCommentsAsync_WithInvalidComments_ReturnsInvalidResult()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = 0, // Invalid ID
                Body = "", // Empty body
                Author = new User { Id = 0, Login = "" }, // Invalid author
                Type = CommentType.Issue,
                PullRequestId = 123,
                CreatedAt = default, // Invalid date
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        var result = await _exportImportService.ValidateCommentsAsync(comments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(1, result.ValidatedCount);
    }

    [Fact]
    public void GetSupportedFormats_ReturnsExpectedFormats()
    {
        // Act
        var formats = _exportImportService.GetSupportedFormats().ToList();

        // Assert
        Assert.Contains(ExportFormat.Json, formats);
        Assert.Contains(ExportFormat.Csv, formats);
        Assert.True(formats.Count > 0);
    }
}
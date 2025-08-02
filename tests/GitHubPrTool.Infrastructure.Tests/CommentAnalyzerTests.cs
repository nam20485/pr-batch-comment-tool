using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Infrastructure.Services;
using GitHubPrTool.TestUtilities.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Infrastructure.Tests;

/// <summary>
/// Tests for CommentAnalyzer functionality
/// </summary>
public class CommentAnalyzerTests
{
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ILogger<CommentAnalyzer>> _mockLogger;
    private readonly CommentAnalyzer _commentAnalyzer;

    public CommentAnalyzerTests()
    {
        _mockAIService = new Mock<IAIService>();
        _mockLogger = new Mock<ILogger<CommentAnalyzer>>();

        _commentAnalyzer = new CommentAnalyzer(
            _mockAIService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullAIService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommentAnalyzer(
            null!,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommentAnalyzer(
            _mockAIService.Object,
            null!));
    }

    [Fact]
    public async Task CategorizeCommentAsync_WithValidComment_ReturnsCommentCategory()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("This is a bug report");

        var expectedAIResponse = @"{
            ""category"": ""Bug"",
            ""subCategory"": ""Error Report"",
            ""confidence"": 0.9,
            ""priority"": 4,
            ""sentiment"": -0.3,
            ""complexity"": 3,
            ""tags"": [""bug"", ""error""]
        }";

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAIResponse);

        _mockAIService
            .Setup(x => x.GetModelInfo())
            .Returns("test-model-v1.0");

        // Act
        var result = await _commentAnalyzer.CategorizeCommentAsync(comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommentCategoryType.Bug, result.Category);
        Assert.Equal("Error Report", result.SubCategory);
        Assert.Equal(0.9, result.Confidence);
        Assert.Equal(4, result.Priority);
        Assert.Equal(-0.3, result.Sentiment);
        Assert.Equal(3, result.Complexity);
        Assert.Contains("bug", result.Tags);
        Assert.Contains("error", result.Tags);
        Assert.Equal("test-model-v1.0", result.ModelVersion);
    }

    [Fact]
    public async Task CategorizeCommentAsync_WithInvalidAIResponse_ReturnsDefaultCategory()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("Test comment");

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("invalid json response");

        _mockAIService
            .Setup(x => x.GetModelInfo())
            .Returns("test-model-v1.0");

        // Act
        var result = await _commentAnalyzer.CategorizeCommentAsync(comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommentCategoryType.General, result.Category);
        Assert.Equal(0.3, result.Confidence);
        Assert.Equal(3, result.Priority);
        Assert.Equal("test-model-v1.0", result.ModelVersion);
    }

    [Fact]
    public async Task CategorizeCommentAsync_WhenAIServiceThrows_ReturnsDefaultCategory()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("Test comment");

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AI service error"));

        _mockAIService
            .Setup(x => x.GetModelInfo())
            .Returns("test-model-v1.0");

        // Act
        var result = await _commentAnalyzer.CategorizeCommentAsync(comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommentCategoryType.General, result.Category);
        Assert.Equal(0.5, result.Confidence);
        Assert.Equal(3, result.Priority);
        Assert.Equal("test-model-v1.0", result.ModelVersion);
    }

    [Fact]
    public async Task CategorizeCommentAsync_WithContext_PassesContextToAI()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("Test comment");
        const string context = "Pull request review context";
        var capturedPrompt = string.Empty;

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, string?, CancellationToken>((prompt, _, _, _) => capturedPrompt = prompt)  
            .ReturnsAsync(@"{""category"": ""General"", ""confidence"": 0.5}");

        _mockAIService.Setup(x => x.GetModelInfo()).Returns("test-model");

        // Act
        await _commentAnalyzer.CategorizeCommentAsync(comment, context);

        // Assert
        Assert.Contains(context, capturedPrompt);
    }

    [Fact]
    public async Task CategorizeCommentsAsync_WithMultipleComments_ReturnsAllCategories()
    {
        // Arrange
        var comments = CommentDataBuilder.CreateMany(2);

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{""category"": ""General"", ""confidence"": 0.7}");

        _mockAIService.Setup(x => x.GetModelInfo()).Returns("test-model");

        // Act
        var result = await _commentAnalyzer.CategorizeCommentsAsync(comments);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result.Values, category => Assert.Equal(0.7, category.Confidence));
    }

    [Fact]
    public async Task GenerateInsightsAsync_WithValidComment_ReturnsInsights()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("This code needs optimization");

        var expectedAIResponse = @"[
            {
                ""type"": ""performance"",
                ""content"": ""Code optimization suggestion detected"",
                ""confidence"": 0.8,
                ""metadata"": null
            }
        ]";

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAIResponse);

        _mockAIService.Setup(x => x.GetModelInfo()).Returns("test-model");

        // Act
        var result = await _commentAnalyzer.GenerateInsightsAsync(comment);

        // Assert
        var insights = result.ToList();
        Assert.Single(insights);
        Assert.Equal("performance", insights[0].Type);
        Assert.Equal("Code optimization suggestion detected", insights[0].Content);
        Assert.Equal(0.8, insights[0].Confidence);
    }

    [Fact]
    public async Task GenerateInsightsAsync_WithInvalidAIResponse_ReturnsErrorInsight()
    {
        // Arrange
        var comment = CommentDataBuilder.WithBody("Test comment");

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("invalid json");

        _mockAIService.Setup(x => x.GetModelInfo()).Returns("test-model");

        // Act
        var result = await _commentAnalyzer.GenerateInsightsAsync(comment);

        // Assert
        var insights = result.ToList();
        Assert.Single(insights);
        Assert.Equal("parsing_error", insights[0].Type);
        Assert.Equal("Unable to parse AI insights response", insights[0].Content);
        Assert.Equal(0.1, insights[0].Confidence);
    }

    [Fact]
    public async Task ExtractTopicsAsync_WithValidComments_ReturnsTopics()
    {
        // Arrange
        var comments = CommentDataBuilder.CreateMany(2);

        var expectedAIResponse = @"{
            ""performance"": 0.8,
            ""security"": 0.9,
            ""authentication"": 0.7
        }";

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAIResponse);

        // Act
        var result = await _commentAnalyzer.ExtractTopicsAsync(comments);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(0.8, result["performance"]);
        Assert.Equal(0.9, result["security"]);
        Assert.Equal(0.7, result["authentication"]);
    }

    [Fact]
    public async Task ExtractTopicsAsync_WhenAIServiceThrows_ReturnsEmptyDictionary()
    {  
        // Arrange
        var comments = CommentDataBuilder.CreateMany(1);

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AI service error"));

        // Act
        var result = await _commentAnalyzer.ExtractTopicsAsync(comments);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractTopicsAsync_WithInvalidAIResponse_ReturnsEmptyDictionary()
    {
        // Arrange
        var comments = CommentDataBuilder.CreateMany(1);

        _mockAIService
            .Setup(x => x.GenerateStructuredTextAsync(
                It.IsAny<string>(),
                "json",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("invalid json response");

        // Act
        var result = await _commentAnalyzer.ExtractTopicsAsync(comments);

        // Assert
        Assert.Empty(result);
    }
}
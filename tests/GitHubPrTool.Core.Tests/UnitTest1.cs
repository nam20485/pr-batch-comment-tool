using GitHubPrTool.Core.Models;
using GitHubPrTool.Core.Services;
using GitHubPrTool.Core.Interfaces;
using Moq;

namespace GitHubPrTool.Core.Tests;

public class CommentServiceTests
{
    private readonly Mock<IGitHubRepository> _mockGitHubRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _mockGitHubRepository = new Mock<IGitHubRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _commentService = new CommentService(_mockGitHubRepository.Object, _mockCacheService.Object);
    }

    [Fact]
    public void FilterComments_WithAuthorFilter_ReturnsMatchingComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = 1, Author = new User { Login = "user1" }, Body = "Comment 1" },
            new() { Id = 2, Author = new User { Login = "user2" }, Body = "Comment 2" },
            new() { Id = 3, Author = new User { Login = "user1" }, Body = "Comment 3" }
        };

        var filter = new CommentFilter { AuthorLogin = "user1" };

        // Act
        var result = _commentService.FilterComments(comments, filter);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains("user1", c.Author.Login));
    }

    [Fact]
    public void FilterComments_WithBodyContainsFilter_ReturnsMatchingComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = 1, Author = new User { Login = "user1" }, Body = "This is a bug" },
            new() { Id = 2, Author = new User { Login = "user2" }, Body = "Feature request" },
            new() { Id = 3, Author = new User { Login = "user3" }, Body = "Another bug report" }
        };

        var filter = new CommentFilter { BodyContains = "bug" };

        // Act
        var result = _commentService.FilterComments(comments, filter);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains("bug", c.Body, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SortComments_ByCreatedAt_ReturnsSortedComments()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var comments = new List<Comment>
        {
            new() { Id = 1, CreatedAt = now.AddHours(-2), Author = new User { Login = "user1" } },
            new() { Id = 2, CreatedAt = now.AddHours(-1), Author = new User { Login = "user2" } },
            new() { Id = 3, CreatedAt = now.AddHours(-3), Author = new User { Login = "user3" } }
        };

        // Act
        var result = _commentService.SortComments(comments, CommentSortBy.CreatedAt);

        // Assert
        var sortedComments = result.ToList();
        Assert.Equal(3, sortedComments[0].Id); // Oldest first
        Assert.Equal(1, sortedComments[1].Id);
        Assert.Equal(2, sortedComments[2].Id); // Newest last
    }

    [Fact]
    public void SortComments_ByCreatedAtDescending_ReturnsSortedCommentsDescending()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var comments = new List<Comment>
        {
            new() { Id = 1, CreatedAt = now.AddHours(-2), Author = new User { Login = "user1" } },
            new() { Id = 2, CreatedAt = now.AddHours(-1), Author = new User { Login = "user2" } },
            new() { Id = 3, CreatedAt = now.AddHours(-3), Author = new User { Login = "user3" } }
        };

        // Act
        var result = _commentService.SortComments(comments, CommentSortBy.CreatedAt, descending: true);

        // Assert
        var sortedComments = result.ToList();
        Assert.Equal(2, sortedComments[0].Id); // Newest first
        Assert.Equal(1, sortedComments[1].Id);
        Assert.Equal(3, sortedComments[2].Id); // Oldest last
    }

    [Fact]
    public async Task DuplicateCommentsAsync_WithValidInput_CallsRepository()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = 1, Body = "Comment 1", Author = new User { Login = "user1" } }
        };
        var targetPullRequestId = 123L;
        var reviewBody = "Duplicated review";

        var expectedReview = new Review
        {
            Id = 1,
            Body = reviewBody,
            PullRequestId = targetPullRequestId
        };

        _mockGitHubRepository
            .Setup(x => x.DuplicateCommentsAsync(comments, targetPullRequestId, reviewBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReview);

        // Act
        var result = await _commentService.DuplicateCommentsAsync(comments, targetPullRequestId, reviewBody);

        // Assert
        Assert.Equal(expectedReview.Id, result.Id);
        Assert.Equal(expectedReview.Body, result.Body);
        _mockGitHubRepository.Verify(
            x => x.DuplicateCommentsAsync(comments, targetPullRequestId, reviewBody, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockCacheService.Verify(
            x => x.RemoveByPatternAsync($"pr-{targetPullRequestId}*", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DuplicateCommentsAsync_WithEmptyComments_ThrowsArgumentException()
    {
        // Arrange
        var comments = new List<Comment>();
        var targetPullRequestId = 123L;
        var reviewBody = "Duplicated review";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.DuplicateCommentsAsync(comments, targetPullRequestId, reviewBody));

        Assert.Contains("At least one comment must be selected", exception.Message);
    }
}
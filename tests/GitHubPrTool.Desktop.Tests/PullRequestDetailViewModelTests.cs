using FluentAssertions;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Desktop.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Desktop.Tests;

/// <summary>
/// Tests for PullRequestDetailViewModel functionality.
/// </summary>
public class PullRequestDetailViewModelTests
{
    private readonly Mock<IGitHubRepository> _mockRepository;
    private readonly Mock<IDataSyncService> _mockDataSync;
    private readonly Mock<ILogger<PullRequestDetailViewModel>> _mockLogger;
    private readonly PullRequestDetailViewModel _viewModel;

    public PullRequestDetailViewModelTests()
    {
        _mockRepository = new Mock<IGitHubRepository>();
        _mockDataSync = new Mock<IDataSyncService>();
        _mockLogger = new Mock<ILogger<PullRequestDetailViewModel>>();
        
        _viewModel = new PullRequestDetailViewModel(
            _mockRepository.Object,
            _mockDataSync.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.PullRequest.Should().BeNull();
        _viewModel.Comments.Should().BeEmpty();
        _viewModel.Reviews.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("Ready");
        _viewModel.SelectedTab.Should().Be("Overview");
        _viewModel.ShowComments.Should().BeTrue();
        _viewModel.ShowReviews.Should().BeTrue();
        _viewModel.AvailableTabs.Should().Contain(new[] { "Overview", "Comments", "Reviews", "Files" });
    }

    [Fact]
    public async Task LoadPullRequestDetailAsync_ShouldLoadPullRequestAndRelatedData()
    {
        // Arrange
        var pullRequest = new PullRequest
        {
            Id = 1,
            Number = 123,
            Title = "Test PR",
            RepositoryId = 1,
            Repository = new Repository { FullName = "test/repo" }
        };

        var comments = new List<Comment>
        {
            new() { Id = 1, Body = "Test comment", Author = new User { Login = "user1" }, CreatedAt = DateTime.Now }
        };

        var reviews = new List<Review>
        {
            new() { Id = 1, Body = "Test review", Author = new User { Login = "reviewer1" }, SubmittedAt = DateTime.Now }
        };

        _mockRepository.Setup(x => x.GetPullRequestAsync(pullRequest.RepositoryId, pullRequest.Number, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(pullRequest);
        
        _mockRepository.Setup(x => x.GetCommentsAsync(pullRequest.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(comments);
        
        _mockRepository.Setup(x => x.GetReviewsAsync(pullRequest.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(reviews);

        // Act
        await _viewModel.LoadPullRequestDetailAsync(pullRequest);

        // Assert
        _viewModel.PullRequest.Should().NotBeNull();
        _viewModel.Comments.Should().HaveCount(1);
        _viewModel.Reviews.Should().HaveCount(1);
        _viewModel.StatusMessage.Should().Contain("Loaded PR #123 with 1 comments and 1 reviews");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void ViewCommentsCommand_ShouldSwitchToCommentsTab()
    {
        // Act
        _viewModel.ViewCommentsCommand.Execute(null);

        // Assert
        _viewModel.SelectedTab.Should().Be("Comments");
        _viewModel.StatusMessage.Should().Contain("Viewing 0 comments");
    }

    [Fact]
    public void ViewReviewsCommand_ShouldSwitchToReviewsTab()
    {
        // Act
        _viewModel.ViewReviewsCommand.Execute(null);

        // Assert
        _viewModel.SelectedTab.Should().Be("Reviews");
        _viewModel.StatusMessage.Should().Contain("Viewing 0 reviews");
    }

    [Fact]
    public void Clear_ShouldResetAllProperties()
    {
        // Arrange
        var pullRequest = new PullRequest { Id = 1, Number = 123, Title = "Test" };
        _viewModel.Comments.Add(new Comment { Id = 1, Body = "Test", Author = new User { Login = "user" } });
        _viewModel.Reviews.Add(new Review { Id = 1, Body = "Test", Author = new User { Login = "user" } });
        _viewModel.SelectedTab = "Comments";

        // Act
        _viewModel.Clear();

        // Assert
        _viewModel.PullRequest.Should().BeNull();
        _viewModel.Comments.Should().BeEmpty();
        _viewModel.Reviews.Should().BeEmpty();
        _viewModel.SelectedTab.Should().Be("Overview");
        _viewModel.StatusMessage.Should().Be("Ready");
    }

    [Fact]
    public async Task LoadPullRequestDetailAsync_WithNullPullRequest_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _viewModel.LoadPullRequestDetailAsync(null!);
        await act.Should().NotThrowAsync();
        
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshCommand_WithNoPullRequest_ShouldReturn()
    {
        // Act
        await _viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        _viewModel.IsLoading.Should().BeFalse();
        // Should not call any repository methods
        _mockRepository.Verify(x => x.GetPullRequestAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
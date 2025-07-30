using FluentAssertions;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Desktop.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Desktop.Tests;

/// <summary>
/// Tests for CommentListViewModel functionality.
/// </summary>
public class CommentListViewModelTests
{
    private readonly Mock<IGitHubRepository> _mockRepository;
    private readonly Mock<IDataSyncService> _mockDataSync;
    private readonly Mock<ILogger<CommentListViewModel>> _mockLogger;
    private readonly CommentListViewModel _viewModel;

    public CommentListViewModelTests()
    {
        _mockRepository = new Mock<IGitHubRepository>();
        _mockDataSync = new Mock<IDataSyncService>();
        _mockLogger = new Mock<ILogger<CommentListViewModel>>();
        
        _viewModel = new CommentListViewModel(
            _mockRepository.Object,
            _mockDataSync.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.Comments.Should().BeEmpty();
        _viewModel.SelectedComments.Should().BeEmpty();
        _viewModel.SearchText.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("Ready");
        _viewModel.SelectedTypeFilter.Should().Be("All");
        _viewModel.SelectedAuthorFilter.Should().Be("All");
        _viewModel.SortBy.Should().Be("Created");
        _viewModel.SortDescending.Should().BeFalse();
        _viewModel.ShowSelection.Should().BeFalse();
        _viewModel.PullRequestTitle.Should().Be("No Pull Request Selected");
        _viewModel.AvailableTypeFilters.Should().Contain(new[] { "All", "Issue", "Review", "Commit" });
        _viewModel.AvailableAuthors.Should().Contain("All");
    }

    [Fact]
    public async Task LoadCommentsAsync_ShouldLoadCommentsAndUpdateCollections()
    {
        // Arrange
        var pullRequest = new PullRequest
        {
            Id = 1,
            Number = 123,
            Title = "Test PR",
            Repository = new Repository { FullName = "test/repo" }
        };

        var comments = new List<Comment>
        {
            new() { Id = 1, Body = "First comment", Type = CommentType.Issue, Author = new User { Login = "user1" }, CreatedAt = DateTime.Now },
            new() { Id = 2, Body = "Second comment", Type = CommentType.Review, Author = new User { Login = "user2" }, CreatedAt = DateTime.Now.AddHours(1) }
        };

        _mockRepository.Setup(x => x.GetCommentsAsync(pullRequest.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(comments);

        // Act
        await _viewModel.LoadCommentsAsync(pullRequest);

        // Assert
        _viewModel.Comments.Should().HaveCount(2);
        _viewModel.PullRequestTitle.Should().Be("PR #123: Test PR");
        _viewModel.AvailableAuthors.Should().Contain(new[] { "All", "user1", "user2" });
        _viewModel.StatusMessage.Should().Contain("Loaded 2 comments");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void ClearFiltersCommand_ShouldResetAllFilters()
    {
        // Arrange
        _viewModel.SearchText = "test";
        _viewModel.SelectedTypeFilter = "Review";
        _viewModel.SelectedAuthorFilter = "user1";
        _viewModel.SortBy = "Author";
        _viewModel.SortDescending = true;

        // Act
        _viewModel.ClearFiltersCommand.Execute(null);

        // Assert
        _viewModel.SearchText.Should().BeEmpty();
        _viewModel.SelectedTypeFilter.Should().Be("All");
        _viewModel.SelectedAuthorFilter.Should().Be("All");
        _viewModel.SortBy.Should().Be("Created");
        _viewModel.SortDescending.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("Filters cleared");
    }

    [Fact]
    public void ToggleSelectionModeCommand_ShouldToggleSelectionMode()
    {
        // Assert initial state
        _viewModel.ShowSelection.Should().BeFalse();

        // Act - enable selection mode
        _viewModel.ToggleSelectionModeCommand.Execute(null);

        // Assert
        _viewModel.ShowSelection.Should().BeTrue();
        _viewModel.StatusMessage.Should().Be("Selection mode enabled");

        // Act - disable selection mode
        _viewModel.ToggleSelectionModeCommand.Execute(null);

        // Assert
        _viewModel.ShowSelection.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("Selection mode disabled");
    }

    [Fact]
    public void ToggleCommentSelection_ShouldAddAndRemoveComments()
    {
        // Arrange
        var comment = new Comment
        {
            Id = 1,
            Body = "Test comment",
            Author = new User { Login = "testuser" }
        };

        // Act - add comment to selection
        _viewModel.ToggleCommentSelection(comment);

        // Assert
        _viewModel.SelectedComments.Should().Contain(comment);

        // Act - remove comment from selection
        _viewModel.ToggleCommentSelection(comment);

        // Assert
        _viewModel.SelectedComments.Should().NotContain(comment);
    }

    [Fact]
    public void SelectAllCommentsCommand_ShouldSelectAllVisibleComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = 1, Body = "Comment 1", Author = new User { Login = "user1" } },
            new() { Id = 2, Body = "Comment 2", Author = new User { Login = "user2" } }
        };

        foreach (var comment in comments)
        {
            _viewModel.Comments.Add(comment);
        }

        // Act
        _viewModel.SelectAllCommentsCommand.Execute(null);

        // Assert
        _viewModel.SelectedComments.Should().HaveCount(2);
        _viewModel.SelectedComments.Should().Contain(comments);
    }

    [Fact]
    public void ClearSelectionCommand_ShouldClearAllSelectedComments()
    {
        // Arrange
        var comment = new Comment { Id = 1, Body = "Test", Author = new User { Login = "user" } };
        _viewModel.SelectedComments.Add(comment);

        // Act
        _viewModel.ClearSelectionCommand.Execute(null);

        // Assert
        _viewModel.SelectedComments.Should().BeEmpty();
    }

    [Fact]
    public void Clear_ShouldResetAllProperties()
    {
        // Arrange
        var pullRequest = new PullRequest { Id = 1, Number = 123, Title = "Test" };
        _viewModel.Comments.Add(new Comment { Id = 1, Body = "Test", Author = new User { Login = "user" } });
        _viewModel.SelectedComments.Add(new Comment { Id = 2, Body = "Test2", Author = new User { Login = "user2" } });
        _viewModel.SearchText = "test";

        // Act
        _viewModel.Clear();

        // Assert
        _viewModel.Comments.Should().BeEmpty();
        _viewModel.SelectedComments.Should().BeEmpty();
        _viewModel.PullRequestTitle.Should().Be("No Pull Request Selected");
        _viewModel.SearchText.Should().BeEmpty();
        _viewModel.StatusMessage.Should().Be("Ready");
        _viewModel.AvailableAuthors.Should().Contain("All");
        _viewModel.AvailableAuthors.Should().HaveCount(1);
    }
}
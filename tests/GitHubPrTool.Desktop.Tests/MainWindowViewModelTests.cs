using FluentAssertions;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Desktop.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Desktop.Tests;

/// <summary>
/// Tests for MainWindowViewModel functionality.
/// </summary>
public class MainWindowViewModelTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<MainWindowViewModel>> _mockLogger;
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<MainWindowViewModel>>();
        
        // Create mocks for the ViewModels
        var mockGitHubRepo = new Mock<IGitHubRepository>();
        var mockDataSync = new Mock<IDataSyncService>();
        var mockAuth = new Mock<IAuthService>();
        var mockRepoLogger = new Mock<ILogger<RepositoryListViewModel>>();
        var mockPrLogger = new Mock<ILogger<PullRequestListViewModel>>();
        var mockPrDetailLogger = new Mock<ILogger<PullRequestDetailViewModel>>();
        var mockCommentLogger = new Mock<ILogger<CommentListViewModel>>();
        
        var repositoryListViewModel = new RepositoryListViewModel(
            mockGitHubRepo.Object, 
            mockDataSync.Object, 
            mockAuth.Object, 
            mockRepoLogger.Object);
            
        var pullRequestListViewModel = new PullRequestListViewModel(
            mockGitHubRepo.Object, 
            mockDataSync.Object, 
            mockAuth.Object, 
            mockPrLogger.Object);
            
        var pullRequestDetailViewModel = new PullRequestDetailViewModel(
            mockGitHubRepo.Object,
            mockDataSync.Object,
            mockPrDetailLogger.Object);
            
        var commentListViewModel = new CommentListViewModel(
            mockGitHubRepo.Object,
            mockDataSync.Object,
            mockCommentLogger.Object);
        
        _viewModel = new MainWindowViewModel(
            _mockAuthService.Object, 
            repositoryListViewModel, 
            pullRequestListViewModel,
            pullRequestDetailViewModel,
            commentListViewModel,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act - constructor called in setup

        // Assert - These values are set after UpdateAuthenticationStatus is called
        _viewModel.ConnectionStatus.Should().Be("Offline");
        _viewModel.CurrentUser.Should().BeNull();
        _viewModel.IsAuthenticated.Should().BeFalse();
        _viewModel.IsContentLoaded.Should().BeFalse();
        
        // Status message is set by UpdateAuthenticationStatus when not authenticated
        _viewModel.StatusMessage.Should().Be("Not authenticated");
    }

    [Fact]
    public void NavigateToRepositories_ShouldUpdateStatusAndContentLoaded()
    {
        // Act
        _viewModel.NavigateToRepositoriesCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Loading repositories...");
        _viewModel.IsContentLoaded.Should().BeTrue();
        _viewModel.CurrentContent.Should().NotBeNull();
        _viewModel.CurrentContent.Should().BeOfType<RepositoryListViewModel>();
    }

    [Fact]
    public void NavigateToPullRequests_ShouldUpdateStatusAndContentLoaded()
    {
        // Act
        _viewModel.NavigateToPullRequestsCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Loading pull requests...");
        _viewModel.IsContentLoaded.Should().BeTrue();
        _viewModel.CurrentContent.Should().NotBeNull();
        _viewModel.CurrentContent.Should().BeOfType<PullRequestListViewModel>();
    }

    [Fact]
    public void NavigateToComments_ShouldUpdateStatusAndContentLoaded()
    {
        // Act
        _viewModel.NavigateToCommentsCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Loading comments...");
        _viewModel.IsContentLoaded.Should().BeTrue();
    }

    [Fact]
    public void BatchComment_ShouldUpdateStatus()
    {
        // Act
        _viewModel.BatchCommentCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Opening batch comment tool...");
    }

    [Fact]
    public void OpenSettings_ShouldUpdateStatus()
    {
        // Act
        _viewModel.OpenSettingsCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Opening settings...");
    }

    [Fact]
    public void ShowAbout_ShouldUpdateStatus()
    {
        // Act
        _viewModel.ShowAboutCommand.Execute(null);

        // Assert
        _viewModel.StatusMessage.Should().Be("Showing about information...");
    }
}
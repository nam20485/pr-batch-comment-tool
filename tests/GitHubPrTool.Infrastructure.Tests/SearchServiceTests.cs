using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Infrastructure.Tests;

/// <summary>
/// Tests for SearchService functionality
/// </summary>
public class SearchServiceTests
{
    private readonly Mock<IGitHubRepository> _mockRepository;
    private readonly Mock<ILogger<SearchService>> _mockLogger;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockRepository = new Mock<IGitHubRepository>();
        _mockLogger = new Mock<ILogger<SearchService>>();
        _searchService = new SearchService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SearchRepositoriesAsync_WithValidQuery_ReturnsMatchingRepositories()
    {
        // Arrange
        var repositories = new List<Repository>
        {
            new Repository 
            { 
                Id = 1, 
                Name = "test-repo", 
                FullName = "user/test-repo",
                Description = "A sample repository",
                Owner = new User { Login = "user1" }
            },
            new Repository 
            { 
                Id = 2, 
                Name = "another-repo", 
                FullName = "user/another-repo",
                Description = "Another repository",
                Owner = new User { Login = "user2" }
            }
        };

        _mockRepository.Setup(r => r.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(repositories);

        // Act
        var results = await _searchService.SearchRepositoriesAsync("test");

        // Assert
        Assert.Single(results);
        Assert.Equal("test-repo", results.First().Name);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsSearchResults()
    {
        // Arrange
        var repositories = new List<Repository>
        {
            new Repository 
            { 
                Id = 1, 
                Name = "test-repo", 
                FullName = "user/test-repo",
                Description = "A sample repository",
                Owner = new User { Login = "user1" }
            }
        };

        _mockRepository.Setup(r => r.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(repositories);

        _mockRepository.Setup(r => r.GetPullRequestsAsync(It.IsAny<long>(), It.IsAny<PullRequestState?>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<PullRequest>());

        // Act
        var results = await _searchService.SearchAsync("test");

        // Assert
        Assert.NotNull(results);
        Assert.Equal("test", results.Query);
        Assert.Single(results.Repositories);
        Assert.Equal(1, results.TotalResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task SearchRepositoriesAsync_WithEmptyQuery_ReturnsEmptyResults(string query)
    {
        // Act
        var results = await _searchService.SearchRepositoriesAsync(query);

        // Assert
        Assert.Empty(results);
    }
}
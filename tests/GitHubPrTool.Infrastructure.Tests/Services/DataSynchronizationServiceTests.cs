using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Octokit;

namespace GitHubPrTool.Infrastructure.Tests.Services;

/// <summary>
/// Tests for the DataSynchronizationService class
/// </summary>
public class DataSynchronizationServiceTests
{
    private readonly Mock<IGitHubClient> _mockGitHubClient;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<DataSynchronizationService>> _mockLogger;
    private readonly DataSynchronizationService _synchronizationService;

    public DataSynchronizationServiceTests()
    {
        _mockGitHubClient = new Mock<IGitHubClient>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<DataSynchronizationService>>();

        _synchronizationService = new DataSynchronizationService(
            _mockGitHubClient.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        var service = new DataSynchronizationService(
            _mockGitHubClient.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
        
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullGitHubClient_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DataSynchronizationService(
            null!,
            _mockCacheService.Object,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullCacheService_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DataSynchronizationService(
            _mockGitHubClient.Object,
            null!,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DataSynchronizationService(
            _mockGitHubClient.Object,
            _mockCacheService.Object,
            null!));
    }

    [Fact]
    public void IsSynchronizationInProgress_Initially_ShouldBeFalse()
    {
        // Arrange & Act & Assert
        Assert.False(_synchronizationService.IsSynchronizationInProgress);
    }

    [Fact]
    public async Task GetLastSynchronizationTimeAsync_WithValidDataType_ShouldReturnDateTime()
    {
        // Arrange
        var expectedTime = DateTime.UtcNow.ToString("O");
        _mockCacheService
            .Setup(c => c.GetAsync<string>("sync_repositories_timestamp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTime);

        // Act
        var result = await _synchronizationService.GetLastSynchronizationTimeAsync("repositories");

        // Assert
        Assert.NotNull(result);
        Assert.True(Math.Abs((result.Value - DateTime.Parse(expectedTime)).TotalSeconds) < 1);
    }

    [Fact]
    public async Task GetLastSynchronizationTimeAsync_WithNoStoredTime_ShouldReturnNull()
    {
        // Arrange
        _mockCacheService
            .Setup(c => c.GetAsync<string>("sync_repositories_timestamp", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _synchronizationService.GetLastSynchronizationTimeAsync("repositories");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLastSynchronizationTimeAsync_WithEntityId_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var entityId = 123L;
        var expectedCacheKey = $"sync_pull_requests_{entityId}_timestamp";
        
        _mockCacheService
            .Setup(c => c.GetAsync<string>(expectedCacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.ToString("O"));

        // Act
        await _synchronizationService.GetLastSynchronizationTimeAsync("pull_requests", entityId);

        // Assert
        _mockCacheService.Verify(
            c => c.GetAsync<string>(expectedCacheKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
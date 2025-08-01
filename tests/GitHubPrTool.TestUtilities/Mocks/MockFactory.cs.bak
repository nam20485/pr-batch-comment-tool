using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Moq;

namespace GitHubPrTool.TestUtilities.Mocks;

/// <summary>
/// Factory for creating commonly used mocks with realistic behaviors
/// </summary>
public static class MockFactory
{
    /// <summary>
    /// Create a mock IGitHubRepository with common setup
    /// </summary>
    public static Mock<IGitHubRepository> CreateGitHubRepository()
    {
        var mock = new Mock<IGitHubRepository>();
        
        // Setup common successful operations
        mock.Setup(x => x.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        mock.Setup(x => x.GetPullRequestsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        mock.Setup(x => x.GetCommentsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        mock.Setup(x => x.GetReviewsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        return mock;
    }

    /// <summary>
    /// Create a mock IGitHubRepository with realistic data
    /// </summary>
    public static Mock<IGitHubRepository> CreateGitHubRepositoryWithData()
    {
        var mock = CreateGitHubRepository();
        
        // Setup with realistic test data
        mock.Setup(x => x.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(GitHubPrTool.TestUtilities.Builders.RepositoryDataBuilder.CreateMany(5));
        
        mock.Setup(x => x.GetPullRequestsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GitHubPrTool.TestUtilities.Builders.PullRequestDataBuilder.CreateMany(3));
        
        mock.Setup(x => x.GetCommentsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GitHubPrTool.TestUtilities.Builders.CommentDataBuilder.CreateMany(10));
        
        return mock;
    }

    /// <summary>
    /// Create a mock ICacheService with basic functionality
    /// </summary>
    public static Mock<ICacheService> CreateCacheService()
    {
        var mock = new Mock<ICacheService>();
        
        // Setup basic cache operations
        mock.Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);
        
        mock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        mock.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        mock.Setup(x => x.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock;
    }

    /// <summary>
    /// Create a mock IAuthService
    /// </summary>
    public static Mock<IAuthService> CreateAuthService(bool isAuthenticated = false)
    {
        var mock = new Mock<IAuthService>();
        
        mock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        mock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(isAuthenticated ? GitHubPrTool.TestUtilities.Builders.UserDataBuilder.Create() : null);
        
        mock.Setup(x => x.LoadAuthenticationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(isAuthenticated);
        
        return mock;
    }

    /// <summary>
    /// Create a mock IDataSyncService
    /// </summary>
    public static Mock<IDataSyncService> CreateDataSyncService()
    {
        var mock = new Mock<IDataSyncService>();
        
        mock.Setup(x => x.SyncRepositoriesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        mock.Setup(x => x.SyncPullRequestsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock;
    }

    /// <summary>
    /// Create a mock INetworkConnectivityService
    /// </summary>
    public static Mock<INetworkConnectivityService> CreateNetworkConnectivityService(bool isConnected = true)
    {
        var mock = new Mock<INetworkConnectivityService>();
        
        mock.Setup(x => x.IsConnected).Returns(isConnected);
        mock.Setup(x => x.CheckConnectivityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(isConnected);
        
        mock.Setup(x => x.StartMonitoring())
            .Verifiable();
        
        mock.Setup(x => x.StopMonitoring())
            .Verifiable();
        
        return mock;
    }

    /// <summary>
    /// Create a mock ISearchService
    /// </summary>
    public static Mock<ISearchService> CreateSearchService()
    {
        var mock = new Mock<ISearchService>();
        
        mock.Setup(x => x.SearchAsync(It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResults<object>
            {
                Items = new List<object>(),
                TotalCount = 0,
                HasNextPage = false,
                HasPreviousPage = false
            });
        
        return mock;
    }

    /// <summary>
    /// Create a mock IAIService
    /// </summary>
    public static Mock<IAIService> CreateAIService()
    {
        var mock = new Mock<IAIService>();
        
        mock.Setup(x => x.AnalyzeCommentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIInsight 
            { 
                Id = 1, 
                Category = "General", 
                Confidence = 0.8f,
                Summary = "AI analysis result"
            });
        
        return mock;
    }

    /// <summary>
    /// Create mocks for a complete service setup
    /// </summary>
    public static ServiceMocks CreateCompleteServiceMocks(bool authenticated = true, bool connected = true)
    {
        return new ServiceMocks
        {
            GitHubRepository = CreateGitHubRepositoryWithData(),
            CacheService = CreateCacheService(),
            AuthService = CreateAuthService(authenticated),
            DataSyncService = CreateDataSyncService(),
            NetworkConnectivity = CreateNetworkConnectivityService(connected),
            SearchService = CreateSearchService(),
            AIService = CreateAIService()
        };
    }
}

/// <summary>
/// Container for all service mocks
/// </summary>
public class ServiceMocks
{
    public Mock<IGitHubRepository> GitHubRepository { get; set; } = null!;
    public Mock<ICacheService> CacheService { get; set; } = null!;
    public Mock<IAuthService> AuthService { get; set; } = null!;
    public Mock<IDataSyncService> DataSyncService { get; set; } = null!;
    public Mock<INetworkConnectivityService> NetworkConnectivity { get; set; } = null!;
    public Mock<ISearchService> SearchService { get; set; } = null!;
    public Mock<IAIService> AIService { get; set; } = null!;
}
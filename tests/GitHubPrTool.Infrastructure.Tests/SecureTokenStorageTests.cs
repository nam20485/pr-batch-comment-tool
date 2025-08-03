using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Infrastructure.Tests;

/// <summary>
/// Tests for SecureTokenStorage functionality
/// </summary>
public class SecureTokenStorageTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
    private readonly Mock<IDataProtector> _mockDataProtector;
    private readonly Mock<ILogger<SecureTokenStorage>> _mockLogger;
    private readonly SecureTokenStorage _tokenStorage;

    public SecureTokenStorageTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        _mockDataProtector = new Mock<IDataProtector>();
        _mockLogger = new Mock<ILogger<SecureTokenStorage>>();

        // Setup data protection provider to return mock data protector
        _mockDataProtectionProvider
            .Setup(x => x.CreateProtector("GitHubPrTool.Tokens"))
            .Returns(_mockDataProtector.Object);

        _tokenStorage = new SecureTokenStorage(
            _mockCacheService.Object,
            _mockDataProtectionProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecureTokenStorage(
            null!,
            _mockDataProtectionProvider.Object,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecureTokenStorage(
            _mockCacheService.Object,
            _mockDataProtectionProvider.Object,
            null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesDataProtector()
    {
        // Arrange & Act (constructor already called in setup)
        
        // Assert
        _mockDataProtectionProvider.Verify(
            x => x.CreateProtector("GitHubPrTool.Tokens"),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task StoreTokenAsync_WithEmptyOrWhitespaceKey_ThrowsArgumentException(string invalidKey)
    {
        // Arrange
        const string token = "test-token";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _tokenStorage.StoreTokenAsync(invalidKey, token));
    }

    [Fact]
    public async Task StoreTokenAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        const string token = "test-token";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _tokenStorage.StoreTokenAsync(null!, token));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task StoreTokenAsync_WithEmptyOrWhitespaceToken_ThrowsArgumentException(string invalidToken)
    {
        // Arrange
        const string key = "test-key";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _tokenStorage.StoreTokenAsync(key, invalidToken));
    }

    [Fact]
    public async Task StoreTokenAsync_WithNullToken_ThrowsArgumentNullException()
    {
        // Arrange
        const string key = "test-key";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _tokenStorage.StoreTokenAsync(key, null!));
    }

    [Fact] 
    public async Task RetrieveTokenAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        const string key = "non-existent-key";
        
        _mockCacheService
            .Setup(x => x.GetAsync<string>($"token_{key}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _tokenStorage.RetrieveTokenAsync(key);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RetrieveTokenAsync_WithEmptyOrWhitespaceKey_ThrowsArgumentException(string invalidKey)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _tokenStorage.RetrieveTokenAsync(invalidKey));
    }

    [Fact]
    public async Task RetrieveTokenAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert  
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _tokenStorage.RetrieveTokenAsync(null!));
    }

    [Fact]
    public async Task RetrieveTokenAsync_WhenCacheThrowsException_ReturnsNull()
    {
        // Arrange
        const string key = "test-key";
        
        _mockCacheService
            .Setup(x => x.GetAsync<string>($"token_{key}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await _tokenStorage.RetrieveTokenAsync(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveTokenAsync_WithValidKey_RemovesTokenFromCache()
    {
        // Arrange
        const string key = "test-key";

        // Act
        await _tokenStorage.RemoveTokenAsync(key);

        // Assert
        _mockCacheService.Verify(
            x => x.RemoveAsync($"token_{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RemoveTokenAsync_WithEmptyOrWhitespaceKey_ThrowsArgumentException(string invalidKey)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _tokenStorage.RemoveTokenAsync(invalidKey));
    }

    [Fact]
    public async Task RemoveTokenAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _tokenStorage.RemoveTokenAsync(null!));
    }

    [Fact]
    public async Task RemoveTokenAsync_WhenCacheServiceThrows_PropagatesException()
    {
        // Arrange
        const string key = "test-key";
        
        _mockCacheService
            .Setup(x => x.RemoveAsync($"token_{key}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _tokenStorage.RemoveTokenAsync(key));
    }

    [Fact]
    public async Task HasTokenAsync_WithExistingToken_ReturnsTrue()
    {
        // Arrange
        const string key = "test-key";
        const string encryptedToken = "encrypted-test-token";
        
        _mockCacheService
            .Setup(x => x.GetAsync<string>($"token_{key}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(encryptedToken);

        // Act
        var result = await _tokenStorage.HasTokenAsync(key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasTokenAsync_WithNonExistentToken_ReturnsFalse()
    {
        // Arrange
        const string key = "non-existent-key";
        
        _mockCacheService
            .Setup(x => x.GetAsync<string>($"token_{key}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _tokenStorage.HasTokenAsync(key);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HasTokenAsync_WithEmptyOrWhitespaceKey_ThrowsArgumentException(string invalidKey)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _tokenStorage.HasTokenAsync(invalidKey));
    }

    [Fact]
    public async Task HasTokenAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _tokenStorage.HasTokenAsync(null!));
    }

    [Fact]
    public async Task HasTokenAsync_WhenCacheServiceThrows_ReturnsFalse()
    {
        // Arrange
        const string key = "test-key";
        
        _mockCacheService
            .Setup(x => x.GetAsync<string>($"token_{key}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _tokenStorage.HasTokenAsync(key);

        // Assert
        Assert.False(result);
    }
}
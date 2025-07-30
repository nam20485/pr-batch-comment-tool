using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubPrTool.Infrastructure.Tests.Services;

/// <summary>
/// Tests for the GeminiService class
/// </summary>
public class GeminiServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<GeminiService>> _mockLogger;

    public GeminiServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<GeminiService>>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
        _mockConfiguration.Setup(c => c["Gemini:BaseUrl"]).Returns("https://test.googleapis.com/v1beta");
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        var service = new GeminiService(new HttpClient(), _mockConfiguration.Object, _mockLogger.Object);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ShouldThrow()
    {
        // Arrange
        var configWithoutApiKey = new Mock<IConfiguration>();
        configWithoutApiKey.Setup(c => c["Gemini:ApiKey"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new GeminiService(new HttpClient(), configWithoutApiKey.Object, _mockLogger.Object));
    }

    [Fact]
    public async Task GenerateCommentSuggestionsAsync_WithValidInput_ShouldNotThrowArgumentException()
    {
        // Arrange
        var service = new GeminiService(new HttpClient(), _mockConfiguration.Object, _mockLogger.Object);
        var codeContext = "public void TestMethod() { }";

        // Act & Assert
        // This will fail with network call, but we're testing the method signature and basic validation
        try
        {
            await service.GenerateCommentSuggestionsAsync(codeContext);
        }
        catch (ArgumentException)
        {
            Assert.True(false, "Should not throw ArgumentException for valid input");
        }
        catch (Exception)
        {
            // Expected - network call will fail in test environment
            Assert.True(true);
        }
    }
}
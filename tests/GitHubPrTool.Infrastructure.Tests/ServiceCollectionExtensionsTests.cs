using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Services;
using GitHubPrTool.Infrastructure.Configuration;
using GitHubPrTool.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace GitHubPrTool.Infrastructure.Tests;

public class ServiceCollectionExtensionsTests
{
    private class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "GitHubPrTool";
        public string ContentRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    [Fact]
    public void AddGitHubPrToolServicesForTesting_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestEnvironment();

        // Act
        services.AddGitHubPrToolServicesForTesting(configuration, environment);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify DbContext is registered and can be resolved
        var dbContext = serviceProvider.GetService<GitHubPrToolDbContext>();
        Assert.NotNull(dbContext);

        // Verify core services are registered
        var commentService = serviceProvider.GetService<CommentService>();
        Assert.NotNull(commentService);

        // Verify logging is configured
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);

        // Verify HTTP client is registered
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);

        // Verify services are registered in DI container (not necessarily instantiated)
        Assert.Contains(services, s => s.ServiceType == typeof(ICacheService));
        Assert.Contains(services, s => s.ServiceType == typeof(IGitHubRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IAuthService));
    }

    [Fact]
    public void AddGitHubPrToolServices_WithConfiguration_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"}
            })
            .Build();
        var environment = new TestEnvironment();

        // Act
        services.AddGitHubPrToolServices(configuration, environment);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all core services are registered
        Assert.NotNull(serviceProvider.GetService<GitHubPrToolDbContext>());
        Assert.NotNull(serviceProvider.GetService<CommentService>());
        Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        Assert.NotNull(serviceProvider.GetService<IHttpClientFactory>());

        // Verify services are registered in DI container
        Assert.Contains(services, s => s.ServiceType == typeof(ICacheService));
        Assert.Contains(services, s => s.ServiceType == typeof(IGitHubRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IAuthService));
    }

    [Fact]
    public void AddGitHubPrToolServicesForTesting_WithNullParameters_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGitHubPrToolServicesForTesting();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify services are still registered with defaults
        var dbContext = serviceProvider.GetService<GitHubPrToolDbContext>();
        Assert.NotNull(dbContext);

        var commentService = serviceProvider.GetService<CommentService>();
        Assert.NotNull(commentService);
    }

    [Fact]
    public void AddGitHubPrToolServicesForTesting_UsesInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGitHubPrToolServicesForTesting();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<GitHubPrToolDbContext>();

        // Verify it's using in-memory database
        Assert.True(dbContext.Database.IsInMemory());
    }
}
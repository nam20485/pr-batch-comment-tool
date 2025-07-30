using GitHubPrTool.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GitHubPrTool.Infrastructure.Tests;

public class LoggingConfigurationTests
{
    private class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "GitHubPrTool";
        public string ContentRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    [Fact]
    public void ConfigureSerilog_WithValidEnvironment_ReturnsConfiguredLogger()
    {
        // Arrange
        var environment = new TestEnvironment { EnvironmentName = "Development" };

        // Act
        var loggerConfig = LoggingConfiguration.ConfigureSerilog(null, environment);
        var logger = loggerConfig.CreateLogger();

        // Assert
        Assert.NotNull(logger);
        Assert.IsType<Serilog.Core.Logger>(logger);
    }

    [Fact]
    public void AddSerilogLogging_WithValidServices_AddsLoggingToServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = new TestEnvironment { EnvironmentName = "Test" };

        // Act
        services.AddSerilogLogging(null, environment);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Check that Serilog logger factory is registered
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);
        
        // Check that we can get a logger instance
        var logger = loggerFactory.CreateLogger<LoggingConfigurationTests>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureSerilog_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Serilog:MinimumLevel:Default", "Information"}
            })
            .Build();

        var environment = new TestEnvironment { EnvironmentName = "Production" };

        // Act
        var loggerConfig = LoggingConfiguration.ConfigureSerilog(configuration, environment);
        var logger = loggerConfig.CreateLogger();

        // Assert
        Assert.NotNull(logger);
        Assert.IsType<Serilog.Core.Logger>(logger);
    }
}
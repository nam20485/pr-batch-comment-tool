using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Provides configuration for Serilog structured logging.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with structured logging to console and file outputs.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The configured logger configuration.</returns>
    public static LoggerConfiguration ConfigureSerilog(IConfiguration? configuration, IHostEnvironment environment)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "GitHubPrTool")
            .Enrich.WithProperty("Environment", environment.EnvironmentName);

        // Apply configuration if provided
        if (configuration != null)
        {
            loggerConfig.ReadFrom.Configuration(configuration);
        }

        // Set minimum level based on environment
        var minimumLevel = environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
        loggerConfig.MinimumLevel.Is(minimumLevel);

        // Override specific namespace log levels
        loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("System", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);

        // Console sink with structured output
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // File sink with rolling policies
        var logsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GitHubPrTool",
            "logs",
            "log-.txt");

        loggerConfig.WriteTo.File(
            path: logsPath,
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        return loggerConfig;
    }

    /// <summary>
    /// Adds Serilog to the service collection with proper configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration? configuration,
        IHostEnvironment environment)
    {
        var logger = ConfigureSerilog(configuration, environment).CreateLogger();
        
        Log.Logger = logger;
        
        services.AddSerilog(logger);
        
        return services;
    }
}
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Services;
using GitHubPrTool.Infrastructure.Data;
using GitHubPrTool.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Provides dependency injection configuration for the GitHub PR Tool services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all GitHub PR Tool services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGitHubPrToolServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Add structured logging
        services.AddSerilogLogging(configuration, environment);

        // Add Entity Framework DbContext
        services.AddDbContext<GitHubPrToolDbContext>(options =>
        {
            var connectionString = GetConnectionString(configuration, environment);
            options.UseSqlite(connectionString);
            
            // Enable sensitive data logging in development
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add Octokit GitHub client
        services.AddSingleton<IGitHubClient>(provider =>
        {
            var productHeaderValue = new ProductHeaderValue("GitHubPrTool");
            return new GitHubClient(productHeaderValue);
        });

        // Add core services
        services.AddScoped<CommentService>();

        // Add infrastructure services
        services.AddScoped<ICacheService, SqliteCacheService>();
        services.AddScoped<IGitHubRepository, GitHubRepositoryService>();
        services.AddScoped<IAuthService, GitHubAuthService>();

        // Add HTTP client for external API calls
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Adds GitHub PR Tool services for testing with in-memory database.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGitHubPrToolServicesForTesting(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        IHostEnvironment? environment = null)
    {
        // Create default test environment if not provided
        environment ??= new TestHostEnvironment();
        
        // Add structured logging
        services.AddSerilogLogging(configuration, environment);

        // Add Entity Framework DbContext with in-memory database
        services.AddDbContext<GitHubPrToolDbContext>(options =>
        {
            options.UseInMemoryDatabase($"GitHubPrTool_Test_{Guid.NewGuid()}");
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Add Octokit GitHub client
        services.AddSingleton<IGitHubClient>(provider =>
        {
            var productHeaderValue = new ProductHeaderValue("GitHubPrTool");
            return new GitHubClient(productHeaderValue);
        });

        // Add core services
        services.AddScoped<CommentService>();

        // Add infrastructure services
        services.AddScoped<ICacheService, SqliteCacheService>();
        services.AddScoped<IGitHubRepository, GitHubRepositoryService>();
        services.AddScoped<IAuthService, GitHubAuthService>();

        // Add HTTP client for external API calls
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Gets the database connection string based on configuration and environment.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The connection string.</returns>
    private static string GetConnectionString(IConfiguration configuration, IHostEnvironment environment)
    {
        // Try to get connection string from configuration first
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Fall back to default location in user's local app data
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(appDataPath, "GitHubPrTool", "Data", "githubprtool.db");
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(dbPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return $"Data Source={dbPath}";
    }

    /// <summary>
    /// Test implementation of IHostEnvironment for unit testing.
    /// </summary>
    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "GitHubPrTool";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
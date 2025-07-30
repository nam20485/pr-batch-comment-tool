using GitHubPrTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Service for managing database connection resilience and health
/// </summary>
public class DatabaseResilienceService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseResilienceService> _logger;

    public DatabaseResilienceService(IServiceProvider serviceProvider, ILogger<DatabaseResilienceService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute a database operation with retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Database operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    public async Task<T> ExecuteWithResilienceAsync<T>(
        Func<GitHubPrToolDbContext, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GitHubPrToolDbContext>();
                
                // Ensure database is available
                await EnsureDatabaseAvailableAsync(dbContext, cancellationToken);
                
                return await operation(dbContext);
            }
            catch (Exception ex) when (IsDatabaseRelatedError(ex) && retryCount < maxRetries - 1)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                
                _logger.LogWarning("Database operation failed. Retry {RetryCount} in {Delay} seconds. Error: {Error}",
                    retryCount, delay.TotalSeconds, ex.Message);
                
                await Task.Delay(delay, cancellationToken);
            }
        }

        // If we get here, all retries failed
        throw new InvalidOperationException($"Database operation failed after {maxRetries} retries");
    }

    /// <summary>
    /// Execute a database operation with retry logic (void return)
    /// </summary>
    /// <param name="operation">Database operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteWithResilienceAsync(
        Func<GitHubPrToolDbContext, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithResilienceAsync(async context =>
        {
            await operation(context);
            return true; // Dummy return value
        }, cancellationToken);
    }

    /// <summary>
    /// Check if the database is healthy and accessible
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if database is healthy, false otherwise</returns>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GitHubPrToolDbContext>();
            
            // Try to execute a simple query to check connectivity
            await dbContext.Database.CanConnectAsync(cancellationToken);
            
            _logger.LogDebug("Database health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return false;
        }
    }

    /// <summary>
    /// Ensure database exists and run any pending migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task EnsureDatabaseReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GitHubPrToolDbContext>();
            
            _logger.LogInformation("Ensuring database is ready");
            
            // Ensure database exists
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            
            // Apply any pending migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations completed");
            }
            else
            {
                _logger.LogDebug("No pending migrations found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure database is ready");
            throw;
        }
    }

    /// <summary>
    /// Test database connectivity with timeout
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection successful within timeout</returns>
    public async Task<bool> TestConnectivityAsync(int timeoutSeconds = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GitHubPrToolDbContext>();
            
            return await dbContext.Database.CanConnectAsync(combinedCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw if it's the original cancellation token
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database connectivity test timed out after {Timeout} seconds", timeoutSeconds);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connectivity test failed");
            return false;
        }
    }

    private async Task EnsureDatabaseAvailableAsync(GitHubPrToolDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            // Test connection with a short timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            if (!await dbContext.Database.CanConnectAsync(combinedCts.Token))
            {
                throw new InvalidOperationException("Cannot connect to database");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw if it's the original cancellation token
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Database connection timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database availability check failed");
            throw;
        }
    }

    private bool IsDatabaseRelatedError(Exception exception)
    {
        // Check for common database-related exceptions
        return exception switch
        {
            TimeoutException => true,
            InvalidOperationException ex when ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase) => true,
            InvalidOperationException ex when ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            Microsoft.Data.Sqlite.SqliteException => true,
            System.Data.Common.DbException => true,
            _ => false
        };
    }
}
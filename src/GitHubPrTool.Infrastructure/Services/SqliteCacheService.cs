using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// SQLite-based cache service implementation
/// </summary>
public class SqliteCacheService : ICacheService
{
    private readonly GitHubPrToolDbContext _context;
    private readonly ILogger<SqliteCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqliteCacheService(GitHubPrToolDbContext context, ILogger<SqliteCacheService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        EnsureCacheTableExists();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var cacheEntry = await _context.Database
                .SqlQueryRaw<CacheEntry>("SELECT Key, Value, ExpiresAt FROM CacheEntries WHERE Key = {0}", key)
                .FirstOrDefaultAsync(cancellationToken);

            if (cacheEntry == null)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            // Check if expired
            if (cacheEntry.ExpiresAt.HasValue && cacheEntry.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            {
                _logger.LogDebug("Cache entry expired for key: {Key}", key);
                await RemoveAsync(key, cancellationToken);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(cacheEntry.Value, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache entry for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var expiresAt = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : (DateTimeOffset?)null;

            await _context.Database.ExecuteSqlRawAsync(
                @"INSERT OR REPLACE INTO CacheEntries (Key, Value, ExpiresAt, CreatedAt, UpdatedAt) 
                  VALUES ({0}, {1}, {2}, {3}, {4})",
                [key, json, expiresAt?.ToString("O") ?? string.Empty, DateTimeOffset.UtcNow.ToString("O"), DateTimeOffset.UtcNow.ToString("O")]);

            _logger.LogDebug("Cache entry set for key: {Key}, expires: {ExpiresAt}", key, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache entry for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CacheEntries WHERE Key = {0}", key);

            _logger.LogDebug("Cache entry removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        try
        {
            // Convert simple wildcard pattern to SQL LIKE pattern
            var likePattern = pattern.Replace("*", "%");

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CacheEntries WHERE Key LIKE {0}", likePattern);

            _logger.LogDebug("Cache entries removed for pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries for pattern: {Pattern}", pattern);
            throw;
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CacheEntries");
            _logger.LogInformation("All cache entries cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache entries");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var exists = await _context.Database
                .SqlQueryRaw<int>("SELECT 1 FROM CacheEntries WHERE Key = {0} LIMIT 1", key)
                .AnyAsync(cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache entry existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _context.Database
                .SqlQueryRaw<CacheStatsResult>(@"
                    SELECT 
                        COUNT(*) AS TotalItems,
                        SUM(LENGTH(Value)) AS TotalSizeBytes
                    FROM CacheEntries 
                    WHERE ExpiresAt IS NULL OR ExpiresAt > {0}", DateTimeOffset.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            return new CacheStatistics
            {
                TotalItems = stats?.TotalItems ?? 0,
                TotalSizeBytes = stats?.TotalSizeBytes ?? 0,
                HitCount = 0, // Would need additional tracking for hit/miss counts
                MissCount = 0,
                LastUpdated = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return new CacheStatistics { LastUpdated = DateTimeOffset.UtcNow };
        }
    }

    private void EnsureCacheTableExists()
    {
        try
        {
            _context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS CacheEntries (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL,
                    ExpiresAt TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                )");

            _context.Database.ExecuteSqlRaw(@"
                CREATE INDEX IF NOT EXISTS IX_CacheEntries_ExpiresAt 
                ON CacheEntries(ExpiresAt)");

            _logger.LogDebug("Cache table initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing cache table");
            throw;
        }
    }

    private class CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset? ExpiresAt { get; set; }
    }

    private class CacheStatsResult
    {
        public long TotalItems { get; set; }
        public long TotalSizeBytes { get; set; }
    }
}
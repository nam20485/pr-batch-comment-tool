namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for local caching operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached item by key
    /// </summary>
    /// <typeparam name="T">Type of the cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached item if found, default(T) otherwise</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a cached item with expiration
    /// </summary>
    /// <typeparam name="T">Type of the item to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Cache expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove a cached item
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all cached items matching a pattern
    /// </summary>
    /// <param name="pattern">Key pattern (supports wildcards)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all cached items
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a key exists in the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cache statistics</returns>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics information
/// </summary>
public record CacheStatistics
{
    /// <summary>
    /// Total number of cached items
    /// </summary>
    public long TotalItems { get; init; }

    /// <summary>
    /// Total cache size in bytes
    /// </summary>
    public long TotalSizeBytes { get; init; }

    /// <summary>
    /// Number of cache hits
    /// </summary>
    public long HitCount { get; init; }

    /// <summary>
    /// Number of cache misses
    /// </summary>
    public long MissCount { get; init; }

    /// <summary>
    /// Cache hit ratio (0.0 to 1.0)
    /// </summary>
    public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0.0;

    /// <summary>
    /// Date when statistics were last updated
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; }
}
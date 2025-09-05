namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for caching data in memory or distributed cache
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached item by key
    /// </summary>
    /// <typeparam name="T">Type of the cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached item or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a cache item with expiration
    /// </summary>
    /// <typeparam name="T">Type of the item to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Cache expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a cache item with absolute expiration
    /// </summary>
    /// <typeparam name="T">Type of the item to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="absoluteExpiration">Absolute expiration date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove a cached item by key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove multiple cached items by pattern
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "events:*")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Infrastructure.Caching;

/// <summary>
/// Implementation of ICacheService using IMemoryCache
/// This will be upgraded to Redis in production
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out var value))
            {
                if (value is string json)
                {
                    var result = JsonSerializer.Deserialize<T>(json);
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult(result);
                }
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving item from cache with key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = null
            };

            _cache.Set(key, json, options);
            _logger.LogDebug("Item cached with key: {Key}, expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting item in cache with key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = null
            };

            _cache.Set(key, json, options);
            _logger.LogDebug("Item cached with key: {Key}, absolute expiration: {AbsoluteExpiration}", key, absoluteExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting item in cache with key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Item removed from cache with key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing item from cache with key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: IMemoryCache doesn't support pattern-based removal
        // This would be implemented properly with Redis
        _logger.LogWarning("Pattern-based cache removal not supported with IMemoryCache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }
}
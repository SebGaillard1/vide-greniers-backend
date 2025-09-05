using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Common.Interfaces;
using System.Text.Json;

namespace VideGreniers.Application.Common.Behaviors;

/// <summary>
/// Interface to mark queries as cacheable
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? CacheTime { get; }
}

/// <summary>
/// Pipeline behavior for caching query responses
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ICacheableQuery
    where TResponse : IErrorOr
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var cacheTime = request.CacheTime ?? TimeSpan.FromMinutes(5);

        // Try to get from cache
        try
        {
            var cachedResponse = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            
            if (cachedResponse != null)
            {
                var deserializedResponse = JsonSerializer.Deserialize<TResponse>(cachedResponse);
                if (deserializedResponse != null)
                {
                    _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
                    return deserializedResponse;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve from cache for key {CacheKey}", cacheKey);
        }

        // Execute the request
        var response = await next();

        // Cache the response if successful
        if (!response.IsError)
        {
            try
            {
                var serializedResponse = JsonSerializer.Serialize(response);
                await _cacheService.SetAsync(cacheKey, serializedResponse, cacheTime, cancellationToken);
                _logger.LogDebug("Cached response for key {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache response for key {CacheKey}", cacheKey);
            }
        }

        return response;
    }
}
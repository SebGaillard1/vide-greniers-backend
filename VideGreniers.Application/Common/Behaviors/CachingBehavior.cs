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
                // We need to get the type parameter of TResponse to deserialize correctly
                var responseType = typeof(TResponse);
                
                // For ErrorOr<T>, we need to extract T and deserialize to that type
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
                {
                    var valueType = responseType.GetGenericArguments()[0];
                    var deserializedValue = JsonSerializer.Deserialize(cachedResponse, valueType);
                    
                    if (deserializedValue != null)
                    {
                        // Use reflection to create ErrorOr<T> using implicit conversion from T
                        var implicitOperatorMethod = responseType.GetMethod("op_Implicit", new[] { valueType });
                        if (implicitOperatorMethod != null)
                        {
                            var errorOrResult = implicitOperatorMethod.Invoke(null, new[] { deserializedValue });
                            if (errorOrResult != null)
                            {
                                _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
                                return (TResponse)errorOrResult;
                            }
                        }
                    }
                }
                else
                {
                    // Fallback for non-ErrorOr types
                    var deserializedResponse = JsonSerializer.Deserialize<TResponse>(cachedResponse);
                    if (deserializedResponse != null)
                    {
                        _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
                        return deserializedResponse;
                    }
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
                // Extract the actual value from the ErrorOr response for serialization
                // Use reflection to get the Value property since TResponse is constrained to IErrorOr
                var valueProperty = typeof(TResponse).GetProperty("Value");
                var valueToCache = valueProperty?.GetValue(response);
                var serializedResponse = JsonSerializer.Serialize(valueToCache);
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
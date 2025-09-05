namespace VideGreniers.Application.Common.Models;

/// <summary>
/// Static class for cache key constants and builders
/// </summary>
public static class CacheKeys
{
    public const string EventPrefix = "events";
    public const string UserPrefix = "users";
    public const string FavoritePrefix = "favorites";
    public const string CategoryPrefix = "categories";
    
    // Event cache keys
    public static string EventById(Guid id) => $"{EventPrefix}:id:{id}";
    public static string EventsList(int page, int pageSize, string? search = null, Guid? categoryId = null) 
        => $"{EventPrefix}:list:page_{page}:size_{pageSize}:search_{search}:category_{categoryId}";
    public static string NearbyEvents(double lat, double lng, int radiusKm, int limit) 
        => $"{EventPrefix}:nearby:lat_{lat:F4}:lng_{lng:F4}:radius_{radiusKm}:limit_{limit}";
    public static string EventsByOrganizer(Guid organizerId, int page, int pageSize) 
        => $"{EventPrefix}:organizer_{organizerId}:page_{page}:size_{pageSize}";
    
    // User cache keys
    public static string UserById(Guid id) => $"{UserPrefix}:id:{id}";
    public static string UserFavorites(Guid userId, int page, int pageSize) 
        => $"{FavoritePrefix}:user_{userId}:page_{page}:size_{pageSize}";
    
    // Category cache keys
    public static string AllCategories => $"{CategoryPrefix}:all";
    public static string CategoryById(Guid id) => $"{CategoryPrefix}:id:{id}";
    
    // Cache invalidation patterns
    public static string AllEventsPattern => $"{EventPrefix}:*";
    public static string UserEventsPattern(Guid userId) => $"{EventPrefix}:*user_{userId}*";
    public static string UserFavoritesPattern(Guid userId) => $"{FavoritePrefix}:user_{userId}:*";
}
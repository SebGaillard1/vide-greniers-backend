using ErrorOr;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Domain.Services;

public interface IFavoriteService
{
    /// <summary>
    /// Add an event to user's favorites
    /// </summary>
    Task<ErrorOr<Favorite>> AddToFavorites(Guid userId, Guid eventId, string? notes = null);

    /// <summary>
    /// Remove an event from user's favorites
    /// </summary>
    Task<ErrorOr<Success>> RemoveFromFavorites(Guid userId, Guid eventId);

    /// <summary>
    /// Archive a favorite (keep for history but hide from active list)
    /// </summary>
    Task<ErrorOr<Success>> ArchiveFavorite(Guid userId, Guid eventId);

    /// <summary>
    /// Restore an archived favorite
    /// </summary>
    Task<ErrorOr<Success>> RestoreFavorite(Guid userId, Guid eventId);

    /// <summary>
    /// Check if user has favorited an event
    /// </summary>
    Task<bool> IsEventFavorited(Guid userId, Guid eventId);

    /// <summary>
    /// Get count of active favorites for an event
    /// </summary>
    Task<int> GetEventFavoritesCount(Guid eventId);

    /// <summary>
    /// Get user's favorite events with filtering options
    /// </summary>
    Task<IEnumerable<Favorite>> GetUserFavorites(
        Guid userId, 
        bool includeArchived = false,
        bool onlyUpcoming = false);

    /// <summary>
    /// Auto-archive favorites for events that have ended
    /// </summary>
    Task<int> ArchiveExpiredFavorites();

    /// <summary>
    /// Clean up favorites for deleted events
    /// </summary>
    Task<int> CleanupFavoritesForDeletedEvents();

    /// <summary>
    /// Get favorites statistics for a user
    /// </summary>
    Task<FavoriteStatistics> GetUserFavoriteStatistics(Guid userId);
}

public record FavoriteStatistics(
    int TotalFavorites,
    int ActiveFavorites,
    int ArchivedFavorites,
    int UpcomingEvents,
    int CompletedEvents,
    DateTime? MostRecentlyAdded,
    int FavoritesThisMonth);
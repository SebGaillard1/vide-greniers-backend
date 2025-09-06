using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for tracking user activities and analytics
/// </summary>
public interface IUserActivityService
{
    /// <summary>
    /// Track an event-related user activity
    /// </summary>
    Task TrackEventActivityAsync(
        Guid userId,
        UserActivityType activityType,
        Guid eventId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track a search-related user activity
    /// </summary>
    Task TrackSearchActivityAsync(
        Guid userId,
        string searchTerm,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track a general user activity (login, logout, profile update, etc.)
    /// </summary>
    Task TrackUserActivityAsync(
        Guid userId,
        UserActivityType activityType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track activity asynchronously in the background (fire and forget)
    /// </summary>
    void TrackActivityAsync(
        Guid userId,
        UserActivityType activityType,
        Guid? eventId = null,
        string? searchTerm = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null);
}
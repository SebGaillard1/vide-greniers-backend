using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Entities;

/// <summary>
/// Represents a user activity log entry for tracking user behavior and analytics
/// </summary>
public sealed class UserActivity : BaseAuditableEntity
{
    private UserActivity() { } // EF Core constructor

    private UserActivity(
        Guid userId,
        UserActivityType activityType,
        string? eventId = null,
        string? searchTerm = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        UserId = userId;
        ActivityType = activityType;
        EventId = eventId != null ? Guid.Parse(eventId) : null;
        SearchTerm = searchTerm;
        Metadata = metadata;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    /// <summary>
    /// ID of the user who performed the activity
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Type of activity performed
    /// </summary>
    public UserActivityType ActivityType { get; private set; }

    /// <summary>
    /// Related event ID (if activity is event-related)
    /// </summary>
    public Guid? EventId { get; private set; }

    /// <summary>
    /// Search term used (if activity is search-related)
    /// </summary>
    public string? SearchTerm { get; private set; }

    /// <summary>
    /// Additional metadata as JSON string (coordinates, filters, etc.)
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// IP address where the activity originated
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Navigation property to Event (if applicable)
    /// </summary>
    public Event? Event { get; private set; }

    /// <summary>
    /// Create a new user activity log entry
    /// </summary>
    public static UserActivity Create(
        Guid userId,
        UserActivityType activityType,
        Guid? eventId = null,
        string? searchTerm = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserActivity(
            userId,
            activityType,
            eventId?.ToString(),
            searchTerm,
            metadata,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Create an event-related activity
    /// </summary>
    public static UserActivity CreateEventActivity(
        Guid userId,
        UserActivityType activityType,
        Guid eventId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return Create(userId, activityType, eventId, null, null, ipAddress, userAgent);
    }

    /// <summary>
    /// Create a search-related activity
    /// </summary>
    public static UserActivity CreateSearchActivity(
        Guid userId,
        string searchTerm,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return Create(userId, UserActivityType.EventSearched, null, searchTerm, metadata, ipAddress, userAgent);
    }

    /// <summary>
    /// Create a general user activity (login, logout, profile update)
    /// </summary>
    public static UserActivity CreateUserActivity(
        Guid userId,
        UserActivityType activityType,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return Create(userId, activityType, null, null, null, ipAddress, userAgent);
    }
}
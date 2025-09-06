namespace VideGreniers.Domain.Enums;

/// <summary>
/// Types of notifications that can be sent to users
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// General system information
    /// </summary>
    System,
    
    /// <summary>
    /// Event-related notifications (new event, updates, cancellation)
    /// </summary>
    Event,
    
    /// <summary>
    /// Event reminder notifications (upcoming events)
    /// </summary>
    EventReminder,
    
    /// <summary>
    /// Favorite-related notifications (favorite event updates)
    /// </summary>
    Favorite,
    
    /// <summary>
    /// User account notifications (profile updates, security)
    /// </summary>
    Account,
    
    /// <summary>
    /// Promotional and marketing notifications
    /// </summary>
    Marketing
}
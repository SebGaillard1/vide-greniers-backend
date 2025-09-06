namespace VideGreniers.Domain.Enums;

/// <summary>
/// Status of a notification
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification has been created but not yet sent
    /// </summary>
    Pending,
    
    /// <summary>
    /// Notification has been sent successfully
    /// </summary>
    Sent,
    
    /// <summary>
    /// Notification has been read by the user
    /// </summary>
    Read,
    
    /// <summary>
    /// Notification failed to send
    /// </summary>
    Failed
}
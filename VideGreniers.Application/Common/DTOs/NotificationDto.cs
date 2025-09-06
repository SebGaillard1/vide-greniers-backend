using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for notification information
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public NotificationStatus Status { get; init; }
    public DateTimeOffset? ReadOnUtc { get; init; }
    public DateTimeOffset? SentOnUtc { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionText { get; init; }
    public string? ImageUrl { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
    
    // Navigation properties
    public Guid? EventId { get; init; }
    public string? EventTitle { get; init; }
    
    // Computed properties
    public bool IsUnread => Status != NotificationStatus.Read && !ReadOnUtc.HasValue;
    public string TypeDisplayName => Type switch
    {
        NotificationType.System => "System",
        NotificationType.Event => "Event",
        NotificationType.EventReminder => "Reminder",
        NotificationType.Favorite => "Favorite",
        NotificationType.Account => "Account",
        NotificationType.Marketing => "Marketing",
        _ => "Unknown"
    };
}
using ErrorOr;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;

namespace VideGreniers.Domain.Entities;

public sealed class Notification : BaseAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTimeOffset? ReadOnUtc { get; private set; }
    public DateTimeOffset? SentOnUtc { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? ActionText { get; private set; }
    public string? ImageUrl { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }

    // Foreign keys
    public Guid UserId { get; private set; }
    public Guid? EventId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Event? Event { get; private set; }

    // Private constructor for EF Core
    private Notification() { }

    private Notification(
        string title,
        string message,
        NotificationType type,
        Guid userId,
        Guid? eventId = null,
        string? actionUrl = null,
        string? actionText = null,
        string? imageUrl = null,
        Dictionary<string, object>? metadata = null)
    {
        Title = title;
        Message = message;
        Type = type;
        Status = NotificationStatus.Pending;
        UserId = userId;
        EventId = eventId;
        ActionUrl = actionUrl;
        ActionText = actionText;
        ImageUrl = imageUrl;
        Metadata = metadata;

        RaiseDomainEvent(new NotificationCreatedDomainEvent(Id, UserId, Type, Title));
    }

    public static ErrorOr<Notification> Create(
        string title,
        string message,
        NotificationType type,
        Guid userId,
        Guid? eventId = null,
        string? actionUrl = null,
        string? actionText = null,
        string? imageUrl = null,
        Dictionary<string, object>? metadata = null)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
        {
            errors.Add(Error.Validation("Notification.TitleTooShort", "Title must be at least 3 characters long"));
        }

        if (title?.Length > 200)
        {
            errors.Add(Error.Validation("Notification.TitleTooLong", "Title cannot exceed 200 characters"));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            errors.Add(Error.Validation("Notification.MessageRequired", "Message is required"));
        }

        if (message?.Length > 1000)
        {
            errors.Add(Error.Validation("Notification.MessageTooLong", "Message cannot exceed 1000 characters"));
        }

        if (userId == Guid.Empty)
        {
            errors.Add(Error.Validation("Notification.InvalidUser", "User ID is required"));
        }

        if (!string.IsNullOrEmpty(actionUrl) && !Uri.TryCreate(actionUrl, UriKind.RelativeOrAbsolute, out _))
        {
            errors.Add(Error.Validation("Notification.InvalidActionUrl", "Action URL format is invalid"));
        }

        if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            errors.Add(Error.Validation("Notification.InvalidImageUrl", "Image URL format is invalid"));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Notification(
            title!.Trim(),
            message!.Trim(),
            type,
            userId,
            eventId,
            actionUrl?.Trim(),
            actionText?.Trim(),
            imageUrl?.Trim(),
            metadata);
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read)
            return;

        Status = NotificationStatus.Read;
        ReadOnUtc = DateTimeOffset.UtcNow;
        MarkAsModified();

        RaiseDomainEvent(new NotificationReadDomainEvent(Id, UserId));
    }

    public void MarkAsSent()
    {
        if (Status != NotificationStatus.Pending)
            return;

        Status = NotificationStatus.Sent;
        SentOnUtc = DateTimeOffset.UtcNow;
        MarkAsModified();

        RaiseDomainEvent(new NotificationSentDomainEvent(Id, UserId, Type));
    }

    public void MarkAsFailed(string? error = null)
    {
        Status = NotificationStatus.Failed;
        if (error != null)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata["error"] = error;
        }
        MarkAsModified();

        RaiseDomainEvent(new NotificationFailedDomainEvent(Id, UserId, error));
    }

    public bool IsUnread()
    {
        return Status != NotificationStatus.Read && ReadOnUtc == null;
    }

    public bool CanBeRetried()
    {
        return Status == NotificationStatus.Failed;
    }

    public void AddMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        MarkAsModified();
    }

    public T? GetMetadata<T>(string key)
    {
        if (Metadata == null || !Metadata.TryGetValue(key, out var value))
            return default;

        return (T?)value;
    }
}
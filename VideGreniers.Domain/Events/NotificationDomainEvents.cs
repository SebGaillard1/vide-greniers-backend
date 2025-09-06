using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Events;

/// <summary>
/// Domain event raised when a notification is created
/// </summary>
public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type,
    string Title) : BaseDomainEvent;

/// <summary>
/// Domain event raised when a notification is read by the user
/// </summary>
public sealed record NotificationReadDomainEvent(
    Guid NotificationId,
    Guid UserId) : BaseDomainEvent;

/// <summary>
/// Domain event raised when a notification is successfully sent
/// </summary>
public sealed record NotificationSentDomainEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type) : BaseDomainEvent;

/// <summary>
/// Domain event raised when a notification fails to send
/// </summary>
public sealed record NotificationFailedDomainEvent(
    Guid NotificationId,
    Guid UserId,
    string? ErrorMessage) : BaseDomainEvent;
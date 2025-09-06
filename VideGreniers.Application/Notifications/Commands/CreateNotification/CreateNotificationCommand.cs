using ErrorOr;
using MediatR;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Notifications.Commands.CreateNotification;

/// <summary>
/// Command to create a new notification
/// </summary>
public record CreateNotificationCommand(
    string Title,
    string Message,
    NotificationType Type,
    Guid UserId,
    Guid? EventId = null,
    string? ActionUrl = null,
    string? ActionText = null,
    string? ImageUrl = null,
    Dictionary<string, object>? Metadata = null) : IRequest<ErrorOr<Guid>>;
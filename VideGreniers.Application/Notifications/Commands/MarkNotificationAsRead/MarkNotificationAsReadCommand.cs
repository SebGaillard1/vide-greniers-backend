using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Notifications.Commands.MarkNotificationAsRead;

/// <summary>
/// Command to mark a notification as read
/// </summary>
public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<ErrorOr<Success>>;
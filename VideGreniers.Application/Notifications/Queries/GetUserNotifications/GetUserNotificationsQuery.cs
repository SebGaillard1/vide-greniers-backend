using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Notifications.Queries.GetUserNotifications;

/// <summary>
/// Query to get user's notifications with pagination
/// </summary>
public record GetUserNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false) : IRequest<ErrorOr<PaginatedList<NotificationDto>>>;
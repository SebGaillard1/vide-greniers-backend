using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Notifications.Commands.CreateNotification;
using VideGreniers.Application.Notifications.Commands.MarkNotificationAsRead;
using VideGreniers.Application.Notifications.Queries.GetUserNotifications;

namespace VideGreniers.API.Controllers;

/// <summary>
/// Notifications management API endpoints
/// </summary>
[Authorize]
[Route("api/notifications")]
[Tags("Notifications")]
public class NotificationsController : ApiController
{
    /// <summary>
    /// Get current user's notifications
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="unreadOnly">Only return unread notifications</param>
    /// <returns>Paginated list of notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false)
    {
        var query = new GetUserNotificationsQuery(page, Math.Min(pageSize, 100), unreadOnly);
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var paginatedList = result.Value;
        var response = new PaginatedApiResponse<NotificationDto>
        {
            Data = paginatedList.Items,
            Success = true,
            Timestamp = DateTime.UtcNow,
            Pagination = new PaginationMetadata
            {
                Page = paginatedList.PageNumber,
                PageSize = pageSize,
                TotalPages = paginatedList.TotalPages,
                TotalCount = paginatedList.TotalCount,
                HasPreviousPage = paginatedList.HasPreviousPage,
                HasNextPage = paginatedList.HasNextPage
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID to mark as read</param>
    /// <returns>Success result</returns>
    [HttpPatch("{notificationId:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var command = new MarkNotificationAsReadCommand(notificationId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Create a new notification (admin only)
    /// </summary>
    /// <param name="request">Notification creation request</param>
    /// <returns>Created notification ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationCommand request)
    {
        var result = await Mediator.Send(request);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var response = new ApiResponse<Guid>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(CreateNotification), new { id = result.Value }, response);
    }
}
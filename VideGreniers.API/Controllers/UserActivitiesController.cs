using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.Models;
using VideGreniers.Application.UserActivities.DTOs;
using VideGreniers.Application.UserActivities.Queries.GetActivityStatistics;
using VideGreniers.Application.UserActivities.Queries.GetUserActivities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.API.Controllers;

/// <summary>
/// User activities and analytics endpoints
/// </summary>
[Authorize]
[Route("api/user-activities")]
[Tags("User Activities")]
public class UserActivitiesController : ApiController
{
    /// <summary>
    /// Get paginated list of user activities
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="activityType">Filter by activity type</param>
    /// <param name="startDate">Filter activities after this date</param>
    /// <param name="endDate">Filter activities before this date</param>
    /// <returns>Paginated list of user activities</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<UserActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserActivities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] UserActivityType? activityType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var query = new GetUserActivitiesQuery
        {
            UserId = userGuid,
            Page = page,
            PageSize = Math.Min(pageSize, 100), // Cap at 100
            ActivityType = activityType,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var paginatedList = result.Value;
        var response = new PaginatedApiResponse<UserActivityDto>
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
    /// Get user activity statistics and analytics
    /// </summary>
    /// <param name="startDate">Statistics period start date (default: 30 days ago)</param>
    /// <param name="endDate">Statistics period end date (default: now)</param>
    /// <returns>User activity statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<UserActivityStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActivityStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var query = new GetActivityStatisticsQuery(userGuid, startDate, endDate);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private string GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? 
               User.FindFirst("id")?.Value ?? 
               User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? 
               string.Empty;
    }
}
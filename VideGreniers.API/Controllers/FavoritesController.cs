using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Users.Commands.AddToFavorites;
using VideGreniers.Application.Users.Commands.RemoveFromFavorites;
using VideGreniers.Application.Users.Commands.ToggleFavorite;
using VideGreniers.Application.Users.Queries.GetFavoriteStatus;
using VideGreniers.Application.Users.Queries.GetUpcomingFavoriteEvents;
using VideGreniers.Application.Users.Queries.GetUserFavorites;

namespace VideGreniers.API.Controllers;

/// <summary>
/// Favorites management API endpoints
/// </summary>
[Authorize]
[Route("api/favorites")]
[Tags("Favorites")]
public class FavoritesController : ApiController
{
    /// <summary>
    /// Get current user's favorite events
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of favorite events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<FavoriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetUserFavoritesQuery
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Cap at 100
        };

        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var paginatedList = result.Value;
        var response = new PaginatedApiResponse<FavoriteDto>
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
    /// Add an event to user's favorites
    /// </summary>
    /// <param name="eventId">Event ID to add to favorites</param>
    /// <returns>Success result</returns>
    [HttpPost("{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddToFavorites(Guid eventId)
    {
        var command = new AddToFavoritesCommand(eventId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Remove an event from user's favorites
    /// </summary>
    /// <param name="eventId">Event ID to remove from favorites</param>
    /// <returns>Success result</returns>
    [HttpDelete("{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromFavorites(Guid eventId)
    {
        var command = new RemoveFromFavoritesCommand(eventId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Toggle favorite status for an event
    /// </summary>
    /// <param name="eventId">Event ID to toggle</param>
    /// <returns>Toggle result with new status</returns>
    [HttpPatch("{eventId:guid}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<ToggleFavoriteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFavorite(Guid eventId)
    {
        var command = new ToggleFavoriteCommand(eventId);
        var result = await Mediator.Send(command);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var response = new ApiResponse<ToggleFavoriteResult>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Check favorite status for multiple events
    /// </summary>
    /// <param name="eventIds">Event IDs to check</param>
    /// <returns>Dictionary of event ID to favorite status</returns>
    [HttpPost("status")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<Guid, bool>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavoriteStatus([FromBody] List<Guid> eventIds)
    {
        var query = new GetFavoriteStatusQuery(eventIds);
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var response = new ApiResponse<Dictionary<Guid, bool>>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Get user's favorite events happening in the upcoming days
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead (default: 7)</param>
    /// <returns>List of upcoming favorite events</returns>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUpcomingFavorites([FromQuery] int daysAhead = 7)
    {
        var query = new GetUpcomingFavoriteEventsQuery(daysAhead);
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var response = new ApiResponse<List<EventDto>>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}
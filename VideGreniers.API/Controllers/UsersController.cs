using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Users.Commands.AddToFavorites;
using VideGreniers.Application.Users.Commands.RemoveFromFavorites;
using VideGreniers.Application.Users.Queries.GetUserFavorites;
using VideGreniers.Application.Users.Queries.GetUserProfile;

namespace VideGreniers.API.Controllers;

/// <summary>
/// User management API endpoints for favorites and profile
/// </summary>
[Authorize]
[Tags("Users")]
public class UsersController : ApiController
{
    /// <summary>
    /// Get current user's profile information
    /// </summary>
    /// <returns>User profile details</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetUserProfileQuery();
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Get current user's favorite events
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of favorite events</returns>
    [HttpGet("favorites")]
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
    [HttpPost("favorites/{eventId:guid}")]
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
    [HttpDelete("favorites/{eventId:guid}")]
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

}
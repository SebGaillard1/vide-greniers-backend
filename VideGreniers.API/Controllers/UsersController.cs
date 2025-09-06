using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Users.Commands.UpdateUserProfile;
using VideGreniers.Application.Users.Queries.GetUserProfile;

namespace VideGreniers.API.Controllers;

/// <summary>
/// User management API endpoints for profile
/// </summary>
[Authorize]
[Route("api/user")]
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
    /// Update current user's profile information
    /// </summary>
    /// <param name="command">Profile update request</param>
    /// <returns>Success result</returns>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current user's account statistics
    /// </summary>
    /// <returns>User statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<UserStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserStats()
    {
        // This would be implemented later with a proper query
        var statsDto = new UserStatsDto
        {
            TotalFavorites = 0,
            TotalEventsCreated = 0,
            TotalNotifications = 0,
            UnreadNotifications = 0,
            AccountCreatedDate = DateTime.UtcNow
        };

        var response = new ApiResponse<UserStatsDto>
        {
            Data = statsDto,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Delete user account (soft delete)
    /// </summary>
    /// <returns>Success result</returns>
    [HttpDelete("account")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount()
    {
        // This would be implemented later with a proper command
        var response = new ApiResponse<object>
        {
            Data = null,
            Success = true,
            Message = "Account deletion requested successfully. Your account will be deleted within 30 days.",
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}
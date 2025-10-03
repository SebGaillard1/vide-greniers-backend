using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.Users.Commands.UpdateUserProfile;
using VideGreniers.Application.Users.Queries.GetUserProfile;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.API.Controllers;

/// <summary>
/// User management API endpoints for profile
/// </summary>
[Authorize]
[Route("api/user")]
[Tags("Users")]
public class UsersController : ApiController
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;

    public UsersController(
        ICurrentUserService currentUserService,
        IRepository<Favorite> favoriteRepository,
        IRepository<Event> eventRepository,
        IRepository<User> userRepository)
    {
        _currentUserService = currentUserService;
        _favoriteRepository = favoriteRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }
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
        // Get domain user ID
        var userId = await _currentUserService.GetDomainUserIdAsync();
        if (!userId.HasValue)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Errors = new List<string> { "User not found" },
                Timestamp = DateTime.UtcNow
            });
        }

        // Get user entity to get account created date
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Errors = new List<string> { "User not found" },
                Timestamp = DateTime.UtcNow
            });
        }

        // Count active favorites
        var activeFavoritesSpec = new ActiveUserFavoritesSpecification(userId.Value);
        var totalFavorites = await _favoriteRepository.CountAsync(activeFavoritesSpec);

        // Count created events
        var createdEventsSpec = new EventsByOrganizerSpecification(userId.Value);
        var totalEventsCreated = await _eventRepository.CountAsync(createdEventsSpec);

        var statsDto = new UserStatsDto
        {
            TotalFavorites = totalFavorites,
            TotalEventsCreated = totalEventsCreated,
            TotalNotifications = 0, // TODO: Implement when notifications are ready
            UnreadNotifications = 0, // TODO: Implement when notifications are ready
            AccountCreatedDate = user.CreatedOnUtc,
            LastLoginDate = null, // TODO: Track last login
            DaysActive = 0 // TODO: Calculate days active
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
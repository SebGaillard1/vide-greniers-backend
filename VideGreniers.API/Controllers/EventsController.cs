using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VideGreniers.API.Common;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.Common.Models;
using VideGreniers.Application.Events.Commands.CancelEvent;
using VideGreniers.Application.Events.Commands.CreateEvent;
using VideGreniers.Application.Events.Commands.PublishEvent;
using VideGreniers.Application.Events.Commands.UpdateEvent;
using VideGreniers.Application.Events.Queries.GetEventById;
using VideGreniers.Application.Events.Queries.GetNearbyEvents;
using VideGreniers.Application.Events.Queries.SearchEvents;
using VideGreniers.Domain.Enums;

namespace VideGreniers.API.Controllers;

/// <summary>
/// Events API endpoints for browsing, creating, and managing events
/// </summary>
[Tags("Events")]
public class EventsController : ApiController
{
    private readonly IUserActivityService _userActivityService;

    public EventsController(IUserActivityService userActivityService)
    {
        _userActivityService = userActivityService;
    }
    /// <summary>
    /// Get paginated list of events with optional filters
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term for title/description</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="eventType">Filter by event type</param>
    /// <param name="city">Filter by city</param>
    /// <param name="startDate">Filter events starting after this date</param>
    /// <param name="endDate">Filter events starting before this date</param>
    /// <param name="hasEntryFee">Filter by whether event has entry fee</param>
    /// <param name="sortBy">Sort field (StartDate, Title, CreatedOnUtc, PublishedOnUtc)</param>
    /// <param name="sortDescending">Sort in descending order</param>
    /// <returns>Paginated list of events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Domain.Enums.EventType? eventType = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] bool? hasEntryFee = null,
        [FromQuery] string sortBy = "StartDate",
        [FromQuery] bool sortDescending = false)
    {
        var query = new SearchEventsQuery
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100), // Cap at 100
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            EventType = eventType,
            City = city,
            StartDate = startDate,
            EndDate = endDate,
            HasEntryFee = hasEntryFee,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        var paginatedList = result.Value;
        var response = new PaginatedApiResponse<EventDto>
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

        // Add cache headers
        Response.Headers.CacheControl = "public, max-age=300"; // 5 minutes
        Response.Headers.TryAdd("X-Total-Count", paginatedList.TotalCount.ToString());

        return Ok(response);
    }

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Event details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        var query = new GetEventByIdQuery(id);
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        // Add cache headers
        Response.Headers.CacheControl = "public, max-age=600"; // 10 minutes

        // Track event view activity for authenticated users
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();
            
            _userActivityService.TrackActivityAsync(
                userGuid, 
                UserActivityType.EventViewed, 
                id, 
                null, 
                null, 
                ipAddress, 
                userAgent);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get events near a specific location
    /// </summary>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="radiusKm">Search radius in kilometers (default: 10, max: 100)</param>
    /// <param name="limit">Maximum number of results (default: 20, max: 50)</param>
    /// <returns>List of nearby events with distances</returns>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<List<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyEvents(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int radiusKm = 10,
        [FromQuery] int limit = 20)
    {
        var query = new GetNearbyEventsQuery
        {
            Latitude = latitude,
            Longitude = longitude,
            RadiusKm = Math.Min(radiusKm, 100), // Cap at 100km
            Limit = Math.Min(limit, 50) // Cap at 50 results
        };

        var result = await Mediator.Send(query);

        // Add cache headers
        Response.Headers.CacheControl = "public, max-age=300"; // 5 minutes

        return HandleResult(result);
    }

    /// <summary>
    /// Create a new event (requires authentication)
    /// </summary>
    /// <param name="command">Event creation data</param>
    /// <returns>Created event ID</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
    {
        var result = await Mediator.Send(command);

        return HandleCreatedResult(result, nameof(GetEventById), new { id = result.Value });
    }

    /// <summary>
    /// Update an existing event (requires authentication and ownership)
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="command">Event update data</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Errors = new List<string> { "Route ID does not match request body ID" },
                Timestamp = DateTime.UtcNow
            });
        }

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Publish an event (requires authentication and ownership)
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        var command = new PublishEventCommand(id);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Cancel an event (requires authentication and ownership)
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="request">Cancellation details</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelEvent(Guid id, [FromBody] CancelEventRequest request)
    {
        var command = new CancelEventCommand(id, request.Reason);
        var result = await Mediator.Send(command);

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

/// <summary>
/// Request model for canceling an event
/// </summary>
public record CancelEventRequest(string Reason);
using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Users.Commands.ToggleFavorite;

/// <summary>
/// Command to toggle an event's favorite status for the current user
/// </summary>
/// <param name="EventId">ID of the event to toggle</param>
public record ToggleFavoriteCommand(Guid EventId) : IRequest<ErrorOr<ToggleFavoriteResult>>;

/// <summary>
/// Result of toggle favorite operation
/// </summary>
/// <param name="EventId">ID of the event</param>
/// <param name="IsFavorite">New favorite status</param>
/// <param name="Action">Action that was performed</param>
public record ToggleFavoriteResult(
    Guid EventId,
    bool IsFavorite,
    string Action);
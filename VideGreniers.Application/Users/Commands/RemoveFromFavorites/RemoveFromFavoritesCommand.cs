using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Users.Commands.RemoveFromFavorites;

/// <summary>
/// Command to remove an event from user's favorites
/// </summary>
public record RemoveFromFavoritesCommand(Guid EventId) : IRequest<ErrorOr<Success>>;
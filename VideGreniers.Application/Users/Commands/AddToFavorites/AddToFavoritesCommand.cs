using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Users.Commands.AddToFavorites;

/// <summary>
/// Command to add an event to user's favorites
/// </summary>
public record AddToFavoritesCommand(Guid EventId) : IRequest<ErrorOr<Success>>;
using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Users.Queries.GetFavoriteStatus;

/// <summary>
/// Query to check favorite status for multiple events
/// </summary>
/// <param name="EventIds">List of event IDs to check</param>
public record GetFavoriteStatusQuery(IList<Guid> EventIds) : IRequest<ErrorOr<Dictionary<Guid, bool>>>;
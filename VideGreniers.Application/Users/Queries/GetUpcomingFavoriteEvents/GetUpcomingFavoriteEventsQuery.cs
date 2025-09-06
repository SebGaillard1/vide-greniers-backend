using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;

namespace VideGreniers.Application.Users.Queries.GetUpcomingFavoriteEvents;

/// <summary>
/// Query to get user's favorite events happening in the next specified days
/// </summary>
/// <param name="DaysAhead">Number of days to look ahead (default: 7)</param>
public record GetUpcomingFavoriteEventsQuery(int DaysAhead = 7) : IRequest<ErrorOr<List<EventDto>>>;
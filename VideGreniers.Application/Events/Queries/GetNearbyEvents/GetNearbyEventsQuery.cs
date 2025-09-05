using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Events.Queries.GetNearbyEvents;

/// <summary>
/// Query to get events near a specific location
/// </summary>
public record GetNearbyEventsQuery : IRequest<ErrorOr<List<EventDto>>>, ICacheableQuery
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int RadiusKm { get; init; } = 10;
    public int Limit { get; init; } = 20;

    public string CacheKey => CacheKeys.NearbyEvents(Latitude, Longitude, RadiusKm, Limit);
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(5);
}
using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Application.Events.Queries.GetNearbyEvents;

/// <summary>
/// Handler for getting nearby events
/// </summary>
public class GetNearbyEventsQueryHandler : IRequestHandler<GetNearbyEventsQuery, ErrorOr<List<EventDto>>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IMapper _mapper;

    public GetNearbyEventsQueryHandler(
        IRepository<Event> eventRepository,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<List<EventDto>>> Handle(GetNearbyEventsQuery request, CancellationToken cancellationToken)
    {
        // Create location for distance calculations
        var userLocationResult = Location.Create(request.Latitude, request.Longitude);
        if (userLocationResult.IsError)
        {
            return userLocationResult.Errors;
        }

        var userLocation = userLocationResult.Value;

        // Use specification pattern for nearby events
        var specification = new EventsByLocationRadiusSpecification(userLocation, request.RadiusKm, onlyVisible: true);
        var events = await _eventRepository.GetAsync(specification, cancellationToken);

        // Calculate distances and filter by radius, then take the limit
        var nearbyEvents = events
            .Select(e => new
            {
                Event = e,
                Distance = CalculateDistance(userLocation, e.Location)
            })
            .Where(x => x.Distance <= request.RadiusKm)
            .OrderBy(x => x.Distance)
            .Take(request.Limit)
            .Select(x =>
            {
                var dto = x.Event.ToDto();
                return dto with { DistanceKm = Math.Round(x.Distance, 2) };
            })
            .ToList();

        return nearbyEvents;
    }

    /// <summary>
    /// Calculate distance between two locations using Haversine formula
    /// </summary>
    private static double CalculateDistance(Location location1, Location location2)
    {
        const double earthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(location1.Latitude);
        var lat2Rad = DegreesToRadians(location2.Latitude);
        var deltaLatRad = DegreesToRadians(location2.Latitude - location1.Latitude);
        var deltaLngRad = DegreesToRadians(location2.Longitude - location1.Longitude);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLngRad / 2) * Math.Sin(deltaLngRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }
}
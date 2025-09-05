using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Services;

public class LocationService : ILocationService
{
    // France approximate bounds for validation
    private const double FranceMinLatitude = 41.0;
    private const double FranceMaxLatitude = 51.5;
    private const double FranceMinLongitude = -5.5;
    private const double FranceMaxLongitude = 10.0;

    public double CalculateDistance(Location from, Location to)
    {
        return from.DistanceTo(to);
    }

    public bool IsWithinRadius(Location center, Location target, double radiusKm)
    {
        return center.IsWithinRadius(target, radiusKm);
    }

    public async Task<IEnumerable<Event>> GetEventsWithinRadius(
        Location userLocation, 
        double radiusKm, 
        IEnumerable<Event> events)
    {
        return await Task.FromResult(events
            .Where(e => e.IsVisibleToPublic() && e.IsWithinRadius(userLocation, radiusKm))
            .OrderBy(e => e.DistanceFrom(userLocation))
            .ThenBy(e => e.DateRange.StartDate));
    }

    public async Task<IEnumerable<EventWithDistance>> GetEventsWithDistances(
        Location userLocation, 
        double radiusKm, 
        IEnumerable<Event> events)
    {
        return await Task.FromResult(events
            .Where(e => e.IsVisibleToPublic())
            .Select(e => new { Event = e, Distance = e.DistanceFrom(userLocation) })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Event.DateRange.StartDate)
            .Select(x => new EventWithDistance(x.Event, Math.Round(x.Distance, 2))));
    }

    public async Task<IEnumerable<string>> GetNearbyCities(Location location, double radiusKm)
    {
        // This would typically query a cities database or external geocoding service
        // For now, return a placeholder implementation
        await Task.CompletedTask;
        
        // In a real implementation, this would:
        // 1. Query a database of cities/towns
        // 2. Use a geocoding service
        // 3. Cache results for performance
        
        return new List<string>();
    }

    public bool IsLocationValid(Location location)
    {
        // Check if location is within reasonable bounds (e.g., France + neighboring countries)
        return location.Latitude >= FranceMinLatitude && 
               location.Latitude <= FranceMaxLatitude &&
               location.Longitude >= FranceMinLongitude && 
               location.Longitude <= FranceMaxLongitude;
    }

    public Location GetCenterPoint(IEnumerable<Location> locations)
    {
        var locationsList = locations.ToList();
        
        if (!locationsList.Any())
        {
            throw new ArgumentException("Cannot calculate center point for empty location list", nameof(locations));
        }

        if (locationsList.Count == 1)
        {
            return locationsList.First();
        }

        var avgLatitude = locationsList.Average(l => l.Latitude);
        var avgLongitude = locationsList.Average(l => l.Longitude);

        var centerLocation = Location.Create(avgLatitude, avgLongitude);
        
        if (centerLocation.IsError)
        {
            throw new InvalidOperationException("Failed to create center point location");
        }

        return centerLocation.Value;
    }

    public int CalculateMapZoomLevel(IEnumerable<Location> locations)
    {
        var locationsList = locations.ToList();
        
        if (!locationsList.Any())
        {
            return 10; // Default zoom level
        }

        if (locationsList.Count == 1)
        {
            return 15; // Close zoom for single location
        }

        var minLat = locationsList.Min(l => l.Latitude);
        var maxLat = locationsList.Max(l => l.Latitude);
        var minLng = locationsList.Min(l => l.Longitude);
        var maxLng = locationsList.Max(l => l.Longitude);

        var latDiff = maxLat - minLat;
        var lngDiff = maxLng - minLng;
        var maxDiff = Math.Max(latDiff, lngDiff);

        // Calculate zoom level based on coordinate span
        // This is a simplified calculation - real implementations would be more sophisticated
        return maxDiff switch
        {
            < 0.01 => 15,   // Very close - neighborhood level
            < 0.05 => 13,   // City level  
            < 0.1 => 11,    // Metropolitan area
            < 0.5 => 9,     // Regional
            < 1.0 => 7,     // Large region
            < 2.0 => 6,     // Small country
            _ => 5          // Country or larger
        };
    }
}
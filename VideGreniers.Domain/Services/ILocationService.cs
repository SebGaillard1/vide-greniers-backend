using VideGreniers.Domain.Entities;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Services;

public interface ILocationService
{
    /// <summary>
    /// Calculate distance between two locations in kilometers
    /// </summary>
    double CalculateDistance(Location from, Location to);

    /// <summary>
    /// Check if location is within specified radius of another location
    /// </summary>
    bool IsWithinRadius(Location center, Location target, double radiusKm);

    /// <summary>
    /// Get events within specified radius of location, sorted by distance
    /// </summary>
    Task<IEnumerable<Event>> GetEventsWithinRadius(
        Location userLocation, 
        double radiusKm, 
        IEnumerable<Event> events);

    /// <summary>
    /// Get events within radius with distance information
    /// </summary>
    Task<IEnumerable<EventWithDistance>> GetEventsWithDistances(
        Location userLocation, 
        double radiusKm, 
        IEnumerable<Event> events);

    /// <summary>
    /// Find nearby cities/areas for location-based suggestions
    /// </summary>
    Task<IEnumerable<string>> GetNearbyCities(Location location, double radiusKm);

    /// <summary>
    /// Validate if coordinates are within reasonable bounds for the application
    /// (e.g., within France, Europe, etc.)
    /// </summary>
    bool IsLocationValid(Location location);

    /// <summary>
    /// Get center point of multiple locations (for map display)
    /// </summary>
    Location GetCenterPoint(IEnumerable<Location> locations);

    /// <summary>
    /// Calculate appropriate zoom level for map based on locations spread
    /// </summary>
    int CalculateMapZoomLevel(IEnumerable<Location> locations);
}

public record EventWithDistance(Event Event, double DistanceKm);
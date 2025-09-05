using Microsoft.EntityFrameworkCore;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Interfaces;

namespace VideGreniers.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Favorites)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetPublishedEventsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published)
            .OrderByDescending(e => e.PublishedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetNearbyEventsAsync(
        double latitude, 
        double longitude, 
        double radiusKm, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        // Calculate bounding box for initial filtering (performance optimization)
        var latDelta = radiusKm / 111.0; // Roughly 111 km per degree of latitude
        var lonDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var minLat = latitude - latDelta;
        var maxLat = latitude + latDelta;
        var minLon = longitude - lonDelta;
        var maxLon = longitude + lonDelta;

        // Get events within bounding box first
        var events = await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published)
            .Where(e => e.Location.Latitude >= minLat && e.Location.Latitude <= maxLat)
            .Where(e => e.Location.Longitude >= minLon && e.Location.Longitude <= maxLon)
            .Where(e => e.DateRange.EndDate >= DateTimeOffset.UtcNow) // Only future/current events
            .ToListAsync(cancellationToken);

        // Calculate exact distances and filter by radius
        var nearbyEvents = events
            .Where(e => CalculateDistance(latitude, longitude, e.Location.Latitude, e.Location.Longitude) <= radiusKm)
            .OrderBy(e => CalculateDistance(latitude, longitude, e.Location.Latitude, e.Location.Longitude))
            .Take(limit)
            .ToList();

        return nearbyEvents.AsReadOnly();
    }

    public async Task<IReadOnlyList<Event>> GetUserEventsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == userId)
            .OrderByDescending(e => e.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published)
            .Where(e => e.DateRange.StartDate >= DateTimeOffset.UtcNow)
            .OrderBy(e => e.DateRange.StartDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

    public void Add(Event @event)
    {
        _context.Events.Add(@event);
    }

    public void Update(Event @event)
    {
        _context.Events.Update(@event);
    }

    public void Delete(Event @event)
    {
        _context.Events.Remove(@event);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var latRad1 = lat1 * Math.PI / 180;
        var latRad2 = lat2 * Math.PI / 180;
        var deltaLat = (lat2 - lat1) * Math.PI / 180;
        var deltaLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(latRad1) * Math.Cos(latRad2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
}
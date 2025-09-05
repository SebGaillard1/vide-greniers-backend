using VideGreniers.Domain.Entities;

namespace VideGreniers.Domain.Interfaces;

/// <summary>
/// Repository interface for Event entity
/// </summary>
public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetPublishedEventsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetNearbyEventsAsync(double latitude, double longitude, double radiusKm, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUserEventsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Event @event);
    void Update(Event @event);
    void Delete(Event @event);
}
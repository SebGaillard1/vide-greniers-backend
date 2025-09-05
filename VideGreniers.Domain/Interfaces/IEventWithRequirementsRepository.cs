using VideGreniers.Domain.Entities;

namespace VideGreniers.Domain.Interfaces;

/// <summary>
/// Repository interface for EventWithRequirements entity
/// </summary>
public interface IEventWithRequirementsRepository
{
    Task<EventWithRequirements?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventWithRequirements>> GetPublishedEventsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventWithRequirements>> GetNearbyEventsAsync(double latitude, double longitude, double radiusKm, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventWithRequirements>> GetUserEventsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventWithRequirements>> GetUpcomingEventsAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(EventWithRequirements @event);
    void Update(EventWithRequirements @event);
    void Delete(EventWithRequirements @event);
}
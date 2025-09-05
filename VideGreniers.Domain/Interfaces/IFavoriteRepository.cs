using VideGreniers.Domain.Entities;

namespace VideGreniers.Domain.Interfaces;

/// <summary>
/// Repository interface for Favorite entity
/// </summary>
public interface IFavoriteRepository
{
    Task<Favorite?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Favorite>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    void Add(Favorite favorite);
    void Delete(Favorite favorite);
}
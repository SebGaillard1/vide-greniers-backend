using VideGreniers.Domain.Entities;

namespace VideGreniers.Domain.Interfaces;

/// <summary>
/// Repository interface for FavoriteWithRequirements entity
/// </summary>
public interface IFavoriteWithRequirementsRepository
{
    Task<FavoriteWithRequirements?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FavoriteWithRequirements>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    void Add(FavoriteWithRequirements favorite);
    void Delete(FavoriteWithRequirements favorite);
}
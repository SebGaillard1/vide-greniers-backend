using Microsoft.EntityFrameworkCore;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Interfaces;

namespace VideGreniers.Infrastructure.Persistence.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _context;

    public FavoriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Favorite?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Favorites
            .Include(f => f.Event)
                .ThenInclude(e => e.Organizer)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId, cancellationToken);
    }

    public async Task<IReadOnlyList<Favorite>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Favorites
            .AsNoTracking()
            .Include(f => f.Event)
                .ThenInclude(e => e.Organizer)
            .Where(f => f.UserId == userId)
            .Where(f => f.Status == FavoriteStatus.Active)
            .OrderByDescending(f => f.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.EventId == eventId && f.Status == FavoriteStatus.Active, cancellationToken);
    }

    public void Add(Favorite favorite)
    {
        _context.Favorites.Add(favorite);
    }

    public void Delete(Favorite favorite)
    {
        _context.Favorites.Remove(favorite);
    }
}
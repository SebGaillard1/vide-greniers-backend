using Microsoft.EntityFrameworkCore;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Application database context interface for dependency injection and testing
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> DomainUsers { get; }
    DbSet<Event> Events { get; }
    DbSet<Favorite> Favorites { get; }
    DbSet<Category> Categories { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<UserActivity> UserActivities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
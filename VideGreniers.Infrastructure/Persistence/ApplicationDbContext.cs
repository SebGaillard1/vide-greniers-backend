using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Entities;
using VideGreniers.Infrastructure.Identity;

namespace VideGreniers.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IDateTimeProvider? _dateTimeProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    // Domain entities
    public DbSet<User> DomainUsers { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global query filters for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(entityType.ClrType);
                method?.Invoke(null, new object[] { builder });
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        UpdateAuditFields();
        
        // Dispatch domain events
        await DispatchDomainEventsAsync();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.GetType().GetProperty("CreatedAt")?.SetValue(entry.Entity, now);
                    entry.Entity.GetType().GetProperty("UpdatedAt")?.SetValue(entry.Entity, now);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType().GetProperty("UpdatedAt")?.SetValue(entry.Entity, now);
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.GetType().GetProperty("CreatedOnUtc")?.SetValue(entry.Entity, now);
                    entry.Entity.GetType().GetProperty("ModifiedOnUtc")?.SetValue(entry.Entity, now);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType().GetProperty("ModifiedOnUtc")?.SetValue(entry.Entity, now);
                    break;
            }
        }
    }

    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // Here we would publish domain events using MediatR or similar
        // For now, we'll just clear them
        await Task.CompletedTask;
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) 
        where TEntity : BaseAuditableEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
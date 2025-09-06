using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Entities;
using VideGreniers.Infrastructure.Identity;

namespace VideGreniers.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
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
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<UserActivity> UserActivities { get; set; } = null!;

    // Identity-related entities
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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
        // Temporarily disabled to test registration functionality
        // The domain entities should handle their own audit fields
        // This will be re-implemented properly later
        return;
        
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // For BaseEntity, we need to use reflection to set the backing field since properties have private setters
                    SetPrivatePropertyValue(entry.Entity, "CreatedOnUtc", now);
                    break;

                case EntityState.Modified:
                    // Use the MarkAsModified method which sets ModifiedOnUtc
                    var markAsModifiedMethod = entry.Entity.GetType().GetMethod("MarkAsModified", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    markAsModifiedMethod?.Invoke(entry.Entity, null);
                    break;
            }
        }
    }

    private void SetPrivatePropertyValue(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property?.SetMethod != null)
        {
            // Try to access the setter even if it's private
            var setter = property.GetSetMethod(nonPublic: true);
            setter?.Invoke(obj, new[] { value });
        }
        else
        {
            // If setter doesn't exist or is not accessible, try to find the backing field
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
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
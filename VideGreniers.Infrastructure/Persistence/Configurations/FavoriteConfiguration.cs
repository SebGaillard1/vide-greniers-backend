using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever(); // We set Guid in the entity constructor

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.Property(f => f.EventId)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<FavoriteStatus>(value))
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(FavoriteStatus.Active);

        builder.Property(f => f.ArchivedOnUtc)
            .IsRequired(false);

        builder.Property(f => f.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        // BaseEntity audit fields
        builder.Property(f => f.CreatedOnUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(f => f.ModifiedOnUtc)
            .IsRequired(false);

        // BaseAuditableEntity audit fields
        builder.Property(f => f.CreatedByUserId)
            .IsRequired(false);

        builder.Property(f => f.ModifiedByUserId)
            .IsRequired(false);

        // Soft delete from BaseAuditableEntity
        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(f => f.DeletedByUserId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete favorites when user is deleted

        builder.HasOne(f => f.Event)
            .WithMany(e => e.Favorites)
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade); // Delete favorites when event is deleted

        // Composite unique constraint
        builder.HasIndex(f => new { f.UserId, f.EventId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false") // Only apply unique constraint to non-deleted records
            .HasDatabaseName("IX_Favorites_UserId_EventId_Unique");

        // Indexes for performance
        builder.HasIndex(f => f.UserId)
            .HasDatabaseName("IX_Favorites_UserId");

        builder.HasIndex(f => f.EventId)
            .HasDatabaseName("IX_Favorites_EventId");

        builder.HasIndex(f => f.Status)
            .HasDatabaseName("IX_Favorites_Status");

        builder.HasIndex(f => f.CreatedOnUtc)
            .HasDatabaseName("IX_Favorites_CreatedOnUtc");

        builder.HasIndex(f => f.IsDeleted)
            .HasDatabaseName("IX_Favorites_IsDeleted");
    }
}
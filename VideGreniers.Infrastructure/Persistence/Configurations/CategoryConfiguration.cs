using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever(); // We set Guid in the entity constructor

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.ColorHex)
            .HasMaxLength(7) // For hex colors like #FF0000
            .IsRequired();

        builder.Property(c => c.IconName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Type)
            .HasConversion(
                type => type.ToString(),
                value => Enum.Parse<CategoryType>(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit fields from BaseEntity
        builder.Property(c => c.CreatedOnUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.ModifiedOnUtc)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Categories_Name_Unique");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");

        builder.HasIndex(c => c.SortOrder)
            .HasDatabaseName("IX_Categories_SortOrder");
    }
}
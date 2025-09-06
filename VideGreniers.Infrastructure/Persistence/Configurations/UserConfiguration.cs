using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever(); // We set Guid in the entity constructor

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        // Configure Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
                
            // Index on email for performance and uniqueness
            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        // Configure PhoneNumber value object
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20)
                .IsRequired(false);
                
            // Add a discriminator to identify when phone exists
            phone.Property<bool>("HasPhoneNumber")
                .HasDefaultValue(false);
        });

        builder.Property(u => u.Role)
            .HasConversion(
                role => role.ToString(),
                value => Enum.Parse<UserRole>(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.LastLoginUtc)
            .IsRequired(false);

        builder.Property(u => u.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields from BaseEntity
        builder.Property(u => u.CreatedOnUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.ModifiedOnUtc)
            .IsRequired(false);

        // BaseAuditableEntity audit fields
        builder.Property(u => u.CreatedByUserId)
            .IsRequired(false);

        builder.Property(u => u.ModifiedByUserId)
            .IsRequired(false);

        // Soft delete from BaseAuditableEntity
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(u => u.DeletedByUserId)
            .IsRequired(false);

        // Note: Relationships are configured in the dependent entity configurations
        // Event -> User relationship configured in EventConfiguration
        // Favorite -> User relationship configured in FavoriteConfiguration

        // Indexes
        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");
    }
}
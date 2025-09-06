using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public sealed class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.EventId)
            .IsRequired(false);

        builder.Property(x => x.SearchTerm)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Metadata)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45) // IPv6 max length
            .IsRequired(false);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Event)
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserActivities_UserId");

        builder.HasIndex(x => x.ActivityType)
            .HasDatabaseName("IX_UserActivities_ActivityType");

        builder.HasIndex(x => x.CreatedOnUtc)
            .HasDatabaseName("IX_UserActivities_CreatedOnUtc");

        builder.HasIndex(x => new { x.UserId, x.ActivityType })
            .HasDatabaseName("IX_UserActivities_UserId_ActivityType");

        builder.HasIndex(x => new { x.EventId, x.ActivityType })
            .HasDatabaseName("IX_UserActivities_EventId_ActivityType")
            .HasFilter("[EventId] IS NOT NULL");

        // Table name
        builder.ToTable("UserActivities");
    }
}
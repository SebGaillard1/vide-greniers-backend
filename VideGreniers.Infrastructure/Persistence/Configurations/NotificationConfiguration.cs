using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ReadOnUtc)
            .IsRequired(false);

        builder.Property(x => x.SentOnUtc)
            .IsRequired(false);

        builder.Property(x => x.ActionUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.ActionText)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        // Configure Dictionary<string, object> as JSON
        builder.Property(x => x.Metadata)
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null))
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.EventId)
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
            .HasDatabaseName("IX_Notifications_UserId");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_Notifications_Type");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Notifications_Status");

        builder.HasIndex(x => x.CreatedOnUtc)
            .HasDatabaseName("IX_Notifications_CreatedOnUtc");

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_Notifications_UserId_Status");

        // Table name
        builder.ToTable("Notifications");
    }
}
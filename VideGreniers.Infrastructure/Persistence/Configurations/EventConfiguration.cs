using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // We set Guid in the entity constructor

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasConversion(
                type => type.ToString(),
                value => Enum.Parse<EventType>(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<EventStatus>(value))
            .HasMaxLength(50)
            .IsRequired();

        // Configure DateRange as owned entity
        builder.OwnsOne(e => e.DateRange, dateRange =>
        {
            dateRange.Property(dr => dr.StartDate)
                .HasColumnName("DateRange_StartDate")
                .IsRequired();

            dateRange.Property(dr => dr.EndDate)
                .HasColumnName("DateRange_EndDate")
                .IsRequired();
                
            // Index on StartDate for temporal queries
            dateRange.HasIndex(dr => dr.StartDate)
                .HasDatabaseName("IX_Events_StartDate");
        });

        // Configure Location as owned entity
        builder.OwnsOne(e => e.Location, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("Location_Latitude")
                .HasPrecision(18, 6)
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("Location_Longitude")
                .HasPrecision(18, 6)
                .IsRequired();
                
            // Composite index on coordinates for geo queries
            location.HasIndex(l => new { l.Latitude, l.Longitude })
                .HasDatabaseName("IX_Events_Location");
        });

        // Configure Address as owned entity
        builder.OwnsOne(e => e.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("Address_PostalCode")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("Address_State")
                .HasMaxLength(100)
                .IsRequired(false);
        });

        // Configure ContactPhoneNumber value object
        builder.OwnsOne(e => e.ContactPhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("ContactPhoneNumber")
                .HasMaxLength(20);
        });

        // Configure ContactEmail value object
        builder.OwnsOne(e => e.ContactEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("ContactEmail")
                .HasMaxLength(256);
        });

        builder.Property(e => e.SpecialInstructions)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Configure Money value object for EntryFee
        builder.OwnsOne(e => e.EntryFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("EntryFee_Amount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("EntryFee_Currency")
                .HasMaxLength(3);
        });

        builder.Property(e => e.AllowsEarlyBird)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.EarlyBirdTime)
            .IsRequired(false);

        // Configure Money value object for EarlyBirdFee
        builder.OwnsOne(e => e.EarlyBirdFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("EarlyBirdFee_Amount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("EarlyBirdFee_Currency")
                .HasMaxLength(3);
        });

        builder.Property(e => e.PublishedOnUtc)
            .IsRequired(false);

        // Foreign keys
        builder.Property(e => e.OrganizerId)
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .IsRequired(false);

        // BaseEntity audit fields
        builder.Property(e => e.CreatedOnUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.ModifiedOnUtc)
            .IsRequired(false);

        // BaseAuditableEntity audit fields
        builder.Property(e => e.CreatedByUserId)
            .IsRequired(false);

        builder.Property(e => e.ModifiedByUserId)
            .IsRequired(false);

        // Soft delete from BaseAuditableEntity
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(e => e.DeletedByUserId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(e => e.Organizer)
            .WithMany(u => u.CreatedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Favorites)
            .WithOne(f => f.Event)
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade); // Delete favorites when event is deleted

        // Indexes for performance
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Events_Status");

        // Note: Composite Status+StartDate index will be created manually if needed

        builder.HasIndex(e => e.OrganizerId)
            .HasDatabaseName("IX_Events_OrganizerId");

        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("IX_Events_CategoryId");

        builder.HasIndex(e => e.PublishedOnUtc)
            .HasDatabaseName("IX_Events_PublishedOnUtc");

        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName("IX_Events_IsDeleted");
    }
}
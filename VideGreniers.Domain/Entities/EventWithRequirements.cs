using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Entities;

/// <summary>
/// Event (Vide-grenier) entity following the exact requirements from the prompt
/// </summary>
public sealed class EventWithRequirements : Entity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public LocationWithAddress Location { get; private set; }
    public DateRangeWithValueObject DateRange { get; private set; }
    public EventStatus Status { get; private set; }
    public Guid OrganizerId { get; private set; }
    public string ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public int ViewCount { get; private set; }
    
    // Navigation
    public UserWithAuth? Organizer { get; private set; }
    private readonly List<FavoriteWithRequirements> _favorites = new();
    public IReadOnlyList<FavoriteWithRequirements> Favorites => _favorites.AsReadOnly();
    
    private EventWithRequirements(
        string title,
        string description,
        LocationWithAddress location,
        DateRangeWithValueObject dateRange,
        Guid organizerId,
        string contactEmail,
        string? contactPhone)
    {
        Title = title;
        Description = description;
        Location = location;
        DateRange = dateRange;
        Status = EventStatus.Draft;
        OrganizerId = organizerId;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        ViewCount = 0;
    }
    
    public static ErrorOr<EventWithRequirements> Create(
        string title,
        string description,
        LocationWithAddress location,
        DateRangeWithValueObject dateRange,
        Guid organizerId,
        string contactEmail,
        string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 100)
            return Errors.Event.InvalidTitle;
        
        if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 2000)
            return Errors.Event.InvalidDescription;
        
        if (string.IsNullOrWhiteSpace(contactEmail))
            return Errors.Event.InvalidContactEmail;
        
        return new EventWithRequirements(title, description, location, dateRange, organizerId, contactEmail, contactPhone);
    }
    
    public ErrorOr<Success> Publish()
    {
        if (Status != EventStatus.Draft)
            return Errors.Event.CannotPublish;
        
        if (DateRange.HasPassed)
            return Errors.Event.EventDateHasPassed;
        
        Status = EventStatus.Published;
        MarkAsUpdated();
        
        AddDomainEvent(new EventPublishedDomainEvent(Id, OrganizerId, Title));
        
        return Result.Success;
    }
    
    public ErrorOr<Success> Cancel(string reason)
    {
        if (Status == EventStatus.Cancelled || Status == EventStatus.Completed)
            return Errors.Event.CannotCancel;
        
        Status = EventStatus.Cancelled;
        MarkAsUpdated();
        
        AddDomainEvent(new EventCancelledDomainEvent(Id, OrganizerId, reason));
        
        return Result.Success;
    }
    
    public void MarkAsCompleted()
    {
        if (DateRange.HasPassed && Status == EventStatus.Published)
        {
            Status = EventStatus.Completed;
            MarkAsUpdated();
        }
    }
    
    public void IncrementViewCount()
    {
        ViewCount++;
    }
    
    public ErrorOr<Success> Update(
        string title,
        string description,
        LocationWithAddress location,
        DateRangeWithValueObject dateRange,
        string contactEmail,
        string? contactPhone)
    {
        if (Status == EventStatus.Cancelled || Status == EventStatus.Completed)
            return Errors.Event.CannotUpdateClosedEvent;
        
        Title = title;
        Description = description;
        Location = location;
        DateRange = dateRange;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        MarkAsUpdated();
        
        return Result.Success;
    }
}
using System.Linq.Expressions;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Specifications;

public class EventsVisibleToPublicSpecification : BaseSpecification<Event>
{
    public EventsVisibleToPublicSpecification() : base(e => 
        e.Status == EventStatus.Published || 
        e.Status == EventStatus.Active || 
        e.Status == EventStatus.Completed)
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class EventsByLocationRadiusSpecification : BaseSpecification<Event>
{
    public EventsByLocationRadiusSpecification(
        Location userLocation, 
        double radiusKm,
        bool onlyVisible = true) : base(e => onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : true)
    {
        // Note: The actual distance calculation will need to be done at the database level
        // This is a simplified version for the specification pattern
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class EventsByDateRangeSpecification : BaseSpecification<Event>
{
    public EventsByDateRangeSpecification(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        bool onlyVisible = true) : base(e => 
        e.DateRange.StartDate >= startDate &&
        e.DateRange.StartDate <= endDate &&
        (onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : 
            true))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class UpcomingEventsSpecification : BaseSpecification<Event>
{
    public UpcomingEventsSpecification(int daysAhead = 30) : base(e => 
        e.DateRange.StartDate > DateTimeOffset.UtcNow &&
        e.DateRange.StartDate <= DateTimeOffset.UtcNow.AddDays(daysAhead) &&
        (e.Status == EventStatus.Published || e.Status == EventStatus.Active))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class EventsByOrganizerSpecification : BaseSpecification<Event>
{
    public EventsByOrganizerSpecification(Guid organizerId, bool includeDeleted = false) 
        : base(e => e.OrganizerId == organizerId && (includeDeleted || !e.IsDeleted))
    {
        AddInclude(e => e.Category);
        ApplyOrderByDescending(e => e.DateRange.StartDate);
    }
}

public class EventsByCategorySpecification : BaseSpecification<Event>
{
    public EventsByCategorySpecification(
        Guid categoryId, 
        bool onlyVisible = true) : base(e => 
        e.CategoryId == categoryId &&
        (onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : 
            true))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class EventsByTypeSpecification : BaseSpecification<Event>
{
    public EventsByTypeSpecification(
        EventType eventType, 
        bool onlyVisible = true) : base(e => 
        e.EventType == eventType &&
        (onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : 
            true))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class EventsByStatusSpecification : BaseSpecification<Event>
{
    public EventsByStatusSpecification(EventStatus status) : base(e => e.Status == status)
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderByDescending(e => e.CreatedOnUtc);
    }
}

public class EventsRequiringStatusUpdateSpecification : BaseSpecification<Event>
{
    public EventsRequiringStatusUpdateSpecification() : base(e => 
        (e.Status == EventStatus.Published && e.DateRange.StartDate <= DateTimeOffset.UtcNow) ||
        (e.Status == EventStatus.Active && e.DateRange.EndDate <= DateTimeOffset.UtcNow))
    {
        // No includes needed for background processing
    }
}

public class SearchEventsSpecification : BaseSpecification<Event>
{
    public SearchEventsSpecification(
        string searchTerm,
        bool onlyVisible = true) : base(e =>
        (e.Title.Contains(searchTerm) || 
         e.Description.Contains(searchTerm) ||
         e.Address.City.Contains(searchTerm) ||
         e.Address.PostalCode.Contains(searchTerm)) &&
        (onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : 
            true))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
    }
}

public class PaginatedEventsSpecification : BaseSpecification<Event>
{
    public PaginatedEventsSpecification(
        int pageNumber,
        int pageSize,
        bool onlyVisible = true) : base(e => 
        onlyVisible ? 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) : 
            true)
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

public class EventWithDetailsSpecification : BaseSpecification<Event>
{
    public EventWithDetailsSpecification(Guid eventId) : base(e => e.Id == eventId)
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        AddInclude(e => e.Favorites);
    }
}

public class ComplexEventFilterSpecification : BaseSpecification<Event>
{
    public ComplexEventFilterSpecification(
        Location? userLocation = null,
        double? radiusKm = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        EventType? eventType = null,
        Guid? categoryId = null,
        bool? hasEntryFee = null,
        string? searchTerm = null,
        int? pageNumber = null,
        int? pageSize = null) : base(BuildCriteria(
            userLocation, radiusKm, startDate, endDate, 
            eventType, categoryId, hasEntryFee, searchTerm))
    {
        AddInclude(e => e.Organizer);
        AddInclude(e => e.Category);
        ApplyOrderBy(e => e.DateRange.StartDate);
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            ApplyPaging((pageNumber.Value - 1) * pageSize.Value, pageSize.Value);
        }
    }

    private static Expression<Func<Event, bool>> BuildCriteria(
        Location? userLocation,
        double? radiusKm,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        EventType? eventType,
        Guid? categoryId,
        bool? hasEntryFee,
        string? searchTerm)
    {
        return e => 
            (e.Status == EventStatus.Published || e.Status == EventStatus.Active) &&
            (startDate == null || e.DateRange.StartDate >= startDate) &&
            (endDate == null || e.DateRange.StartDate <= endDate) &&
            (eventType == null || e.EventType == eventType) &&
            (categoryId == null || e.CategoryId == categoryId) &&
            (hasEntryFee == null || (hasEntryFee.Value ? e.EntryFee != null : e.EntryFee == null)) &&
            (string.IsNullOrWhiteSpace(searchTerm) || 
             e.Title.Contains(searchTerm) || 
             e.Description.Contains(searchTerm) ||
             e.Address.City.Contains(searchTerm));
        // Note: Location/radius filtering would typically be done with spatial queries at the database level
    }
}
using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Entities;

public sealed class Event : BaseAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public EventType EventType { get; private set; }
    public EventStatus Status { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public PhoneNumber? ContactPhoneNumber { get; private set; }
    public Email? ContactEmail { get; private set; }
    public string? SpecialInstructions { get; private set; }
    public Money? EntryFee { get; private set; }
    public bool AllowsEarlyBird { get; private set; }
    public TimeSpan? EarlyBirdTime { get; private set; }
    public Money? EarlyBirdFee { get; private set; }
    public DateTime? PublishedOnUtc { get; private set; }
    public int FavoriteCount { get; private set; }

    // Foreign keys
    public Guid OrganizerId { get; private set; }
    public Guid? CategoryId { get; private set; }

    // Navigation properties
    public User Organizer { get; private set; } = null!;
    public Category? Category { get; private set; }

    private readonly List<Favorite> _favorites = [];
    public IReadOnlyList<Favorite> Favorites => _favorites.AsReadOnly();

    // Private constructor for EF Core
    private Event() { }

    private Event(
        string title,
        string description,
        EventType eventType,
        DateRange dateRange,
        Location location,
        Address address,
        Guid organizerId,
        PhoneNumber? contactPhoneNumber = null,
        Email? contactEmail = null,
        string? specialInstructions = null,
        Money? entryFee = null,
        Guid? categoryId = null)
    {
        Title = title;
        Description = description;
        EventType = eventType;
        Status = EventStatus.Draft;
        DateRange = dateRange;
        Location = location;
        Address = address;
        OrganizerId = organizerId;
        ContactPhoneNumber = contactPhoneNumber;
        ContactEmail = contactEmail;
        SpecialInstructions = specialInstructions;
        EntryFee = entryFee;
        CategoryId = categoryId;
        AllowsEarlyBird = false;

        RaiseDomainEvent(new EventCreatedDomainEvent(Id, OrganizerId, Title));
    }

    public static ErrorOr<Event> Create(
        string title,
        string description,
        EventType eventType,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        double latitude,
        double longitude,
        string street,
        string city,
        string postalCode,
        string country,
        Guid organizerId,
        string? contactPhoneNumber = null,
        string? contactEmail = null,
        string? specialInstructions = null,
        decimal? entryFeeAmount = null,
        string? entryFeeCurrency = null,
        Guid? categoryId = null,
        string? state = null)
    {
        var errors = new List<Error>();

        // Validate title
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
        {
            errors.Add(Errors.Event.TitleTooShort);
        }

        if (title?.Length > 200)
        {
            errors.Add(Errors.Event.TitleTooLong);
        }

        // Validate description
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 2000)
        {
            errors.Add(Errors.Event.DescriptionTooLong);
        }

        // Validate special instructions
        if (!string.IsNullOrWhiteSpace(specialInstructions) && specialInstructions.Length > 1000)
        {
            errors.Add(Error.Validation("Event.SpecialInstructionsTooLong", "Special instructions cannot exceed 1000 characters."));
        }

        // Create value objects
        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsError)
        {
            errors.AddRange(dateRangeResult.Errors);
        }

        var locationResult = Location.Create(latitude, longitude);
        if (locationResult.IsError)
        {
            errors.AddRange(locationResult.Errors);
        }

        var addressResult = Address.Create(street, city, postalCode, country, state);
        if (addressResult.IsError)
        {
            errors.AddRange(addressResult.Errors);
        }

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(contactPhoneNumber))
        {
            var phoneResult = PhoneNumber.Create(contactPhoneNumber);
            if (phoneResult.IsError)
            {
                errors.AddRange(phoneResult.Errors);
            }
            else
            {
                phoneNumber = phoneResult.Value;
            }
        }

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            var emailResult = Email.Create(contactEmail);
            if (emailResult.IsError)
            {
                errors.AddRange(emailResult.Errors);
            }
            else
            {
                email = emailResult.Value;
            }
        }

        Money? entryFee = null;
        if (entryFeeAmount.HasValue && !string.IsNullOrWhiteSpace(entryFeeCurrency))
        {
            var entryFeeResult = Money.Create(entryFeeAmount.Value, entryFeeCurrency);
            if (entryFeeResult.IsError)
            {
                errors.AddRange(entryFeeResult.Errors);
            }
            else
            {
                entryFee = entryFeeResult.Value;
            }
        }

        if (organizerId == Guid.Empty)
        {
            errors.Add(Error.Validation("Event.InvalidOrganizer", "Organizer ID is required."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Event(
            title!.Trim(),
            description?.Trim() ?? string.Empty,
            eventType,
            dateRangeResult.Value,
            locationResult.Value,
            addressResult.Value,
            organizerId,
            phoneNumber,
            email,
            specialInstructions?.Trim(),
            entryFee,
            categoryId);
    }

    public ErrorOr<Updated> UpdateDetails(
        string title,
        string description,
        EventType eventType,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string? specialInstructions = null,
        Guid? categoryId = null)
    {
        if (Status == EventStatus.Published || Status == EventStatus.Active)
        {
            return Errors.Event.CannotModifyPublishedEvent;
        }

        var errors = new List<Error>();

        // Validate title
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
        {
            errors.Add(Errors.Event.TitleTooShort);
        }

        if (title?.Length > 200)
        {
            errors.Add(Errors.Event.TitleTooLong);
        }

        // Validate description
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 2000)
        {
            errors.Add(Errors.Event.DescriptionTooLong);
        }

        // Validate special instructions
        if (!string.IsNullOrWhiteSpace(specialInstructions) && specialInstructions.Length > 1000)
        {
            errors.Add(Error.Validation("Event.SpecialInstructionsTooLong", "Special instructions cannot exceed 1000 characters."));
        }

        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsError)
        {
            errors.AddRange(dateRangeResult.Errors);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Title = title!.Trim();
        Description = description?.Trim() ?? string.Empty;
        EventType = eventType;
        DateRange = dateRangeResult.Value;
        SpecialInstructions = specialInstructions?.Trim();
        CategoryId = categoryId;
        MarkAsModified();

        RaiseDomainEvent(new EventUpdatedDomainEvent(Id, OrganizerId));

        return Result.Updated;
    }

    public ErrorOr<Updated> UpdateLocation(
        double latitude,
        double longitude,
        string street,
        string city,
        string postalCode,
        string country,
        string? state = null)
    {
        if (Status == EventStatus.Published || Status == EventStatus.Active)
        {
            return Errors.Event.CannotModifyPublishedEvent;
        }

        var errors = new List<Error>();

        var locationResult = Location.Create(latitude, longitude);
        if (locationResult.IsError)
        {
            errors.AddRange(locationResult.Errors);
        }

        var addressResult = Address.Create(street, city, postalCode, country, state);
        if (addressResult.IsError)
        {
            errors.AddRange(addressResult.Errors);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Location = locationResult.Value;
        Address = addressResult.Value;
        MarkAsModified();

        return Result.Updated;
    }

    public ErrorOr<Updated> UpdateContactInformation(
        string? contactPhoneNumber = null,
        string? contactEmail = null)
    {
        var errors = new List<Error>();

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(contactPhoneNumber))
        {
            var phoneResult = PhoneNumber.Create(contactPhoneNumber);
            if (phoneResult.IsError)
            {
                errors.AddRange(phoneResult.Errors);
            }
            else
            {
                phoneNumber = phoneResult.Value;
            }
        }

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            var emailResult = Email.Create(contactEmail);
            if (emailResult.IsError)
            {
                errors.AddRange(emailResult.Errors);
            }
            else
            {
                email = emailResult.Value;
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        ContactPhoneNumber = phoneNumber;
        ContactEmail = email;
        MarkAsModified();

        return Result.Updated;
    }

    public ErrorOr<Updated> SetEntryFee(decimal? amount = null, string currency = "EUR")
    {
        if (Status == EventStatus.Published || Status == EventStatus.Active)
        {
            return Errors.Event.CannotModifyPublishedEvent;
        }

        if (!amount.HasValue)
        {
            EntryFee = null;
            MarkAsModified();
            return Result.Updated;
        }

        var entryFeeResult = Money.Create(amount.Value, currency);
        if (entryFeeResult.IsError)
        {
            return entryFeeResult.Errors;
        }

        EntryFee = entryFeeResult.Value;
        MarkAsModified();

        return Result.Updated;
    }

    public ErrorOr<Updated> SetEarlyBirdAccess(
        bool allowsEarlyBird,
        TimeSpan? earlyBirdTime = null,
        decimal? earlyBirdFeeAmount = null,
        string earlyBirdFeeCurrency = "EUR")
    {
        if (Status == EventStatus.Published || Status == EventStatus.Active)
        {
            return Errors.Event.CannotModifyPublishedEvent;
        }

        AllowsEarlyBird = allowsEarlyBird;

        if (!allowsEarlyBird)
        {
            EarlyBirdTime = null;
            EarlyBirdFee = null;
            MarkAsModified();
            return Result.Updated;
        }

        if (earlyBirdTime.HasValue)
        {
            if (earlyBirdTime.Value.TotalHours > 2)
            {
                return Error.Validation("Event.InvalidEarlyBirdTime", "Early bird access cannot be more than 2 hours.");
            }

            EarlyBirdTime = earlyBirdTime.Value;
        }

        if (earlyBirdFeeAmount.HasValue)
        {
            var earlyBirdFeeResult = Money.Create(earlyBirdFeeAmount.Value, earlyBirdFeeCurrency);
            if (earlyBirdFeeResult.IsError)
            {
                return earlyBirdFeeResult.Errors;
            }

            EarlyBirdFee = earlyBirdFeeResult.Value;
        }

        MarkAsModified();
        return Result.Updated;
    }

    public ErrorOr<Updated> Publish()
    {
        if (Status == EventStatus.Published)
        {
            return Result.Updated;
        }

        if (Status != EventStatus.Draft)
        {
            return Error.Conflict("Event.CannotPublish", $"Cannot publish event in {Status} status.");
        }

        Status = EventStatus.Published;
        PublishedOnUtc = DateTime.UtcNow;
        MarkAsModified();

        RaiseDomainEvent(new EventPublishedDomainEvent(Id, OrganizerId, Title));

        return Result.Updated;
    }

    public ErrorOr<Updated> Cancel(string? reason = null)
    {
        if (Status == EventStatus.Cancelled)
        {
            return Errors.Event.AlreadyCancelled;
        }

        if (Status == EventStatus.Completed)
        {
            return Errors.Event.CannotCancelCompletedEvent;
        }

        Status = EventStatus.Cancelled;
        MarkAsModified();

        RaiseDomainEvent(new EventCancelledDomainEvent(Id, OrganizerId, reason));

        return Result.Updated;
    }

    public ErrorOr<Updated> Postpone(DateTimeOffset newStartDate, DateTimeOffset newEndDate, string? reason = null)
    {
        if (Status == EventStatus.Completed)
        {
            return Error.Conflict("Event.CannotPostponeCompleted", "Cannot postpone a completed event.");
        }

        if (Status == EventStatus.Cancelled)
        {
            return Error.Conflict("Event.CannotPostponeCancelled", "Cannot postpone a cancelled event.");
        }

        var dateRangeResult = DateRange.Create(newStartDate, newEndDate);
        if (dateRangeResult.IsError)
        {
            return dateRangeResult.Errors;
        }

        var oldDateRange = DateRange;
        DateRange = dateRangeResult.Value;
        Status = EventStatus.Postponed;
        MarkAsModified();

        RaiseDomainEvent(new EventPostponedDomainEvent(Id, OrganizerId, oldDateRange, DateRange, reason));

        return Result.Updated;
    }

    public void MarkAsActive()
    {
        if (Status == EventStatus.Published && DateRange.HasStarted)
        {
            Status = EventStatus.Active;
            MarkAsModified();
        }
    }

    public void MarkAsCompleted()
    {
        if (Status == EventStatus.Active && DateRange.HasEnded)
        {
            Status = EventStatus.Completed;
            MarkAsModified();

            RaiseDomainEvent(new EventCompletedDomainEvent(Id, OrganizerId));
        }
    }

    public bool IsVisibleToPublic()
    {
        return Status is EventStatus.Published or EventStatus.Active or EventStatus.Completed;
    }

    public bool CanBeEditedBy(Guid userId, UserRole userRole)
    {
        return OrganizerId == userId || userRole is UserRole.Admin or UserRole.Moderator;
    }

    public bool CanBeDeletedBy(Guid userId, UserRole userRole)
    {
        return (OrganizerId == userId && Status == EventStatus.Draft) || 
               userRole is UserRole.Admin or UserRole.Moderator;
    }

    public double DistanceFrom(Location userLocation)
    {
        return Location.DistanceTo(userLocation);
    }

    public bool IsWithinRadius(Location userLocation, double radiusKm)
    {
        return Location.IsWithinRadius(userLocation, radiusKm);
    }

    public bool IsHappeningOn(DateOnly date)
    {
        var startDate = DateOnly.FromDateTime(DateRange.StartDate.Date);
        var endDate = DateOnly.FromDateTime(DateRange.EndDate.Date);
        
        return date >= startDate && date <= endDate;
    }

    public bool IsUpcoming()
    {
        return DateRange.IsFuture;
    }

    public bool IsActive()
    {
        return Status == EventStatus.Active || (Status == EventStatus.Published && DateRange.IsActive);
    }

    public bool HasEnded()
    {
        return Status == EventStatus.Completed || DateRange.HasEnded;
    }

    // Method to be called by Favorite entity
    internal void AddFavorite(Favorite favorite)
    {
        if (!_favorites.Contains(favorite))
        {
            _favorites.Add(favorite);
        }
    }

    internal void RemoveFavorite(Favorite favorite)
    {
        _favorites.Remove(favorite);
    }

    public void IncrementFavoriteCount()
    {
        FavoriteCount++;
        MarkAsModified();
    }

    public void DecrementFavoriteCount()
    {
        if (FavoriteCount > 0)
        {
            FavoriteCount--;
            MarkAsModified();
        }
    }

    public void UpdateFavoriteCount(int count)
    {
        if (count < 0) count = 0;
        FavoriteCount = count;
        MarkAsModified();
    }
}
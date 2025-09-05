using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Services;

public class EventValidationService : IEventValidationService
{
    private const int MaxSimultaneousEventsPerOrganizer = 3;
    private const double MinDistanceBetweenEventsKm = 1.0; // Minimum distance for same time slot

    public ErrorOr<Success> ValidateUserCanCreateEvent(User user)
    {
        if (!user.IsActive)
        {
            return Error.Forbidden("User.InactiveUser", "Inactive users cannot create events.");
        }

        if (!user.IsEmailVerified)
        {
            return Error.Forbidden("User.EmailNotVerified", "Email must be verified to create events.");
        }

        if (!user.CanCreateEvents())
        {
            return Error.Forbidden("User.InsufficientPermissions", "User does not have permission to create events.");
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidateUserCanEditEvent(User user, Event @event)
    {
        if (!user.IsActive)
        {
            return Error.Forbidden("User.InactiveUser", "Inactive users cannot edit events.");
        }

        if (!@event.CanBeEditedBy(user.Id, user.Role))
        {
            return Errors.Event.UnauthorizedAccess;
        }

        if (@event.Status == EventStatus.Completed)
        {
            return Error.Forbidden("Event.CannotEditCompleted", "Completed events cannot be edited.");
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidateUserCanDeleteEvent(User user, Event @event)
    {
        if (!user.IsActive)
        {
            return Error.Forbidden("User.InactiveUser", "Inactive users cannot delete events.");
        }

        if (!@event.CanBeDeletedBy(user.Id, user.Role))
        {
            return Errors.Event.UnauthorizedAccess;
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidateEventCanBePublished(Event @event)
    {
        if (@event.Status != EventStatus.Draft)
        {
            return Error.Conflict("Event.NotDraft", "Only draft events can be published.");
        }

        if (@event.DateRange.HasStarted)
        {
            return Errors.Event.StartDateInPast;
        }

        return ValidatePublicationRequirements(@event);
    }

    public ErrorOr<Success> ValidateEventCanBeCancelled(Event @event)
    {
        if (@event.Status == EventStatus.Cancelled)
        {
            return Errors.Event.AlreadyCancelled;
        }

        if (@event.Status == EventStatus.Completed)
        {
            return Errors.Event.CannotCancelCompletedEvent;
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidateEventCanBePostponed(Event @event)
    {
        if (@event.Status == EventStatus.Completed)
        {
            return Error.Conflict("Event.CannotPostponeCompleted", "Completed events cannot be postponed.");
        }

        if (@event.Status == EventStatus.Cancelled)
        {
            return Error.Conflict("Event.CannotPostponeCancelled", "Cancelled events cannot be postponed.");
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidateEventBusinessRules(Event @event)
    {
        var errors = new List<Error>();

        // Validate event duration based on type
        var duration = @event.DateRange.Duration;
        switch (@event.EventType)
        {
            case EventType.GarageSale:
                if (duration.TotalDays > 3)
                {
                    errors.Add(Error.Validation(
                        "Event.GarageSaleTooLong", 
                        "Garage sales cannot last more than 3 days."));
                }
                break;

            case EventType.EstateSale:
                if (duration.TotalDays > 5)
                {
                    errors.Add(Error.Validation(
                        "Event.EstateSaleTooLong", 
                        "Estate sales cannot last more than 5 days."));
                }
                break;

            case EventType.FleaMarket:
                if (duration.TotalDays > 7)
                {
                    errors.Add(Error.Validation(
                        "Event.FleaMarketTooLong", 
                        "Flea markets cannot last more than 7 days."));
                }
                break;
        }

        // Validate early bird settings
        if (@event.AllowsEarlyBird)
        {
            if (@event.EarlyBirdTime == null)
            {
                errors.Add(Error.Validation(
                    "Event.EarlyBirdTimeRequired", 
                    "Early bird time is required when early bird access is enabled."));
            }

            if (@event.EarlyBirdFee != null && @event.EntryFee != null && @event.EarlyBirdFee > @event.EntryFee)
            {
                errors.Add(Error.Validation(
                    "Event.EarlyBirdFeeExceedsEntryFee", 
                    "Early bird fee cannot exceed regular entry fee."));
            }
        }

        // Validate weekend timing for certain event types
        if (@event.EventType == EventType.GarageSale)
        {
            var startDay = @event.DateRange.StartDate.DayOfWeek;
            if (startDay is not (DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                errors.Add(Error.Validation(
                    "Event.GarageSaleWeekendOnly", 
                    "Garage sales should typically be held on weekends or Friday."));
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ValidateNoEventConflicts(
        Guid organizerId, 
        DateTimeOffset startDate, 
        DateTimeOffset endDate,
        Guid? excludeEventId = null)
    {
        // This would typically query the repository to check for conflicts
        // For now, we'll return a placeholder implementation
        await Task.CompletedTask;

        // In a real implementation, this would:
        // 1. Query events by organizer in the date range
        // 2. Check for location conflicts
        // 3. Validate maximum simultaneous events
        // 4. Return specific conflict information

        return Result.Success;
    }

    public ErrorOr<Success> ValidateEventLocation(Event @event)
    {
        var errors = new List<Error>();

        // Validate location based on event type
        switch (@event.EventType)
        {
            case EventType.GarageSale:
            case EventType.EstateSale:
            case EventType.MovingSale:
                // These should be at residential addresses
                // Could validate against address patterns or external services
                break;

            case EventType.FleaMarket:
            case EventType.CommunityWide:
                // These typically need larger spaces
                break;
        }

        // Basic location validation (could be enhanced with geocoding services)
        if (string.IsNullOrWhiteSpace(@event.Address.Street))
        {
            errors.Add(Error.Validation(
                "Event.IncompleteAddress", 
                "Complete address is required for published events."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return Result.Success;
    }

    public ErrorOr<Success> ValidatePublicationRequirements(Event @event)
    {
        var errors = new List<Error>();

        // Required fields for publication
        if (string.IsNullOrWhiteSpace(@event.Title))
        {
            errors.Add(Error.Validation(
                "Event.TitleRequired", 
                "Title is required for publication."));
        }

        if (string.IsNullOrWhiteSpace(@event.Description))
        {
            errors.Add(Error.Validation(
                "Event.DescriptionRequired", 
                "Description is required for publication."));
        }

        // Contact information should be provided
        if (@event.ContactPhoneNumber == null && @event.ContactEmail == null)
        {
            errors.Add(Error.Validation(
                "Event.ContactInfoRequired", 
                "At least one contact method (phone or email) is required for publication."));
        }

        // Address should be complete
        var addressValidation = ValidateEventLocation(@event);
        if (addressValidation.IsError)
        {
            errors.AddRange(addressValidation.Errors);
        }

        // Event should be in the future
        if (@event.DateRange.HasStarted)
        {
            errors.Add(Errors.Event.StartDateInPast);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return Result.Success;
    }
}
using ErrorOr;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Services;

public interface IEventValidationService
{
    /// <summary>
    /// Validate if user can create an event
    /// </summary>
    ErrorOr<Success> ValidateUserCanCreateEvent(User user);

    /// <summary>
    /// Validate if user can edit an event
    /// </summary>
    ErrorOr<Success> ValidateUserCanEditEvent(User user, Event @event);

    /// <summary>
    /// Validate if user can delete an event
    /// </summary>
    ErrorOr<Success> ValidateUserCanDeleteEvent(User user, Event @event);

    /// <summary>
    /// Validate if event can be published
    /// </summary>
    ErrorOr<Success> ValidateEventCanBePublished(Event @event);

    /// <summary>
    /// Validate if event can be cancelled
    /// </summary>
    ErrorOr<Success> ValidateEventCanBeCancelled(Event @event);

    /// <summary>
    /// Validate if event can be postponed
    /// </summary>
    ErrorOr<Success> ValidateEventCanBePostponed(Event @event);

    /// <summary>
    /// Validate business rules for event creation
    /// </summary>
    ErrorOr<Success> ValidateEventBusinessRules(Event @event);

    /// <summary>
    /// Check for conflicts with existing events (same organizer, overlapping dates)
    /// </summary>
    Task<ErrorOr<Success>> ValidateNoEventConflicts(
        Guid organizerId, 
        DateTimeOffset startDate, 
        DateTimeOffset endDate,
        Guid? excludeEventId = null);

    /// <summary>
    /// Validate if event location is appropriate for the event type
    /// </summary>
    ErrorOr<Success> ValidateEventLocation(Event @event);

    /// <summary>
    /// Check if event meets publication requirements (complete information)
    /// </summary>
    ErrorOr<Success> ValidatePublicationRequirements(Event @event);
}

public enum EventConflictType
{
    SameLocation,
    SameTimeSlot,
    TooManySimultaneousEvents
}
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Events;

public sealed record EventCreatedDomainEvent(
    Guid EventId,
    Guid OrganizerId,
    string Title) : BaseDomainEvent;

public sealed record EventUpdatedDomainEvent(
    Guid EventId,
    Guid OrganizerId) : BaseDomainEvent;

public sealed record EventPublishedDomainEvent(
    Guid EventId,
    Guid OrganizerId,
    string Title) : BaseDomainEvent;

public sealed record EventCancelledDomainEvent(
    Guid EventId,
    Guid OrganizerId,
    string? Reason) : BaseDomainEvent;

public sealed record EventPostponedDomainEvent(
    Guid EventId,
    Guid OrganizerId,
    DateRange OldDateRange,
    DateRange NewDateRange,
    string? Reason) : BaseDomainEvent;

public sealed record EventCompletedDomainEvent(
    Guid EventId,
    Guid OrganizerId) : BaseDomainEvent;
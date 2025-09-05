using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email) : BaseDomainEvent;

public sealed record UserProfileUpdatedDomainEvent(
    Guid UserId) : BaseDomainEvent;

public sealed record UserEmailVerifiedDomainEvent(
    Guid UserId,
    string Email) : BaseDomainEvent;

public sealed record UserRoleChangedDomainEvent(
    Guid UserId,
    UserRole OldRole,
    UserRole NewRole) : BaseDomainEvent;

public sealed record UserDeactivatedDomainEvent(
    Guid UserId) : BaseDomainEvent;

public sealed record UserActivatedDomainEvent(
    Guid UserId) : BaseDomainEvent;
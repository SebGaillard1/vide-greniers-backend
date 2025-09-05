using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Domain.Events;

public sealed record FavoriteAddedDomainEvent(
    Guid FavoriteId,
    Guid UserId,
    Guid EventId) : BaseDomainEvent;

public sealed record FavoriteRemovedDomainEvent(
    Guid FavoriteId,
    Guid UserId,
    Guid EventId) : BaseDomainEvent;

public sealed record FavoriteArchivedDomainEvent(
    Guid FavoriteId,
    Guid UserId,
    Guid EventId) : BaseDomainEvent;

public sealed record FavoriteRestoredDomainEvent(
    Guid FavoriteId,
    Guid UserId,
    Guid EventId) : BaseDomainEvent;
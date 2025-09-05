using ErrorOr;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;

namespace VideGreniers.Domain.Entities;

public sealed class Favorite : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public FavoriteStatus Status { get; private set; }
    public DateTime? ArchivedOnUtc { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Event Event { get; private set; } = null!;

    // Private constructor for EF Core
    private Favorite() { }

    private Favorite(Guid userId, Guid eventId, string? notes = null)
    {
        UserId = userId;
        EventId = eventId;
        Status = FavoriteStatus.Active;
        Notes = notes?.Trim();

        SetCreatedBy(userId);
    }

    public static ErrorOr<Favorite> Create(Guid userId, Guid eventId, string? notes = null)
    {
        var errors = new List<Error>();

        if (userId == Guid.Empty)
        {
            errors.Add(Error.Validation("Favorite.InvalidUserId", "User ID is required."));
        }

        if (eventId == Guid.Empty)
        {
            errors.Add(Error.Validation("Favorite.InvalidEventId", "Event ID is required."));
        }

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 500)
        {
            errors.Add(Error.Validation("Favorite.NotesTooLong", "Notes cannot exceed 500 characters."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        var favorite = new Favorite(userId, eventId, notes);
        
        favorite.RaiseDomainEvent(new FavoriteAddedDomainEvent(
            favorite.Id, 
            userId, 
            eventId));

        return favorite;
    }

    public ErrorOr<Updated> UpdateNotes(string? notes)
    {
        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 500)
        {
            return Error.Validation("Favorite.NotesTooLong", "Notes cannot exceed 500 characters.");
        }

        Notes = notes?.Trim();
        SetModifiedBy(UserId);

        return Result.Updated;
    }

    public void Archive()
    {
        if (Status == FavoriteStatus.Archived)
        {
            return;
        }

        Status = FavoriteStatus.Archived;
        ArchivedOnUtc = DateTime.UtcNow;
        SetModifiedBy(UserId);

        RaiseDomainEvent(new FavoriteArchivedDomainEvent(Id, UserId, EventId));
    }

    public new void Restore()
    {
        if (Status != FavoriteStatus.Archived)
        {
            return;
        }

        Status = FavoriteStatus.Active;
        ArchivedOnUtc = null;
        SetModifiedBy(UserId);

        RaiseDomainEvent(new FavoriteRestoredDomainEvent(Id, UserId, EventId));
    }

    public void Remove()
    {
        if (Status == FavoriteStatus.Removed)
        {
            return;
        }

        Status = FavoriteStatus.Removed;
        SetModifiedBy(UserId);

        RaiseDomainEvent(new FavoriteRemovedDomainEvent(Id, UserId, EventId));
    }

    public bool IsActive => Status == FavoriteStatus.Active;
    public bool IsArchived => Status == FavoriteStatus.Archived;
    public bool IsRemoved => Status == FavoriteStatus.Removed;

    public bool BelongsTo(Guid userId) => UserId == userId;
}
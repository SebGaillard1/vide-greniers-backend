namespace VideGreniers.Domain.Common.Models;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    public Guid Id { get; private init; } = Guid.NewGuid();

    public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? ModifiedOnUtc { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void MarkAsModified()
    {
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public bool Equals(BaseEntity? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseEntity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}
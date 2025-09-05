using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Domain.Common.Models;

/// <summary>
/// Base entity class following the exact requirements from the prompt
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    
    // Pour le soft delete
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    
    // Liste des événements du domaine
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
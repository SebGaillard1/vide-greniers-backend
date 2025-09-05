using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Domain.Entities;

/// <summary>
/// Favorite entity following the exact requirements from the prompt
/// </summary>
public sealed class FavoriteWithRequirements : Entity  
{
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime AddedAt { get; private set; }
    
    // Navigation
    public UserWithAuth? User { get; private set; }
    public EventWithRequirements? Event { get; private set; }
    
    private FavoriteWithRequirements(Guid userId, Guid eventId)
    {
        UserId = userId;
        EventId = eventId;
        AddedAt = DateTime.UtcNow;
    }
    
    public static ErrorOr<FavoriteWithRequirements> Create(Guid userId, Guid eventId)
    {
        if (userId == Guid.Empty)
            return Errors.Favorite.InvalidUserId;
            
        if (eventId == Guid.Empty)
            return Errors.Favorite.InvalidEventId;
        
        return new FavoriteWithRequirements(userId, eventId);
    }
}
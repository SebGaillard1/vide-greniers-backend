using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Entities;

/// <summary>
/// User entity following the exact requirements from the prompt with OAuth support
/// </summary>
public sealed class UserWithAuth : Entity
{
    public string Email { get; private set; }
    public string? FullName { get; private set; }
    public AuthProvider AuthProvider { get; private set; }
    public string? ExternalAuthId { get; private set; } // ID from Google/Apple
    
    // Navigation properties
    private readonly List<EventWithRequirements> _organizedEvents = new();
    public IReadOnlyList<EventWithRequirements> OrganizedEvents => _organizedEvents.AsReadOnly();
    
    private readonly List<FavoriteWithRequirements> _favorites = new();
    public IReadOnlyList<FavoriteWithRequirements> Favorites => _favorites.AsReadOnly();
    
    private UserWithAuth(
        string email,
        string? fullName,
        AuthProvider authProvider,
        string? externalAuthId)
    {
        Email = email;
        FullName = fullName;
        AuthProvider = authProvider;
        ExternalAuthId = externalAuthId;
    }
    
    public static ErrorOr<UserWithAuth> Create(
        string email,
        string? fullName,
        AuthProvider authProvider = AuthProvider.Local,
        string? externalAuthId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Errors.User.InvalidEmail;
        
        if (!IsValidEmail(email))
            return Errors.User.InvalidEmail;
        
        if (authProvider != AuthProvider.Local && string.IsNullOrWhiteSpace(externalAuthId))
            return Errors.User.ExternalAuthIdRequired;
        
        return new UserWithAuth(email, fullName, authProvider, externalAuthId);
    }
    
    public ErrorOr<EventWithRequirements> CreateEvent(
        string title,
        string description,
        LocationWithAddress location,
        DateRangeWithValueObject dateRange,
        string? contactEmail = null,
        string? contactPhone = null)
    {
        var eventResult = EventWithRequirements.Create(
            title,
            description,
            location,
            dateRange,
            this.Id,
            contactEmail ?? this.Email,
            contactPhone);
        
        if (eventResult.IsError)
            return eventResult.Errors;
        
        var @event = eventResult.Value;
        _organizedEvents.Add(@event);
        
        AddDomainEvent(new EventCreatedDomainEvent(@event.Id, this.Id, @event.Title));
        
        return @event;
    }
    
    public ErrorOr<FavoriteWithRequirements> AddToFavorites(EventWithRequirements @event)
    {
        if (_favorites.Any(f => f.EventId == @event.Id))
            return Errors.Favorite.AlreadyExists;
        
        var favorite = FavoriteWithRequirements.Create(this.Id, @event.Id);
        
        if (favorite.IsError)
            return favorite.Errors;
        
        _favorites.Add(favorite.Value);
        return favorite.Value;
    }
    
    public void RemoveFromFavorites(Guid eventId)
    {
        _favorites.RemoveAll(f => f.EventId == eventId);
    }
    
    public void UpdateProfile(string? fullName)
    {
        FullName = fullName;
        MarkAsUpdated();
    }
    
    private static bool IsValidEmail(string email)
    {
        // Simple email validation
        return email.Contains('@') && email.Contains('.');
    }
}
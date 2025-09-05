using VideGreniers.Application.Common.DTOs;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Common.Extensions;

/// <summary>
/// Extension methods for manual mapping of complex domain objects
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps an Event entity to EventDto with all complex properties
    /// </summary>
    public static EventDto ToDto(this Event eventEntity)
    {
        return new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            EventType = eventEntity.EventType,
            Status = eventEntity.Status,
            Location = new LocationDto
            {
                Street = eventEntity.Address.Street,
                City = eventEntity.Address.City,
                PostalCode = eventEntity.Address.PostalCode,
                Country = eventEntity.Address.Country,
                State = eventEntity.Address.State,
                Latitude = eventEntity.Location.Latitude,
                Longitude = eventEntity.Location.Longitude
            },
            StartDate = eventEntity.DateRange.StartDate,
            EndDate = eventEntity.DateRange.EndDate,
            ContactEmail = eventEntity.ContactEmail?.Value,
            ContactPhone = eventEntity.ContactPhoneNumber?.Value,
            SpecialInstructions = eventEntity.SpecialInstructions,
            EntryFeeAmount = eventEntity.EntryFee?.Amount,
            EntryFeeCurrency = eventEntity.EntryFee?.Currency,
            AllowsEarlyBird = eventEntity.AllowsEarlyBird,
            EarlyBirdTime = eventEntity.EarlyBirdTime.HasValue 
                ? TimeOnly.FromTimeSpan(eventEntity.EarlyBirdTime.Value) 
                : null,
            EarlyBirdFeeAmount = eventEntity.EarlyBirdFee?.Amount,
            EarlyBirdFeeCurrency = eventEntity.EarlyBirdFee?.Currency,
            PublishedOnUtc = eventEntity.PublishedOnUtc?.ToUniversalTime(),
            CreatedOnUtc = eventEntity.CreatedOnUtc,
            ModifiedOnUtc = eventEntity.ModifiedOnUtc,
            OrganizerName = eventEntity.Organizer != null 
                ? $"{eventEntity.Organizer.FirstName} {eventEntity.Organizer.LastName}".Trim() 
                : null,
            CategoryName = eventEntity.Category?.Name,
            CategoryIcon = eventEntity.Category?.IconName,
            CategoryColor = eventEntity.Category?.ColorHex,
            // These will be set separately by handlers
            DistanceKm = null,
            IsFavorite = null,
            FavoriteCount = 0
        };
    }

    /// <summary>
    /// Maps a User entity to UserDto with computed properties
    /// </summary>
    public static UserDto ToDto(this User user, int? createdEventsCount = null, int? favoritesCount = null)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber?.Value,
            CreatedOnUtc = user.CreatedOnUtc,
            ModifiedOnUtc = user.ModifiedOnUtc,
            CreatedEventsCount = createdEventsCount ?? 0,
            FavoritesCount = favoritesCount ?? 0
        };
    }

    /// <summary>
    /// Maps a Favorite entity to FavoriteDto
    /// </summary>
    public static FavoriteDto ToDto(this Favorite favorite)
    {
        return new FavoriteDto
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            EventId = favorite.EventId,
            Status = favorite.Status,
            CreatedOnUtc = favorite.CreatedOnUtc,
            ArchivedOnUtc = favorite.ArchivedOnUtc,
            Event = favorite.Event?.ToDto(),
            UserName = favorite.User != null 
                ? $"{favorite.User.FirstName} {favorite.User.LastName}".Trim() 
                : null
        };
    }
}
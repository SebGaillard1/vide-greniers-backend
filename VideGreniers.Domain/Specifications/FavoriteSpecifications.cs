using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Specifications;

public class UserFavoritesSpecification : BaseSpecification<Favorite>
{
    public UserFavoritesSpecification(Guid userId, FavoriteStatus? status = null) 
        : base(f => f.UserId == userId && (status == null || f.Status == status))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        AddInclude(f => f.Event.Organizer);
        ApplyOrderByDescending(f => f.CreatedOnUtc);
    }
}

public class ActiveUserFavoritesSpecification : BaseSpecification<Favorite>
{
    public ActiveUserFavoritesSpecification(Guid userId) 
        : base(f => f.UserId == userId && f.Status == FavoriteStatus.Active)
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        AddInclude(f => f.Event.Organizer);
        ApplyOrderByDescending(f => f.CreatedOnUtc);
    }
}

public class EventFavoritesSpecification : BaseSpecification<Favorite>
{
    public EventFavoritesSpecification(Guid eventId, bool onlyActive = true) 
        : base(f => f.EventId == eventId && (!onlyActive || f.Status == FavoriteStatus.Active))
    {
        AddInclude(f => f.User);
        ApplyOrderByDescending(f => f.CreatedOnUtc);
    }
}

public class UserEventFavoriteSpecification : BaseSpecification<Favorite>
{
    public UserEventFavoriteSpecification(Guid userId, Guid eventId) 
        : base(f => f.UserId == userId && f.EventId == eventId)
    {
        AddInclude(f => f.Event);
    }
}

public class FavoritesForUpcomingEventsSpecification : BaseSpecification<Favorite>
{
    public FavoritesForUpcomingEventsSpecification(Guid userId) 
        : base(f => f.UserId == userId && 
                   f.Status == FavoriteStatus.Active &&
                   f.Event.DateRange.StartDate > DateTimeOffset.UtcNow &&
                   (f.Event.Status == EventStatus.Published || f.Event.Status == EventStatus.Active))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        AddInclude(f => f.Event.Organizer);
        ApplyOrderBy(f => f.Event.DateRange.StartDate);
    }
}

public class FavoritesForTodayEventsSpecification : BaseSpecification<Favorite>
{
    public FavoritesForTodayEventsSpecification(Guid userId) 
        : base(f => f.UserId == userId && 
                   f.Status == FavoriteStatus.Active &&
                   f.Event.DateRange.StartDate.Date == DateTime.UtcNow.Date &&
                   (f.Event.Status == EventStatus.Published || f.Event.Status == EventStatus.Active))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        AddInclude(f => f.Event.Organizer);
        ApplyOrderBy(f => f.Event.DateRange.StartDate);
    }
}

public class RecentlyAddedFavoritesSpecification : BaseSpecification<Favorite>
{
    public RecentlyAddedFavoritesSpecification(Guid userId, int daysBack = 7) 
        : base(f => f.UserId == userId && 
                   f.Status == FavoriteStatus.Active &&
                   f.CreatedOnUtc >= DateTime.UtcNow.AddDays(-daysBack))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        ApplyOrderByDescending(f => f.CreatedOnUtc);
    }
}

public class FavoritesByLocationSpecification : BaseSpecification<Favorite>
{
    public FavoritesByLocationSpecification(Guid userId, string city) 
        : base(f => f.UserId == userId && 
                   f.Status == FavoriteStatus.Active &&
                   f.Event.Address.City.Contains(city) &&
                   (f.Event.Status == EventStatus.Published || f.Event.Status == EventStatus.Active))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        ApplyOrderBy(f => f.Event.DateRange.StartDate);
    }
}

public class FavoritesByCategorySpecification : BaseSpecification<Favorite>
{
    public FavoritesByCategorySpecification(Guid userId, Guid categoryId) 
        : base(f => f.UserId == userId && 
                   f.Status == FavoriteStatus.Active &&
                   f.Event.CategoryId == categoryId &&
                   (f.Event.Status == EventStatus.Published || f.Event.Status == EventStatus.Active))
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        ApplyOrderBy(f => f.Event.DateRange.StartDate);
    }
}

public class ArchivedFavoritesSpecification : BaseSpecification<Favorite>
{
    public ArchivedFavoritesSpecification(Guid userId) 
        : base(f => f.UserId == userId && f.Status == FavoriteStatus.Archived)
    {
        AddInclude(f => f.Event);
        AddInclude(f => f.Event.Category);
        ApplyOrderByDescending(f => f.ArchivedOnUtc);
    }
}

public class FavoritesCountByEventSpecification : BaseSpecification<Favorite>
{
    public FavoritesCountByEventSpecification(Guid eventId) 
        : base(f => f.EventId == eventId && f.Status == FavoriteStatus.Active)
    {
        // Used for counting, no includes needed
    }
}

public class UserEventsFavoriteStatusSpecification : BaseSpecification<Favorite>
{
    public UserEventsFavoriteStatusSpecification(Guid userId, IList<Guid> eventIds) 
        : base(f => f.UserId == userId && eventIds.Contains(f.EventId))
    {
        // Used for checking status, no includes needed for performance
    }
}
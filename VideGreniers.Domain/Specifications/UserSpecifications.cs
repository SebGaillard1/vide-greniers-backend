using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Specifications;

public class UserByEmailSpecification : BaseSpecification<User>
{
    public UserByEmailSpecification(string email) : base(u => u.Email.Value == email.ToLowerInvariant())
    {
    }
}

public class ActiveUsersSpecification : BaseSpecification<User>
{
    public ActiveUsersSpecification() : base(u => u.IsActive && !u.IsDeleted)
    {
        ApplyOrderBy(u => u.FirstName);
    }
}

public class UsersByRoleSpecification : BaseSpecification<User>
{
    public UsersByRoleSpecification(UserRole role) : base(u => u.Role == role && u.IsActive && !u.IsDeleted)
    {
        ApplyOrderBy(u => u.FirstName);
    }
}

public class OrganizersSpecification : BaseSpecification<User>
{
    public OrganizersSpecification() : base(u => 
        (u.Role == UserRole.Organizer || u.Role == UserRole.Admin) && 
        u.IsActive && 
        !u.IsDeleted)
    {
        ApplyOrderBy(u => u.FirstName);
    }
}

public class UserWithFavoritesSpecification : BaseSpecification<User>
{
    public UserWithFavoritesSpecification(Guid userId) : base(u => u.Id == userId)
    {
        AddInclude(u => u.Favorites);
        AddInclude("Favorites.Event");
        AddInclude("Favorites.Event.Category");
    }
}

public class UserWithCreatedEventsSpecification : BaseSpecification<User>
{
    public UserWithCreatedEventsSpecification(Guid userId) : base(u => u.Id == userId)
    {
        AddInclude(u => u.CreatedEvents);
        AddInclude("CreatedEvents.Category");
    }
}

public class RecentlyRegisteredUsersSpecification : BaseSpecification<User>
{
    public RecentlyRegisteredUsersSpecification(int daysBack = 30) : base(u => 
        u.CreatedOnUtc >= DateTime.UtcNow.AddDays(-daysBack) && 
        u.IsActive)
    {
        ApplyOrderByDescending(u => u.CreatedOnUtc);
    }
}

public class UnverifiedEmailUsersSpecification : BaseSpecification<User>
{
    public UnverifiedEmailUsersSpecification(int daysOld = 7) : base(u => 
        !u.IsEmailVerified && 
        u.CreatedOnUtc <= DateTime.UtcNow.AddDays(-daysOld) && 
        u.IsActive)
    {
        ApplyOrderBy(u => u.CreatedOnUtc);
    }
}

public class InactiveUsersSpecification : BaseSpecification<User>
{
    public InactiveUsersSpecification(int daysInactive = 90) : base(u => 
        (u.LastLoginUtc == null || u.LastLoginUtc <= DateTime.UtcNow.AddDays(-daysInactive)) && 
        u.IsActive && 
        !u.IsDeleted)
    {
        ApplyOrderBy(u => u.LastLoginUtc);
    }
}

public class UserSearchSpecification : BaseSpecification<User>
{
    public UserSearchSpecification(string searchTerm) : base(u =>
        (u.FirstName.Contains(searchTerm) || 
         u.LastName.Contains(searchTerm) || 
         u.Email.Value.Contains(searchTerm)) &&
        u.IsActive && 
        !u.IsDeleted)
    {
        ApplyOrderBy(u => u.FirstName);
    }
}

public class PaginatedUsersSpecification : BaseSpecification<User>
{
    public PaginatedUsersSpecification(
        int pageNumber, 
        int pageSize, 
        bool includeInactive = false) : base(u => 
        includeInactive || (u.IsActive && !u.IsDeleted))
    {
        ApplyOrderBy(u => u.FirstName);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}
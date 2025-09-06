using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Specifications;

public class UserNotificationsSpecification : BaseSpecification<Notification>
{
    public UserNotificationsSpecification(Guid userId, bool unreadOnly = false) 
        : base(n => n.UserId == userId && (!unreadOnly || n.Status != NotificationStatus.Read))
    {
        AddInclude(n => n.Event);
        ApplyOrderByDescending(n => n.CreatedOnUtc);
    }
}

public class UnreadNotificationsSpecification : BaseSpecification<Notification>
{
    public UnreadNotificationsSpecification(Guid userId) 
        : base(n => n.UserId == userId && n.Status != NotificationStatus.Read)
    {
        ApplyOrderByDescending(n => n.CreatedOnUtc);
    }
}

public class NotificationsByTypeSpecification : BaseSpecification<Notification>
{
    public NotificationsByTypeSpecification(Guid userId, NotificationType type) 
        : base(n => n.UserId == userId && n.Type == type)
    {
        AddInclude(n => n.Event);
        ApplyOrderByDescending(n => n.CreatedOnUtc);
    }
}

public class PendingNotificationsSpecification : BaseSpecification<Notification>
{
    public PendingNotificationsSpecification() 
        : base(n => n.Status == NotificationStatus.Pending)
    {
        AddInclude(n => n.User);
        AddInclude(n => n.Event);
        ApplyOrderBy(n => n.CreatedOnUtc);
    }
}

public class FailedNotificationsSpecification : BaseSpecification<Notification>
{
    public FailedNotificationsSpecification(DateTime? olderThan = null) 
        : base(n => n.Status == NotificationStatus.Failed && 
                   (!olderThan.HasValue || n.CreatedOnUtc <= olderThan.Value))
    {
        AddInclude(n => n.User);
        AddInclude(n => n.Event);
        ApplyOrderBy(n => n.CreatedOnUtc);
    }
}

public class EventNotificationsSpecification : BaseSpecification<Notification>
{
    public EventNotificationsSpecification(Guid eventId) 
        : base(n => n.EventId == eventId)
    {
        AddInclude(n => n.User);
        ApplyOrderByDescending(n => n.CreatedOnUtc);
    }
}

public class RecentNotificationsSpecification : BaseSpecification<Notification>
{
    public RecentNotificationsSpecification(Guid userId, int daysBack = 7) 
        : base(n => n.UserId == userId && 
                   n.CreatedOnUtc >= DateTime.UtcNow.AddDays(-daysBack))
    {
        AddInclude(n => n.Event);
        ApplyOrderByDescending(n => n.CreatedOnUtc);
    }
}
namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for user statistics
/// </summary>
public record UserStatsDto
{
    public int TotalFavorites { get; init; }
    public int TotalEventsCreated { get; init; }
    public int TotalNotifications { get; init; }
    public int UnreadNotifications { get; init; }
    public DateTime AccountCreatedDate { get; init; }
    public DateTime? LastLoginDate { get; init; }
    public int DaysActive { get; init; }
}
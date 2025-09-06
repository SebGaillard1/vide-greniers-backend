using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.UserActivities.DTOs;

/// <summary>
/// Data transfer object for user activity statistics
/// </summary>
public sealed record UserActivityStatisticsDto
{
    public int TotalActivities { get; init; }
    public Dictionary<UserActivityType, int> ActivityCounts { get; init; } = new();
    public int EventsViewed { get; init; }
    public int EventsFavorited { get; init; }
    public int EventsCreated { get; init; }
    public int SearchesPerformed { get; init; }
    public List<string> TopSearchTerms { get; init; } = new();
    public DateTime? LastActivityDate { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}
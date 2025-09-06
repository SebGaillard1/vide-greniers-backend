using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.UserActivities.DTOs;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.UserActivities.Queries.GetActivityStatistics;

public sealed class GetActivityStatisticsQueryHandler 
    : IRequestHandler<GetActivityStatisticsQuery, ErrorOr<UserActivityStatisticsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActivityStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserActivityStatisticsDto>> Handle(
        GetActivityStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30); // Default to last 30 days
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var activities = await _context.UserActivities
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && 
                       x.CreatedOnUtc >= startDate && 
                       x.CreatedOnUtc <= endDate)
            .ToListAsync(cancellationToken);

        var activityCounts = activities
            .GroupBy(x => x.ActivityType)
            .ToDictionary(g => g.Key, g => g.Count());

        var topSearchTerms = activities
            .Where(x => x.ActivityType == UserActivityType.EventSearched && 
                       !string.IsNullOrWhiteSpace(x.SearchTerm))
            .GroupBy(x => x.SearchTerm!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        var statistics = new UserActivityStatisticsDto
        {
            TotalActivities = activities.Count,
            ActivityCounts = activityCounts,
            EventsViewed = activityCounts.GetValueOrDefault(UserActivityType.EventViewed, 0),
            EventsFavorited = activityCounts.GetValueOrDefault(UserActivityType.EventFavorited, 0),
            EventsCreated = activityCounts.GetValueOrDefault(UserActivityType.EventCreated, 0),
            SearchesPerformed = activityCounts.GetValueOrDefault(UserActivityType.EventSearched, 0),
            TopSearchTerms = topSearchTerms,
            LastActivityDate = activities.MaxBy(x => x.CreatedOnUtc)?.CreatedOnUtc,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };

        return statistics;
    }
}
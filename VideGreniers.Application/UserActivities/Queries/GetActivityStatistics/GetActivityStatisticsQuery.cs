using ErrorOr;
using MediatR;
using VideGreniers.Application.UserActivities.DTOs;

namespace VideGreniers.Application.UserActivities.Queries.GetActivityStatistics;

/// <summary>
/// Query to get user activity statistics
/// </summary>
public sealed record GetActivityStatisticsQuery(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<ErrorOr<UserActivityStatisticsDto>>;
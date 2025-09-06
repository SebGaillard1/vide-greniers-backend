using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Models;
using VideGreniers.Application.UserActivities.DTOs;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.UserActivities.Queries.GetUserActivities;

/// <summary>
/// Query to get paginated list of user activities
/// </summary>
public sealed record GetUserActivitiesQuery : IRequest<ErrorOr<PaginatedList<UserActivityDto>>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public UserActivityType? ActivityType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
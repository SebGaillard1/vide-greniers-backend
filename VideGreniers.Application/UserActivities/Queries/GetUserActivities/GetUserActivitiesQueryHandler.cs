using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.Common.Models;
using VideGreniers.Application.UserActivities.DTOs;

namespace VideGreniers.Application.UserActivities.Queries.GetUserActivities;

public sealed class GetUserActivitiesQueryHandler 
    : IRequestHandler<GetUserActivitiesQuery, ErrorOr<PaginatedList<UserActivityDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserActivitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<PaginatedList<UserActivityDto>>> Handle(
        GetUserActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.UserActivities
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId);

        // Apply filters
        if (request.ActivityType.HasValue)
        {
            query = query.Where(x => x.ActivityType == request.ActivityType.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedOnUtc >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedOnUtc <= request.EndDate.Value);
        }

        // Order by most recent first
        query = query.OrderByDescending(x => x.CreatedOnUtc);

        // Project to DTO
        var dtoQuery = query.Select(x => new UserActivityDto(
            x.Id,
            x.ActivityType,
            x.EventId,
            x.Event != null ? x.Event.Title : null,
            x.SearchTerm,
            x.Metadata,
            x.CreatedOnUtc));

        var result = await PaginatedList<UserActivityDto>.CreateAsync(
            dtoQuery,
            request.Page,
            request.PageSize,
            cancellationToken);

        return result;
    }
}
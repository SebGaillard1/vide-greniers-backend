using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Users.Queries.GetUserFavorites;

/// <summary>
/// Query to get user's favorite events with pagination
/// </summary>
public record GetUserFavoritesQuery : IRequest<ErrorOr<PaginatedList<FavoriteDto>>>, ICacheableQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public string CacheKey => CacheKeys.UserFavorites(Guid.Empty, Page, PageSize); // Will be updated in handler with actual user ID
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(5);
}
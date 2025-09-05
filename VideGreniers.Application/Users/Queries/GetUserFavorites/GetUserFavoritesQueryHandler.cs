using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.Common.Models;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Queries.GetUserFavorites;

/// <summary>
/// Handler for getting user's favorite events
/// </summary>
public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, ErrorOr<PaginatedList<FavoriteDto>>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserFavoritesQueryHandler(
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<PaginatedList<FavoriteDto>>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to view favorites");
        }

        var userId = _currentUserService.UserId.Value;

        // Validate page size
        if (request.PageSize > 100)
        {
            return Error.Validation("GetUserFavoritesQuery.PageSize", "Page size cannot exceed 100");
        }

        // Get user's active favorites with event details
        var specification = new ActiveUserFavoritesSpecification(userId);
        var allFavorites = await _favoriteRepository.GetAsync(specification, cancellationToken);

        // Apply pagination manually since we're using specifications
        var totalCount = allFavorites.Count;
        var pagedFavorites = allFavorites
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var favoriteDtos = pagedFavorites.Select(f => f.ToDto()).ToList();

        var paginatedResult = PaginatedList<FavoriteDto>.Create(
            favoriteDtos,
            totalCount,
            request.Page,
            request.PageSize);

        return paginatedResult;
    }
}
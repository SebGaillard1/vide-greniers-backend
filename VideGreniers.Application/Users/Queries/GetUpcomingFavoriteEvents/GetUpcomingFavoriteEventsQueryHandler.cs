using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Queries.GetUpcomingFavoriteEvents;

/// <summary>
/// Handler for getting user's upcoming favorite events
/// </summary>
public class GetUpcomingFavoriteEventsQueryHandler : IRequestHandler<GetUpcomingFavoriteEventsQuery, ErrorOr<List<FavoriteDto>>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetUpcomingFavoriteEventsQueryHandler(
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IMapper mapper)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<List<FavoriteDto>>> Handle(GetUpcomingFavoriteEventsQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized("User must be authenticated to get upcoming favorites");
        }

        // Get domain user ID
        var userId = await _currentUserService.GetDomainUserIdAsync();
        if (!userId.HasValue)
        {
            return Error.NotFound("User not found");
        }

        // Try to get from cache first
        var cacheKey = $"favorites:upcoming:user_{userId.Value}:days_{request.DaysAhead}";
        var cachedResult = await _cacheService.GetAsync<List<FavoriteDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Get upcoming favorite events specification
        var endDate = DateTimeOffset.UtcNow.AddDays(request.DaysAhead);
        var specification = new FavoritesForUpcomingEventsSpecification(userId.Value);

        var favorites = await _favoriteRepository.GetAsync(specification, cancellationToken);

        // Filter by days ahead and convert to DTOs
        var upcomingFavorites = favorites
            .Where(f => f.Event.DateRange.StartDate <= endDate)
            .Select(f => f.ToDto())
            .ToList();

        // Cache the result for 5 minutes
        await _cacheService.SetAsync(cacheKey, upcomingFavorites, TimeSpan.FromMinutes(5), cancellationToken);

        return upcomingFavorites;
    }
}
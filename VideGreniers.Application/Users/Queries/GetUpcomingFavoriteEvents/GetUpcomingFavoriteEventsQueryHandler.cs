using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Queries.GetUpcomingFavoriteEvents;

/// <summary>
/// Handler for getting user's upcoming favorite events
/// </summary>
public class GetUpcomingFavoriteEventsQueryHandler : IRequestHandler<GetUpcomingFavoriteEventsQuery, ErrorOr<List<EventDto>>>
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

    public async Task<ErrorOr<List<EventDto>>> Handle(GetUpcomingFavoriteEventsQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to get upcoming favorites");
        }

        var userId = _currentUserService.UserId.Value;

        // Try to get from cache first
        var cacheKey = $"favorites:upcoming:user_{userId}:days_{request.DaysAhead}";
        var cachedResult = await _cacheService.GetAsync<List<EventDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Get upcoming favorite events specification
        var endDate = DateTimeOffset.UtcNow.AddDays(request.DaysAhead);
        var specification = new FavoritesForUpcomingEventsSpecification(userId);
        
        var favorites = await _favoriteRepository.GetAsync(specification, cancellationToken);
        
        // Filter by days ahead in case specification doesn't handle custom days
        var upcomingFavorites = favorites
            .Where(f => f.Event.DateRange.StartDate <= endDate)
            .Select(f => f.Event)
            .ToList();

        var result = _mapper.Map<List<EventDto>>(upcomingFavorites);

        // Cache the result for 5 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
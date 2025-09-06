using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Queries.GetFavoriteStatus;

/// <summary>
/// Handler for checking favorite status of multiple events
/// </summary>
public class GetFavoriteStatusQueryHandler : IRequestHandler<GetFavoriteStatusQuery, ErrorOr<Dictionary<Guid, bool>>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetFavoriteStatusQueryHandler(
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Dictionary<Guid, bool>>> Handle(GetFavoriteStatusQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to check favorite status");
        }

        var userId = _currentUserService.UserId.Value;

        if (!request.EventIds.Any())
        {
            return new Dictionary<Guid, bool>();
        }

        // Try to get from cache first
        var cacheKey = $"favorites:status:user_{userId}:{string.Join(",", request.EventIds.OrderBy(x => x))}";
        var cachedResult = await _cacheService.GetAsync<Dictionary<Guid, bool>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Get user's active favorites for these specific events
        var specification = new UserEventsFavoriteStatusSpecification(userId, request.EventIds);
        var favorites = await _favoriteRepository.GetAsync(specification, cancellationToken);

        // Create result dictionary
        var result = request.EventIds.ToDictionary(
            eventId => eventId,
            eventId => favorites.Any(f => f.EventId == eventId && f.Status == Domain.Enums.FavoriteStatus.Active)
        );

        // Cache the result for 2 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2), cancellationToken);

        return result;
    }
}
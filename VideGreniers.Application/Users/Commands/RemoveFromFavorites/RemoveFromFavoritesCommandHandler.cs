using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Commands.RemoveFromFavorites;

/// <summary>
/// Handler for removing an event from user's favorites
/// </summary>
public class RemoveFromFavoritesCommandHandler : IRequestHandler<RemoveFromFavoritesCommand, ErrorOr<Success>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemoveFromFavoritesCommandHandler(
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to remove favorites");
        }

        var userId = _currentUserService.UserId.Value;

        // Find the favorite
        var favoriteSpec = new UserEventFavoriteSpecification(userId, request.EventId);
        var favorite = await _favoriteRepository.GetSingleAsync(favoriteSpec, cancellationToken);

        if (favorite == null)
        {
            return Error.NotFound("Favorite not found");
        }

        if (favorite.Status != Domain.Enums.FavoriteStatus.Active)
        {
            return Error.Validation("RemoveFromFavorites.Favorite", "Favorite is already inactive");
        }

        // Archive the favorite (soft delete)
        favorite.Archive();

        await _favoriteRepository.UpdateAsync(favorite, cancellationToken);

        // Invalidate user favorites cache
        await _cacheService.RemoveByPatternAsync($"favorites:user_{userId}:*", cancellationToken);

        return Result.Success;
    }
}
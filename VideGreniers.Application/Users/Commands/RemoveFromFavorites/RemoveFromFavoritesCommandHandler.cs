using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;
using IUnitOfWork = VideGreniers.Domain.Interfaces.IUnitOfWork;

namespace VideGreniers.Application.Users.Commands.RemoveFromFavorites;

/// <summary>
/// Handler for removing an event from user's favorites
/// </summary>
public class RemoveFromFavoritesCommandHandler : IRequestHandler<RemoveFromFavoritesCommand, ErrorOr<Success>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromFavoritesCommandHandler(
        IRepository<Favorite> favoriteRepository,
        IRepository<Event> eventRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized("User must be authenticated to remove favorites");
        }

        // Get domain user ID
        var userId = await _currentUserService.GetDomainUserIdAsync();
        if (!userId.HasValue)
        {
            return Error.NotFound("User not found");
        }

        // Find the favorite
        var favoriteSpec = new UserEventFavoriteSpecification(userId.Value, request.EventId);
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

        // Decrement favorite count on event
        var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (eventEntity != null)
        {
            eventEntity.DecrementFavoriteCount();
            await _eventRepository.UpdateAsync(eventEntity, cancellationToken);
        }

        // Save changes to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate user favorites cache
        await _cacheService.RemoveByPatternAsync($"favorites:user_{userId.Value}:*", cancellationToken);

        return Result.Success;
    }
}
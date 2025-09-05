using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Commands.AddToFavorites;

/// <summary>
/// Handler for adding an event to user's favorites
/// </summary>
public class AddToFavoritesCommandHandler : IRequestHandler<AddToFavoritesCommand, ErrorOr<Success>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public AddToFavoritesCommandHandler(
        IRepository<Favorite> favoriteRepository,
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _favoriteRepository = favoriteRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Success>> Handle(AddToFavoritesCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to add favorites");
        }

        var userId = _currentUserService.UserId.Value;

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User not found");
        }

        // Verify event exists and is published
        var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (eventEntity == null)
        {
            return Error.NotFound("Event not found");
        }

        if (eventEntity.Status != Domain.Enums.EventStatus.Published && 
            eventEntity.Status != Domain.Enums.EventStatus.Active)
        {
            return Error.Validation("AddToFavorites.Event", "Only published or active events can be favorited");
        }

        // Check if already favorited
        var existingFavoriteSpec = new UserEventFavoriteSpecification(userId, request.EventId);
        var existingFavorite = await _favoriteRepository.GetSingleAsync(existingFavoriteSpec, cancellationToken);

        if (existingFavorite != null)
        {
            if (existingFavorite.Status == Domain.Enums.FavoriteStatus.Active)
            {
                return Error.Conflict("Event is already in favorites");
            }

            // Reactivate archived favorite
            existingFavorite.Restore();

            await _favoriteRepository.UpdateAsync(existingFavorite, cancellationToken);
        }
        else
        {
            // Create new favorite
            var favoriteResult = Favorite.Create(userId, request.EventId);
            if (favoriteResult.IsError)
            {
                return favoriteResult.Errors;
            }

            await _favoriteRepository.AddAsync(favoriteResult.Value, cancellationToken);
        }

        // Invalidate user favorites cache
        await _cacheService.RemoveByPatternAsync($"favorites:user_{userId}:*", cancellationToken);

        return Result.Success;
    }
}
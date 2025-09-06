using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Commands.ToggleFavorite;

/// <summary>
/// Handler for toggling an event's favorite status
/// </summary>
public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, ErrorOr<ToggleFavoriteResult>>
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public ToggleFavoriteCommandHandler(
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

    public async Task<ErrorOr<ToggleFavoriteResult>> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to toggle favorites");
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
            return Error.Validation("ToggleFavorite.Event", "Only published or active events can be favorited");
        }

        // Check if already favorited
        var existingFavoriteSpec = new UserEventFavoriteSpecification(userId, request.EventId);
        var existingFavorite = await _favoriteRepository.GetSingleAsync(existingFavoriteSpec, cancellationToken);

        string action;
        bool isFavorite;

        if (existingFavorite != null && existingFavorite.Status == Domain.Enums.FavoriteStatus.Active)
        {
            // Remove from favorites
            existingFavorite.Archive();
            await _favoriteRepository.UpdateAsync(existingFavorite, cancellationToken);
            
            // Decrement favorite count on event
            eventEntity.DecrementFavoriteCount();
            await _eventRepository.UpdateAsync(eventEntity, cancellationToken);
            
            action = "removed";
            isFavorite = false;
        }
        else if (existingFavorite != null)
        {
            // Reactivate archived favorite
            existingFavorite.Restore();
            await _favoriteRepository.UpdateAsync(existingFavorite, cancellationToken);
            
            // Increment favorite count on event
            eventEntity.IncrementFavoriteCount();
            await _eventRepository.UpdateAsync(eventEntity, cancellationToken);
            
            action = "added";
            isFavorite = true;
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
            
            // Increment favorite count on event
            eventEntity.IncrementFavoriteCount();
            await _eventRepository.UpdateAsync(eventEntity, cancellationToken);
            
            action = "added";
            isFavorite = true;
        }

        // Invalidate user favorites cache
        await _cacheService.RemoveByPatternAsync($"favorites:user_{userId}:*", cancellationToken);

        return new ToggleFavoriteResult(request.EventId, isFavorite, action);
    }
}
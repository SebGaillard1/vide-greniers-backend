using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Events.Queries.GetEventById;

/// <summary>
/// Handler for getting an event by ID
/// </summary>
public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, ErrorOr<EventDto>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(
        IRepository<Event> eventRepository,
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<EventDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new EventWithDetailsSpecification(request.EventId);
        var eventEntity = await _eventRepository.GetSingleAsync(specification, cancellationToken);

        if (eventEntity == null)
        {
            return Error.NotFound("Event not found");
        }

        // Map to DTO
        var eventDto = eventEntity.ToDto();

        // Check if current user has favorited this event
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            var favoriteSpec = new UserEventFavoriteSpecification(_currentUserService.UserId.Value, request.EventId);
            var favorite = await _favoriteRepository.GetSingleAsync(favoriteSpec, cancellationToken);
            
            // Update DTO with favorite status
            eventDto = eventDto with 
            { 
                IsFavorite = favorite != null && favorite.Status == Domain.Enums.FavoriteStatus.Active 
            };
        }

        // Get favorite count for this event
        var favoriteCountSpec = new FavoritesCountByEventSpecification(request.EventId);
        var favoriteCount = await _favoriteRepository.CountAsync(favoriteCountSpec, cancellationToken);
        
        eventDto = eventDto with { FavoriteCount = favoriteCount };

        return eventDto;
    }
}
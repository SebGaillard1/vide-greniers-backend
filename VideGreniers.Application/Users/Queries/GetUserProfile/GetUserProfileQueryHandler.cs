using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Handler for getting current user's profile
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ErrorOr<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(
        IRepository<User> userRepository,
        IRepository<Event> eventRepository,
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _eventRepository = eventRepository;
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to view profile");
        }

        var userId = _currentUserService.UserId.Value;

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User not found");
        }

        // Get user's created events count
        var eventsSpec = new EventsByOrganizerSpecification(userId, includeDeleted: false);
        var createdEventsCount = await _eventRepository.CountAsync(eventsSpec, cancellationToken);

        // Get user's active favorites count
        var favoritesSpec = new ActiveUserFavoritesSpecification(userId);
        var favoritesCount = await _favoriteRepository.CountAsync(favoritesSpec, cancellationToken);

        // Map to DTO with computed properties
        var userDto = user.ToDto(createdEventsCount, favoritesCount);

        return userDto;
    }
}
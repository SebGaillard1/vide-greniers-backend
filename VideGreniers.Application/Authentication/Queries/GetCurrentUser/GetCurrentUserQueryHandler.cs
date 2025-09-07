using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Handler for getting current authenticated user information with roles
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ErrorOr<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(
        IRepository<User> userRepository,
        IRepository<Event> eventRepository,
        IRepository<Favorite> favoriteRepository,
        ICurrentUserService currentUserService,
        IAuthenticationService authenticationService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _eventRepository = eventRepository;
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _authenticationService = authenticationService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        // Get user from domain repository
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User not found");
        }

        // Get user roles from authentication service
        var rolesResult = await _authenticationService.GetUserRolesAsync(userId);
        if (rolesResult.IsError)
        {
            return rolesResult.Errors;
        }

        // Get user's created events count
        var eventsSpec = new EventsByOrganizerSpecification(userId, includeDeleted: false);
        var createdEventsCount = await _eventRepository.CountAsync(eventsSpec, cancellationToken);

        // Get user's active favorites count
        var favoritesSpec = new ActiveUserFavoritesSpecification(userId);
        var favoritesCount = await _favoriteRepository.CountAsync(favoritesSpec, cancellationToken);

        // Map to DTO with computed properties and roles
        var userDto = user.ToDto(createdEventsCount, favoritesCount, rolesResult.Value);

        return userDto;
    }
}
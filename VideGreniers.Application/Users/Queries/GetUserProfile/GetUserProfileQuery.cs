using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Query to get current user's profile information
/// </summary>
public record GetUserProfileQuery : IRequest<ErrorOr<UserDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.UserById(Guid.Empty); // Will be updated in handler with actual user ID
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(10);
}
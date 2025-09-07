using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Query to get current authenticated user information with roles
/// </summary>
public record GetCurrentUserQuery : IRequest<ErrorOr<UserDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.UserById(Guid.Empty); // Will be updated in handler with actual user ID
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(5);
}
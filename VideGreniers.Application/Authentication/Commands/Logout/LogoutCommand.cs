using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Authentication.Commands.Logout;

/// <summary>
/// Command to logout a user and revoke their refresh token
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke</param>
public record LogoutCommand(string RefreshToken) : IRequest<ErrorOr<bool>>;
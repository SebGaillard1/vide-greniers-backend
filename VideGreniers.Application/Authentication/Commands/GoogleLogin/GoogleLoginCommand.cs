using ErrorOr;
using MediatR;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Authentication.Commands.GoogleLogin;

/// <summary>
/// Command to login with Google OAuth
/// </summary>
public sealed record GoogleLoginCommand(
    string IdToken,
    string? DeviceInfo = null) : IRequest<ErrorOr<AuthenticationResult>>;
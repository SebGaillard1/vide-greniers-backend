using ErrorOr;
using MediatR;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Authentication.Commands.AppleLogin;

/// <summary>
/// Command to login with Apple Sign In
/// </summary>
public sealed record AppleLoginCommand(
    string IdentityToken,
    string? UserFirstName = null,
    string? UserLastName = null,
    string? DeviceInfo = null) : IRequest<ErrorOr<AuthenticationResult>>;
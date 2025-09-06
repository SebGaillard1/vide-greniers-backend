using ErrorOr;
using MediatR;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Authentication.Commands.Login;

/// <summary>
/// Command to login a user with email and password
/// </summary>
/// <param name="Email">User email address</param>
/// <param name="Password">User password</param>
/// <param name="DeviceInfo">Information about the device (optional)</param>
public record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null) : IRequest<ErrorOr<AuthenticationResult>>;
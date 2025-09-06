using ErrorOr;
using MediatR;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Authentication.Commands.Register;

/// <summary>
/// Command to register a new user with email and password
/// </summary>
/// <param name="Email">User email address</param>
/// <param name="Password">User password</param>
/// <param name="FirstName">User first name (optional)</param>
/// <param name="LastName">User last name (optional)</param>
/// <param name="DeviceInfo">Information about the device (optional)</param>
public record RegisterCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? DeviceInfo = null) : IRequest<ErrorOr<AuthenticationResult>>;
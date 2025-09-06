using ErrorOr;
using MediatR;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a refresh token
/// </summary>
/// <param name="AccessToken">The expired access token</param>
/// <param name="RefreshToken">The refresh token to use</param>
/// <param name="DeviceInfo">Information about the device (optional)</param>
public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string? DeviceInfo = null) : IRequest<ErrorOr<AuthenticationResult>>;
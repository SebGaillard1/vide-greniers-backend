using ErrorOr;
using VideGreniers.Application.Common.DTOs;

namespace VideGreniers.Application.Authentication.Models;

/// <summary>
/// Result of authentication operations - matches iOS AuthResponse structure
/// </summary>
public record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto User);

/// <summary>
/// User information from authentication
/// </summary>
public record AuthenticatedUser(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt);

/// <summary>
/// Refresh token information
/// </summary>
public record RefreshTokenInfo(
    string Token,
    string UserId,
    DateTime ExpiresAt,
    bool IsRevoked,
    string? DeviceInfo);
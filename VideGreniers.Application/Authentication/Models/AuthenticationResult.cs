using ErrorOr;

namespace VideGreniers.Application.Authentication.Models;

/// <summary>
/// Result of authentication operations
/// </summary>
public record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime ExpiresAt);

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
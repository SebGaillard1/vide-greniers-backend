using System.Security.Claims;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the given user
    /// </summary>
    /// <param name="user">The user to generate token for</param>
    /// <param name="roles">User's roles</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(AuthenticatedUser user, IList<string> roles);

    /// <summary>
    /// Generates a secure refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns claims principal
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Claims principal if valid, null if invalid</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the expiration time for access tokens
    /// </summary>
    DateTime GetAccessTokenExpiration();

    /// <summary>
    /// Gets the expiration time for refresh tokens
    /// </summary>
    DateTime GetRefreshTokenExpiration();
}
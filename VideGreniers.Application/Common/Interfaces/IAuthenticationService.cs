using ErrorOr;
using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for user authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <returns>Authentication result</returns>
    Task<ErrorOr<AuthenticationResult>> RegisterAsync(string email, string password, string? firstName, string? lastName);

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Authentication result</returns>
    Task<ErrorOr<AuthenticationResult>> LoginAsync(string email, string password);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="accessToken">Current access token</param>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New authentication result</returns>
    Task<ErrorOr<AuthenticationResult>> RefreshTokenAsync(string accessToken, string refreshToken);

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to revoke</param>
    /// <returns>Success or error</returns>
    Task<ErrorOr<bool>> LogoutAsync(string refreshToken);

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User information</returns>
    Task<ErrorOr<AuthenticatedUser>> GetUserByIdAsync(string userId);

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User information</returns>
    Task<ErrorOr<AuthenticatedUser>> GetUserByEmailAsync(string email);

    /// <summary>
    /// Check if email exists
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if exists</returns>
    Task<bool> EmailExistsAsync(string email);
}
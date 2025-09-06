namespace VideGreniers.Application.Authentication.Common;

/// <summary>
/// Result returned from authentication operations
/// </summary>
/// <param name="User">User information</param>
/// <param name="AccessToken">JWT access token</param>
/// <param name="RefreshToken">Refresh token</param>
/// <param name="AccessTokenExpiry">When the access token expires</param>
public record AuthenticationResult(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry);

/// <summary>
/// User data transfer object
/// </summary>
/// <param name="Id">User ID</param>
/// <param name="FirstName">User first name</param>
/// <param name="LastName">User last name</param>
/// <param name="Email">User email</param>
/// <param name="Roles">User roles</param>
/// <param name="AuthProvider">Authentication provider (Email, Google, Apple)</param>
/// <param name="IsEmailVerified">Whether email is verified</param>
public record UserDto(
    Guid Id,
    string? FirstName,
    string? LastName,
    string Email,
    IList<string> Roles,
    string AuthProvider,
    bool IsEmailVerified);
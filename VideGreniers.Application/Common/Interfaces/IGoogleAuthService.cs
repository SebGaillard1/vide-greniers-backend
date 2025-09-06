namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for Google OAuth authentication
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Validates a Google ID token and returns user information
    /// </summary>
    /// <param name="idToken">Google ID token to validate</param>
    /// <returns>Google user information if valid, null if invalid</returns>
    Task<GoogleUserInfo?> ValidateIdTokenAsync(string idToken);
}

/// <summary>
/// Google user information from OAuth
/// </summary>
/// <param name="Id">Google user ID</param>
/// <param name="Email">User email address</param>
/// <param name="Name">Full name</param>
/// <param name="FirstName">First name</param>
/// <param name="LastName">Last name</param>
/// <param name="Picture">Profile picture URL</param>
/// <param name="EmailVerified">Whether email is verified</param>
public record GoogleUserInfo(
    string Id,
    string Email,
    string? Name,
    string? FirstName,
    string? LastName,
    string? Picture,
    bool EmailVerified);
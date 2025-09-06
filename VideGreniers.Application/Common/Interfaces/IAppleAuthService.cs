namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for Apple OAuth authentication
/// </summary>
public interface IAppleAuthService
{
    /// <summary>
    /// Validates an Apple identity token and returns user information
    /// </summary>
    /// <param name="identityToken">Apple identity token to validate</param>
    /// <returns>Apple user information if valid, null if invalid</returns>
    Task<AppleUserInfo?> ValidateIdentityTokenAsync(string identityToken);
}

/// <summary>
/// Apple user information from OAuth
/// </summary>
/// <param name="Id">Apple user ID (sub claim)</param>
/// <param name="Email">User email address</param>
/// <param name="FirstName">First name (from user object if provided)</param>
/// <param name="LastName">Last name (from user object if provided)</param>
/// <param name="EmailVerified">Whether email is verified (Apple emails are always verified)</param>
public record AppleUserInfo(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified = true);
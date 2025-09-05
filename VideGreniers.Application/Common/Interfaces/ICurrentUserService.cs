namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current authenticated user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID if authenticated
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email if authenticated
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's name if authenticated
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Indicates whether the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    bool IsInRole(string role);
}
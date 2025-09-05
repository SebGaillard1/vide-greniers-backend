using Microsoft.AspNetCore.Identity;

namespace VideGreniers.Infrastructure.Identity;

/// <summary>
/// ApplicationUser extends IdentityUser for ASP.NET Core Identity
/// This will be mapped to our Domain User entity
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Reference to the Domain User entity
    /// </summary>
    public Guid? DomainUserId { get; set; }

    /// <summary>
    /// First name from user profile
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name from user profile
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// External authentication provider (Google, Apple)
    /// </summary>
    public string? AuthProvider { get; set; }

    /// <summary>
    /// External authentication ID from provider
    /// </summary>
    public string? ExternalAuthId { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the user was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
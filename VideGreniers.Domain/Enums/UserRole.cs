namespace VideGreniers.Domain.Enums;

/// <summary>
/// Represents user roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user who can browse events and manage favorites
    /// </summary>
    User = 0,

    /// <summary>
    /// User who can create and manage events
    /// </summary>
    Organizer = 1,

    /// <summary>
    /// System administrator with full access
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Moderator who can review and manage content
    /// </summary>
    Moderator = 3
}
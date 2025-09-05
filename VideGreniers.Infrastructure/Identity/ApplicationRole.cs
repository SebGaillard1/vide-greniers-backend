using Microsoft.AspNetCore.Identity;

namespace VideGreniers.Infrastructure.Identity;

/// <summary>
/// ApplicationRole extends IdentityRole for ASP.NET Core Identity
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Description of the role
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public ApplicationRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
}
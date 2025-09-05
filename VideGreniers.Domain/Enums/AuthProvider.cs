namespace VideGreniers.Domain.Enums;

/// <summary>
/// Represents the authentication provider used by a user
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// Local authentication using email/password
    /// </summary>
    Local = 0,

    /// <summary>
    /// Google OAuth authentication
    /// </summary>
    Google = 1,

    /// <summary>
    /// Apple Sign In authentication
    /// </summary>
    Apple = 2
}
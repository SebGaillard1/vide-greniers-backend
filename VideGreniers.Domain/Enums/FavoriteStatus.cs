namespace VideGreniers.Domain.Enums;

/// <summary>
/// Represents the status of a user's favorite event
/// </summary>
public enum FavoriteStatus
{
    /// <summary>
    /// Favorite is active and visible in user's list
    /// </summary>
    Active = 0,

    /// <summary>
    /// Favorite is archived but kept for history
    /// </summary>
    Archived = 1,

    /// <summary>
    /// Favorite was removed but event data retained
    /// </summary>
    Removed = 2
}
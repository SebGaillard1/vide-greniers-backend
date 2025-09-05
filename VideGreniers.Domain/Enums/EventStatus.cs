namespace VideGreniers.Domain.Enums;

/// <summary>
/// Represents the status of an event in its lifecycle
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Event is being created/edited and not yet visible to public
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Event is published and visible to public
    /// </summary>
    Published = 1,

    /// <summary>
    /// Event is currently active/happening
    /// </summary>
    Active = 2,

    /// <summary>
    /// Event has ended successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Event has been cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Event has been postponed to a later date
    /// </summary>
    Postponed = 5
}
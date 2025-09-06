namespace VideGreniers.Domain.Enums;

/// <summary>
/// Types of user activities that are tracked in the system
/// </summary>
public enum UserActivityType
{
    /// <summary>
    /// User viewed an event
    /// </summary>
    EventViewed = 0,

    /// <summary>
    /// User added an event to favorites
    /// </summary>
    EventFavorited = 1,

    /// <summary>
    /// User removed an event from favorites
    /// </summary>
    EventUnfavorited = 2,

    /// <summary>
    /// User created a new event
    /// </summary>
    EventCreated = 3,

    /// <summary>
    /// User updated their own event
    /// </summary>
    EventUpdated = 4,

    /// <summary>
    /// User published their event
    /// </summary>
    EventPublished = 5,

    /// <summary>
    /// User cancelled their event
    /// </summary>
    EventCancelled = 6,

    /// <summary>
    /// User searched for events
    /// </summary>
    EventSearched = 7,

    /// <summary>
    /// User logged in
    /// </summary>
    UserLogin = 8,

    /// <summary>
    /// User logged out
    /// </summary>
    UserLogout = 9,

    /// <summary>
    /// User updated their profile
    /// </summary>
    ProfileUpdated = 10,

    /// <summary>
    /// User saved a search
    /// </summary>
    SearchSaved = 11,

    /// <summary>
    /// User executed a saved search
    /// </summary>
    SavedSearchExecuted = 12
}
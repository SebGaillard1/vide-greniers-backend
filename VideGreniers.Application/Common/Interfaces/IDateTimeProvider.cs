namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Service for providing current date and time
/// This abstraction allows for easier testing by mocking the current time
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets today's date
    /// </summary>
    DateOnly Today { get; }
}
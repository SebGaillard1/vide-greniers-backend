using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Infrastructure.Services;

/// <summary>
/// Implementation of IDateTimeProvider that returns current system time
/// This service can be easily mocked for testing
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime Now => DateTime.Now;

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
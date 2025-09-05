using ErrorOr;
using VideGreniers.Domain.Common.Errors;

namespace VideGreniers.Domain.ValueObjects;

public sealed record DateRange
{
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset EndDate { get; }

    private DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static ErrorOr<DateRange> Create(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var errors = new List<Error>();

        if (startDate >= endDate)
        {
            errors.Add(Errors.Event.InvalidDateRange);
        }

        if (startDate < DateTimeOffset.UtcNow.AddHours(-1)) // Allow 1 hour buffer for scheduling
        {
            errors.Add(Errors.Event.StartDateInPast);
        }

        var duration = endDate - startDate;
        if (duration.TotalDays > 30) // Maximum 30 days for a garage sale event
        {
            errors.Add(Error.Validation("DateRange.TooLong", "Event cannot last more than 30 days."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new DateRange(startDate, endDate);
    }

    public TimeSpan Duration => EndDate - StartDate;

    public bool IsActive => IsActiveAt(DateTimeOffset.UtcNow);

    public bool IsActiveAt(DateTimeOffset referenceTime)
    {
        return referenceTime >= StartDate && referenceTime <= EndDate;
    }

    public bool HasStarted => HasStartedAt(DateTimeOffset.UtcNow);

    public bool HasStartedAt(DateTimeOffset referenceTime)
    {
        return referenceTime >= StartDate;
    }

    public bool HasEnded => HasEndedAt(DateTimeOffset.UtcNow);

    public bool HasEndedAt(DateTimeOffset referenceTime)
    {
        return referenceTime > EndDate;
    }

    public bool IsFuture => IsFutureAt(DateTimeOffset.UtcNow);

    public bool IsFutureAt(DateTimeOffset referenceTime)
    {
        return StartDate > referenceTime;
    }

    public bool OverlapsWith(DateRange other)
    {
        return StartDate < other.EndDate && EndDate > other.StartDate;
    }

    public bool Contains(DateTimeOffset date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public string GetFormattedRange(string format = "yyyy-MM-dd HH:mm")
    {
        if (StartDate.Date == EndDate.Date)
        {
            return $"{StartDate.ToString($"yyyy-MM-dd")} from {StartDate.ToString("HH:mm")} to {EndDate.ToString("HH:mm")}";
        }

        return $"{StartDate.ToString(format)} to {EndDate.ToString(format)}";
    }

    public override string ToString() => GetFormattedRange();
}
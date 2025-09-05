using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Domain.ValueObjects;

/// <summary>
/// DateRange value object following the exact requirements from the prompt
/// </summary>
public sealed class DateRangeWithValueObject : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    
    private DateRangeWithValueObject(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
    
    public static ErrorOr<DateRangeWithValueObject> Create(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            return Errors.DateRange.EndDateMustBeAfterStartDate;
            
        if (startDate < DateTime.UtcNow.AddHours(-1)) // Petite marge pour les fuseaux horaires
            return Errors.DateRange.StartDateCannotBeInPast;
        
        return new DateRangeWithValueObject(startDate, endDate);
    }
    
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    
    public bool HasPassed => DateTime.UtcNow > EndDate;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
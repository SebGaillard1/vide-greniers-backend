using System.Linq.Expressions;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Specifications;

/// <summary>
/// Specification for active events
/// </summary>
public sealed class ActiveEventSpecification : ISpecificationForRequirements<EventWithRequirements>
{
    public bool IsSatisfiedBy(EventWithRequirements entity)
    {
        return entity.Status == EventStatus.Published 
               && entity.DateRange.IsActive 
               && !entity.IsDeleted;
    }
    
    public Expression<Func<EventWithRequirements, bool>> ToExpression()
    {
        return e => e.Status == EventStatus.Published 
                    && DateTime.UtcNow >= e.DateRange.StartDate 
                    && DateTime.UtcNow <= e.DateRange.EndDate
                    && !e.IsDeleted;
    }
}
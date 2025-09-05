using System.Linq.Expressions;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Specifications;

/// <summary>
/// Specification for published events
/// </summary>
public sealed class PublishedEventSpecification : ISpecificationForRequirements<EventWithRequirements>
{
    public bool IsSatisfiedBy(EventWithRequirements entity)
    {
        return entity.Status == EventStatus.Published && !entity.IsDeleted;
    }
    
    public Expression<Func<EventWithRequirements, bool>> ToExpression()
    {
        return e => e.Status == EventStatus.Published && !e.IsDeleted;
    }
}
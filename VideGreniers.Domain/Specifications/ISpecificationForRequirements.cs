using System.Linq.Expressions;

namespace VideGreniers.Domain.Specifications;

/// <summary>
/// Specification interface for implementing the Specification pattern
/// </summary>
public interface ISpecificationForRequirements<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}
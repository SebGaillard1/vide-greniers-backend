namespace VideGreniers.Domain.Common.Models;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
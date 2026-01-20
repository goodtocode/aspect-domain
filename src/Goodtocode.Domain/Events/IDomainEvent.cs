namespace Goodtocode.Domain.Events;

public interface IDomainEvent { }
public interface IDomainEvent<T> : IDomainEvent
{
    T Item { get; }
    DateTime OccurredOn { get; }
}
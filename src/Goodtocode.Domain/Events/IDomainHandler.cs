namespace Goodtocode.Domain.Events;

public interface IDomainHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent);
}
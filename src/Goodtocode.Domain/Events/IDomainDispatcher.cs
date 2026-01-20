namespace Goodtocode.Domain.Events;

public interface IDomainDispatcher
{
    Task DispatchAsync<TModel>(IEnumerable<IDomainEvent<TModel>> domainEvents);
}
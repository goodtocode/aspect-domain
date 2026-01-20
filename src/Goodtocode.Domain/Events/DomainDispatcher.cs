namespace Goodtocode.Domain.Events;

/// <summary>
/// Dispatches domain events to their corresponding handlers using dependency injection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DomainDispatcher"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider used to resolve domain event handlers.</param>
public class DomainDispatcher(IServiceProvider serviceProvider) : IDomainDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Dispatches the specified domain events to their handlers asynchronously.
    /// </summary>
    /// <typeparam name="TModel">The type of the domain model.</typeparam>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DispatchAsync<TModel>(IEnumerable<IDomainEvent<TModel>> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainHandler<>).MakeGenericType(domainEvent.GetType());
            var handler = _serviceProvider.GetService(handlerType);
            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, [domainEvent])!;
            }
        }
    }
}



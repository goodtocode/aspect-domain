using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Events;

[TestClass]
public sealed class DomainDispatcherTests
{
    private sealed class TestEvent : IDomainEvent<TestEvent>
    {
        public TestEvent Item => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }

    private class TestHandler : IDomainHandler<TestEvent>
    {
        public bool WasCalled { get; private set; }
        public Task HandleAsync(TestEvent domainEvent)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ServiceProviderStub(object? handler) : IServiceProvider
    {
        private readonly object? handler = handler;

        public object? Handler => handler;

        public object? GetService(Type serviceType) => Handler;
    }

    [TestMethod]
    public async Task DispatchAsyncCallsHandlerWhenHandlerExists()
    {
        // Arrange
        var handler = new TestHandler();
        var serviceProvider = new ServiceProviderStub(handler);
        var dispatcher = new DomainDispatcher(serviceProvider);
        var events = new[] { new TestEvent() };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        Assert.IsTrue(handler.WasCalled, "Handler should be called for dispatched event.");
    }

    [TestMethod]
    public async Task DispatchAsyncDoesNotThrowWhenNoHandlerFound()
    {
        // Arrange
        var serviceProvider = new ServiceProviderStub(null);
        var dispatcher = new DomainDispatcher(serviceProvider);
        var events = new[] { new TestEvent() };

        // Act & Assert
        await dispatcher.DispatchAsync(events);
        // No exception expected
    }

    [TestMethod]
    public async Task DispatchAsyncDoesNothingWhenEventsEmpty()
    {
        // Arrange
        var handler = new TestHandler();
        var serviceProvider = new ServiceProviderStub(handler);
        var dispatcher = new DomainDispatcher(serviceProvider);
        var events = Array.Empty<TestEvent>();

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        Assert.IsFalse(handler.WasCalled, "Handler should not be called when no events are dispatched.");
    }
}

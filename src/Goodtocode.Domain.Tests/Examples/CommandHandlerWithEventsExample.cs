using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;
using Goodtocode.Domain.Tests.TestHelpers;
using Microsoft.Testing.Platform.Services;

namespace Goodtocode.Domain.Tests.Examples;

/// <summary>
/// Complete example demonstrating:
/// 1. Command Handler pattern
/// 2. Creating SecuredEntity domain objects
/// 3. Adding domain events
/// 4. Dispatching events
/// 5. Publishing to service bus
/// </summary>
[TestClass]
public class CommandHandlerWithEventsExample
{
    #region 1. Domain Entity (Secured Aggregate Root)

    /// <summary>
    /// Person aggregate root - inherits from SecuredEntity for multi-tenant support
    /// </summary>
    private sealed class Person : SecuredEntity<Person>
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public bool IsVerified { get; private set; }

        private Person() { } // EF constructor

        public Person(Guid id, string firstName, string lastName, string email, Guid ownerId, Guid tenantId)
            : base(id, ownerId, tenantId)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            IsVerified = false;

            // Add domain event for creation
            AddDomainEvent(new PersonCreatedEvent(this));
        }

        public void Verify()
        {
            if (!IsVerified)
            {
                IsVerified = true;
                this.MarkModified();

                // Add domain event for verification
                AddDomainEvent(new PersonVerifiedEvent(this));
            }
        }

        public void UpdateEmail(string newEmail)
        {
            if (Email != newEmail)
            {
                var oldEmail = Email;
                Email = newEmail;
                this.MarkModified();

                // Add domain event for email change
                AddDomainEvent(new PersonEmailChangedEvent(this, oldEmail, newEmail));
            }
        }
    }

    #endregion

    #region 2. Domain Events

    /// <summary>
    /// Event raised when a person is created
    /// </summary>
    private sealed class PersonCreatedEvent(CommandHandlerWithEventsExample.Person person) : IDomainEvent<Person>
    {
        public Person Item { get; } = person;
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event raised when a person is verified
    /// </summary>
    private sealed class PersonVerifiedEvent(CommandHandlerWithEventsExample.Person person) : IDomainEvent<Person>
    {
        public Person Item { get; } = person;
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event raised when a person's email changes
    /// </summary>
    private sealed class PersonEmailChangedEvent(CommandHandlerWithEventsExample.Person person, string oldEmail, string newEmail) : IDomainEvent<Person>
    {
        public Person Item { get; } = person;
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string OldEmail { get; } = oldEmail;
        public string NewEmail { get; } = newEmail;
    }

    #endregion

    #region 3. Commands

    /// <summary>
    /// Command to create a new person
    /// </summary>
    private sealed record CreatePersonCommand(
        string FirstName,
        string LastName,
        string Email,
        Guid OwnerId,
        Guid TenantId);

    /// <summary>
    /// Command to verify a person
    /// </summary>
    private sealed record VerifyPersonCommand(Guid PersonId);

    #endregion

    #region 4. Simulated DbContext (No EF dependency)

    /// <summary>
    /// Simulated database context interface
    /// </summary>
    private interface IAppDbContext
    {
        ICollection<Person> Persons { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// In-memory implementation for demonstration
    /// </summary>
    private sealed class InMemoryDbContext : IAppDbContext
    {
        public ICollection<Person> Persons { get; } = [];

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Simulate database save
            return Task.FromResult(Persons.Count);
        }
    }

    #endregion

    #region 5. Service Bus Integration

    /// <summary>
    /// Service bus message interface
    /// </summary>
    private interface IServiceBusMessage
    {
        string MessageType { get; }
        string EventData { get; }
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Service bus publisher interface (Azure Service Bus, RabbitMQ, etc.)
    /// </summary>
    private interface IServiceBusPublisher
    {
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Mock service bus for demonstration
    /// </summary>
    private sealed class MockServiceBusPublisher : IServiceBusPublisher
    {
        public List<string> PublishedMessages { get; } = [];

        public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var message = $"Published: {domainEvent?.GetType().Name} at {DateTime.UtcNow}";
            PublishedMessages.Add(message);
            return Task.CompletedTask;
        }
    }

    #endregion

    #region 6. Domain Event Handlers

    /// <summary>
    /// Handler for PersonCreatedEvent - sends notification and publishes to service bus
    /// </summary>
    private sealed class PersonCreatedEventHandler(IServiceBusPublisher serviceBusPublisher)
        : IDomainHandler<PersonCreatedEvent>
    {
        private readonly IServiceBusPublisher _serviceBusPublisher = serviceBusPublisher;

        public async Task HandleAsync(PersonCreatedEvent domainEvent)
        {
            // Business logic: Send welcome email, create audit log, etc.
            Console.WriteLine($"Person created: {domainEvent.Item.FirstName} {domainEvent.Item.LastName}");

            // Publish to service bus for integration with other systems
            await _serviceBusPublisher.PublishAsync(domainEvent);
        }
    }

    /// <summary>
    /// Handler for PersonVerifiedEvent
    /// </summary>
    private sealed class PersonVerifiedEventHandler(IServiceBusPublisher serviceBusPublisher)
        : IDomainHandler<PersonVerifiedEvent>
    {
        private readonly IServiceBusPublisher _serviceBusPublisher = serviceBusPublisher;

        public async Task HandleAsync(PersonVerifiedEvent domainEvent)
        {
            // Business logic: Grant access, send confirmation email, etc.
            Console.WriteLine($"Person verified: {domainEvent.Item.Email}");

            // Publish to service bus
            await _serviceBusPublisher.PublishAsync(domainEvent);
        }
    }

    /// <summary>
    /// Handler for PersonEmailChangedEvent
    /// </summary>
    private sealed class PersonEmailChangedEventHandler(IServiceBusPublisher serviceBusPublisher)
        : IDomainHandler<PersonEmailChangedEvent>
    {
        private readonly IServiceBusPublisher _serviceBusPublisher = serviceBusPublisher;

        public async Task HandleAsync(PersonEmailChangedEvent domainEvent)
        {
            // Business logic: Update email indexes, send verification email, etc.
            Console.WriteLine($"Email changed from {domainEvent.OldEmail} to {domainEvent.NewEmail}");

            // Publish to service bus
            await _serviceBusPublisher.PublishAsync(domainEvent);
        }
    }

    #endregion

    #region 7. Command Handlers

    /// <summary>
    /// Command handler for creating a person
    /// This is where the magic happens: Command -> Entity -> Events -> Dispatcher -> Service Bus
    /// </summary>
    private sealed class CreatePersonCommandHandler(
        IAppDbContext dbContext,
        IDomainDispatcher dispatcher)
    {
        private readonly IAppDbContext _dbContext = dbContext;
        private readonly IDomainDispatcher _dispatcher = dispatcher;

        public async Task<Guid> Handle(CreatePersonCommand command)
        {
            // 1. Create the domain entity (aggregate root)
            var person = new Person(
                Guid.NewGuid(),
                command.FirstName,
                command.LastName,
                command.Email,
                command.OwnerId,
                command.TenantId);

            // 2. Add to DbContext (repository pattern)
            _dbContext.Persons.Add(person);

            // 3. Save to database
            await _dbContext.SaveChangesAsync();

            // 4. Dispatch domain events (this triggers handlers which publish to service bus)
            await _dispatcher.DispatchAsync(person.DomainEvents);

            // 5. Clear events after dispatching
            person.ClearDomainEvents();

            return person.Id;
        }
    }

    /// <summary>
    /// Command handler for verifying a person (demonstrates multiple events)
    /// </summary>
    private sealed class VerifyPersonCommandHandler(
        IAppDbContext dbContext,
        IDomainDispatcher dispatcher)
    {
        private readonly IAppDbContext _dbContext = dbContext;
        private readonly IDomainDispatcher _dispatcher = dispatcher;

        public async Task Handle(VerifyPersonCommand command)
        {
            // 1. Load entity from repository
            var person = _dbContext.Persons.FirstOrDefault(p => p.Id == command.PersonId) ?? throw new InvalidOperationException($"Person {command.PersonId} not found");

            // 2. Execute domain logic (this adds PersonVerifiedEvent)
            person.Verify();

            // 3. Additional operations that add more events
            person.UpdateEmail(person.Email.ToLowerInvariant()); // This adds PersonEmailChangedEvent

            // 4. Save changes
            await _dbContext.SaveChangesAsync();

            // 5. Dispatch ALL accumulated events
            // Note: Person now has 2 events (PersonVerified + PersonEmailChanged)
            await _dispatcher.DispatchAsync(person.DomainEvents);

            // 6. Clear events
            person.ClearDomainEvents();
        }
    }

    #endregion

    #region 8. Integration Tests - Demonstrating the Full Flow

    [TestMethod]
    public async Task CreatePersonAddsEntityToDbContextAndDispatchesEventToServiceBus()
    {
        // Arrange - Setup infrastructure
        var dbContext = new InMemoryDbContext();
        var serviceBus = new MockServiceBusPublisher();

        // Setup DI container manually (in real app, use Microsoft.Extensions.DependencyInjection)
        var services = new ServiceCollection();
        services.AddSingleton<IServiceBusPublisher>(serviceBus);
        services.AddTransient<IDomainHandler<PersonCreatedEvent>>(sp =>
        {
            var svc = ((ServiceCollection)sp).GetRequiredService<IServiceBusPublisher>();
            return new PersonCreatedEventHandler(svc);
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new DomainDispatcher(serviceProvider);

        var handler = new CreatePersonCommandHandler(dbContext, dispatcher);

        var command = new CreatePersonCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            Guid.NewGuid(), // OwnerId
            Guid.NewGuid()  // TenantId
        );

        // Act - Execute command
        var personId = await handler.Handle(command);

        // Assert - Verify entity was created
        Assert.HasCount(1, dbContext.Persons);
        var person = dbContext.Persons.First();
        Assert.AreEqual("John", person.FirstName);
        Assert.AreEqual("Doe", person.LastName);
        Assert.AreEqual("john.doe@example.com", person.Email);
        Assert.IsFalse(person.IsVerified);
        Assert.AreNotEqual(Guid.Empty, person.OwnerId);
        Assert.AreNotEqual(Guid.Empty, person.TenantId);

        // Assert - Verify event was published to service bus
        Assert.HasCount(1, serviceBus.PublishedMessages);
        Assert.Contains("PersonCreatedEvent", serviceBus.PublishedMessages[0]);
    }

    [TestMethod]
    public async Task VerifyPersonDispatchesMultipleEventsToServiceBus()
    {
        // Arrange - Create person first
        var dbContext = new InMemoryDbContext();
        var serviceBus = new MockServiceBusPublisher();

        var person = new Person(
            Guid.NewGuid(),
            "Jane",
            "Smith",
            "Jane.Smith@Example.com", // Mixed case to test email change
            Guid.NewGuid(),
            Guid.NewGuid());

        dbContext.Persons.Add(person);
        person.ClearDomainEvents(); // Clear creation event

        // Setup DI for multiple handlers
        var services = new ServiceCollection();
        services.AddSingleton<IServiceBusPublisher>(serviceBus);
        services.AddTransient<IDomainHandler<PersonVerifiedEvent>>(sp =>
        {
            var svc = ((ServiceCollection)sp).GetRequiredService<IServiceBusPublisher>();
            return new PersonVerifiedEventHandler(svc);
        });
        services.AddTransient<IDomainHandler<PersonEmailChangedEvent>>(sp =>
        {
            var svc = ((ServiceCollection)sp).GetRequiredService<IServiceBusPublisher>();
            return new PersonEmailChangedEventHandler(svc);
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new DomainDispatcher(serviceProvider);

        var handler = new VerifyPersonCommandHandler(dbContext, dispatcher);
        var command = new VerifyPersonCommand(person.Id);

        // Act - Execute command that triggers multiple events
        await handler.Handle(command);

        // Assert - Verify person was updated
        Assert.IsTrue(person.IsVerified);
        Assert.AreEqual("jane.smith@example.com", person.Email); // Email normalized

        // Assert - Verify BOTH events were published to service bus
        Assert.HasCount(2, serviceBus.PublishedMessages);
        Assert.Contains(m => m.Contains("PersonVerifiedEvent"), serviceBus.PublishedMessages);
        Assert.Contains(m => m.Contains("PersonEmailChangedEvent"), serviceBus.PublishedMessages);
    }

    [TestMethod]
    public async Task CommandHandlerWithServiceBusFailureShouldHandleGracefully()
    {
        // Arrange - Setup failing service bus
        var dbContext = new InMemoryDbContext();
        var failingServiceBus = new FailingServiceBusPublisher();

        var services = new ServiceCollection();
        services.AddSingleton<IServiceBusPublisher>(failingServiceBus);
        services.AddTransient<IDomainHandler<PersonCreatedEvent>>(sp =>
        {
            var svc = ((ServiceCollection)sp).GetRequiredService<IServiceBusPublisher>();
            return new PersonCreatedEventHandler(svc);
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new DomainDispatcher(serviceProvider);
        var handler = new CreatePersonCommandHandler(dbContext, dispatcher);

        var command = new CreatePersonCommand(
            "Test",
            "User",
            "test@example.com",
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act & Assert - Should throw exception when service bus fails
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command));

        // Note: In production, you'd use patterns like:
        // - Outbox pattern (store events in DB, publish later)
        // - Circuit breaker (fail gracefully)
        // - Retry policies (Polly)
    }

    /// <summary>
    /// Mock service bus that fails to demonstrate error handling
    /// </summary>
    private sealed class FailingServiceBusPublisher : IServiceBusPublisher
    {
        public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Service bus unavailable");
        }
    }

    #endregion

    #region 9. Helper for DI (simplified ServiceCollection)

    private sealed class ServiceCollection : List<ServiceDescriptor>, IServiceProvider
    {
        public void AddSingleton<TService>(TService implementation) where TService : class
        {
            Add(new ServiceDescriptor(typeof(TService), implementation));
        }

        public void AddTransient<TService>(Func<IServiceProvider, TService> factory)
        {
            Add(new ServiceDescriptor(typeof(TService), sp => factory(sp)!));
        }

        public ServiceCollection BuildServiceProvider() => this;

        public object? GetService(Type serviceType)
        {
            var descriptor = this.FirstOrDefault(d => d.ServiceType == serviceType);
            if (descriptor == null) return null;

            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(this);

            return null;
        }

        public TService GetRequiredService<TService>() where TService : class
        {
            var service = GetService(typeof(TService)) as TService;
            return service ?? throw new InvalidOperationException($"Service {typeof(TService).Name} not registered");
        }
    }

    private sealed class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public object? ImplementationInstance { get; }
        public Func<IServiceProvider, object>? ImplementationFactory { get; }

        public ServiceDescriptor(Type serviceType, object implementationInstance)
        {
            ServiceType = serviceType;
            ImplementationInstance = implementationInstance;
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
        }
    }

    #endregion
}

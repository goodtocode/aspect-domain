# Goodtocode.Domain

Domain-Driven Design (DDD) base library for .NET Standard 2.1 and modern .NET projects.

[![NuGet CI/CD](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml)

Goodtocode.Domain provides foundational types for building DDD, clean architecture, and event-driven systems. It includes base classes for domain entities, audit fields, domain events, and secured/multi-tenant entities. The library is lightweight, dependency-free, and designed to work with EF Core, Cosmos DB, or custom repositories.

## Target Frameworks
- Library: `netstandard2.1`
- Tests/examples: `net10.0`

## Features
- Domain entity base with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `CreatedBy`, `ModifiedBy`, `DeletedBy`, `Timestamp`)
- Domain event pattern and dispatcher (`IDomainEvent`, `IDomainHandler`, `DomainDispatcher`)
- Equality and identity management for aggregate roots
- Partition key support for document stores (`PartitionKey`)
- Secured entity base for multi-tenancy and ownership (`OwnerId`, `TenantId`)
- Extension methods for authorization and ownership queries

## Install
```bash
dotnet add package Goodtocode.Domain
```

## Quick-Start (Repo)
1. Clone this repository
   ```
   git clone https://github.com/goodtocode/aspect-domain.git
   ```
2. Build the solution
   ```
   cd src
   dotnet build Goodtocode.Domain.sln
   ```
3. Run tests
   ```
   cd Goodtocode.Domain.Tests
   dotnet test
   ```

## Core Concepts
- `DomainEntity<TModel>`: Base entity with audit fields, identity, and domain event tracking.
- `SecuredEntity<TModel>`: Adds `OwnerId` and `TenantId`, with `PartitionKey` defaulting to `TenantId.ToString()`.
- Domain events: Implement `IDomainEvent<TModel>` and dispatch with `DomainDispatcher`.

## Key Examples

### 1. Basic Domain Entity with Audit Fields
```csharp
using Goodtocode.Domain.Entities;

public sealed class MyEntity : DomainEntity<MyEntity>
{
    public string Name { get; private set; } = string.Empty;
    public int Value { get; private set; }

    public static MyEntity Create(Guid id, string name, int value, Guid createdBy)
    {
        var entity = new MyEntity
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            Name = name,
            Value = value
        };

        entity.SetCreatedOn(DateTime.UtcNow);
        entity.SetCreatedBy(createdBy);

        return entity;
    }
}
```

### 2. Secured Entity with Multi-Tenant Ownership
```csharp
using Goodtocode.Domain.Entities;

public sealed class Document : SecuredEntity<Document>
{
    public string Title { get; private set; } = string.Empty;

    private Document() { }

    public Document(Guid id, Guid ownerId, Guid tenantId, string title)
        : base(id, ownerId, tenantId)
    {
        Title = title;
        SetCreatedOn(DateTime.UtcNow);
        SetCreatedBy(ownerId);
    }
}

// Query helpers
var ownedDocuments = queryableDocuments.WhereOwner(ownerId);
var tenantDocuments = queryableDocuments.WhereTenant(tenantId);
var authorized = queryableDocuments.WhereAuthorized(tenantId, ownerId);
```

### 3. Domain Events + Dispatcher
```csharp
using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

public sealed class Person : SecuredEntity<Person>
{
    public string Email { get; private set; } = string.Empty;

    public Person(Guid id, Guid ownerId, Guid tenantId, string email)
        : base(id, ownerId, tenantId)
    {
        Email = email;
        SetCreatedOn(DateTime.UtcNow);
        AddDomainEvent(new PersonCreatedEvent(this));
    }
}

public sealed class PersonCreatedEvent : IDomainEvent<Person>
{
    public Person Item { get; }
    public DateTime OccurredOn { get; }

    public PersonCreatedEvent(Person person)
    {
        Item = person;
        OccurredOn = DateTime.UtcNow;
    }
}

public sealed class PersonCreatedHandler : IDomainHandler<PersonCreatedEvent>
{
    public Task HandleAsync(PersonCreatedEvent domainEvent)
    {
        Console.WriteLine($"Created: {domainEvent.Item.Email}");
        return Task.CompletedTask;
    }
}

// Dispatcher usage (with your DI container)
var serviceProvider = new ServiceCollection()
    .AddTransient<IDomainHandler<PersonCreatedEvent>, PersonCreatedHandler>()
    .BuildServiceProvider();

var dispatcher = new DomainDispatcher(serviceProvider);
await dispatcher.DispatchAsync(person.DomainEvents);
person.ClearDomainEvents();
```

## Complete Examples
See the fully working examples in the test project:
- `Goodtocode.Domain.Tests/Examples/RowLevelSecurityExample.cs` (row-level security, audit fields, and partition key usage)
- `Goodtocode.Domain.Tests/Examples/CommandHandlerWithEventsExample.cs` (command handlers, domain events, dispatcher, and service bus integration)

## Technologies
- [C# .NET](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/)

## Version History

| Version | Date        | Release Notes     |
|---------|-------------|-------------------|
| 1.0.0   | 2026-Jan-19 | Initial release   |

## License

This project is licensed with the [MIT license](https://mit-license.org/).

## Contact
- [GitHub Repo](https://github.com/goodtocode)
- [@goodtocode](https://twitter.com/goodtocode)

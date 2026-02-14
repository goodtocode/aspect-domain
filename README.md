# Goodtocode.Domain

Domain-Driven Design (DDD) base library for .NET Standard 2.1 and modern .NET projects.

[![NuGet CI/CD](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml)

Goodtocode.Domain provides foundational types for building DDD, clean architecture, and event-driven systems. It includes base classes for domain entities, audit fields, domain events, and secured/multi-tenant entities. The library is lightweight, dependency-free, and designed to work with EF Core, Cosmos DB, or custom repositories.

## Target Frameworks
- Library: `netstandard2.1`
- Tests/examples: `net10.0`

## Features
- Domain entity base with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `Timestamp`)
- Domain event pattern and dispatcher (`IDomainEvent`, `IDomainHandler`, `DomainDispatcher`)
- Equality and identity management for aggregate roots
- Partition key support for document stores (`PartitionKey` defaults to `Id.ToString()`; for secured entities, defaults to `TenantId.ToString()`)
- Secured entity base for multi-tenancy and ownership (`OwnerId`, `TenantId`, `CreatedBy`, `ModifiedBy`, `DeletedBy`)
- Extension methods for authorization and ownership queries
- **Invariant state protection** for audit and security fields (fields are only set if not already set, ensuring consistency and preventing accidental overwrites)

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
- `DomainEntity<TModel>`: Base entity with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `Timestamp`), identity (`Id`), partition key, and domain event tracking.
- `SecuredEntity<TModel>`: Extends `DomainEntity<TModel>` with `OwnerId`, `TenantId`, and audit fields for user actions (`CreatedBy`, `ModifiedBy`, `DeletedBy`). `PartitionKey` defaults to `TenantId.ToString()` for multi-tenant isolation.
- **Invariant state protection**: Methods like `MarkCreated`, `MarkDeleted`, etc. only set fields if not already set, ensuring entity state is consistent and protected from accidental changes.
- Domain events: Implement `IDomainEvent<TModel>` and dispatch with `DomainDispatcher`.

## Key Examples

### 1. Basic Domain Entity with Audit Fields
```csharp
using Goodtocode.Domain.Entities;

public sealed class MyEntity : DomainEntity<MyEntity>
{
    public string Name { get; private set; } = string.Empty;
    public int Value { get; private set; }

    private MyEntity() { }

    public MyEntity(Guid id, string name, int value) : base(id)
    {
        Name = name;
        Value = value;
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

    public Document(Guid id, Guid ownerId, Guid tenantId, string title) : base(id, ownerId, tenantId)
    {
        Title = title;
    }
}

// Query helpers
var ownedDocuments = queryableDocuments.WhereOwner(ownerId);
var tenantDocuments = queryableDocuments.WhereTenant(tenantId);
var authorized = queryableDocuments.WhereAuthorized(tenantId, ownerId);
```

person.ClearDomainEvents();
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
var serviceProvider = new ServiceCollection();
serviceProvider.AddTransient<IDomainHandler<PersonCreatedEvent>, PersonCreatedHandler>();
serviceProvider.BuildServiceProvider();

var dispatcher = new DomainDispatcher(serviceProvider);
await dispatcher.DispatchAsync(person.DomainEvents);
person.ClearDomainEvents();
```


---

## Integrating with EF Core: Audit & Security Field Automation

To ensure audit and security fields are set correctly and invariant state is protected, you must wire up your `DbContext` to set these fields during the entity lifecycle.

**Example:**
```csharp
public class ExampleDbContext : DbContext 
{ 
    private readonly ICurrentUserContext _currentUserContext;

    public ExampleDbContext(DbContextOptions options, ICurrentUserContext currentUserContext)
        : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        SetSecurityFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Modified)
                {
                    auditable.MarkModified();
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditable.MarkDeleted();
                    entry.State = EntityState.Modified;
                }
            }
        }
    }

    private void SetSecurityFields()
    {
        if (_currentUserContext is null) return;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is ISecurable securable)
            {
                if (entry.State == EntityState.Added)
                {
                    if (securable.OwnerId == Guid.Empty)
                        securable.ChangeOwner(_currentUserContext.OwnerId);
                    if (securable.TenantId == Guid.Empty)
                        securable.ChangeTenant(_currentUserContext.TenantId);
                }
            }
        }
    }
}
```

**Note:**  
- This pattern should be implemented in your infrastructure layer (not in the domain library).
- See `Goodtocode.Domain.Tests/Examples/ExampleDbContext.cs` for a working reference

---

## Complete Examples
See the fully working examples in the test project:
- `Goodtocode.Domain.Tests/Examples/RowLevelSecurityExample.cs` (row-level security, audit fields, and partition key usage)
- `Goodtocode.Domain.Tests/Examples/CommandHandlerWithEventsExample.cs` (command handlers, domain events, dispatcher, and service bus integration)
- `Goodtocode.Domain.Tests/Examples/ExampleDbContext.cs` (**EF Core integration for audit and security fields**)

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

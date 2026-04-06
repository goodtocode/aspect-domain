# Goodtocode.Domain

Domain-Driven Design (DDD) base library for .NET Standard 2.1 and modern .NET projects.

[![NuGet CI/CD](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml)

Goodtocode.Domain provides foundational types for building DDD, clean architecture, and event-driven systems. It includes base classes for domain entities, audit fields, domain events, secured/multi-tenant entities, and immutable versioned entities with full lifecycle management. The library is lightweight, dependency-free, and designed to work with EF Core, Cosmos DB, Table Storage, or custom repositories.

## Target Frameworks
- Library: `netstandard2.1`
- Tests/examples: `net10.0`

## Features
- Domain entity base with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `Timestamp`)
- Domain event pattern and dispatcher (`IDomainEvent`, `IDomainHandler`, `DomainDispatcher`)
- Equality and identity management for aggregate roots
- UUIDv7-based `RowKey` for time-ordered, chronologically sortable storage keys
- Partition key and row key support for document/table stores (`PartitionKey` defaults to `Id.ToString()`; `RowKey` is always a new UUIDv7 — distinct from `Id`)
- Secured entity base for multi-tenancy and ownership (`OwnerId`, `TenantId`, `CreatedBy`, `ModifiedBy`, `DeletedBy`)
- Extension methods for authorization and ownership queries
- **Invariant state protection** for audit and security fields (fields are only set if not already set, ensuring consistency and preventing accidental overwrites)
- **Immutable versioned entities** via `SecuredVersionedEntity<TModel>`: all state changes produce new rows; no in-place mutation after persistence
- Full versioning lifecycle: `CreateNextVersion()`, `CreateSuccessor()`, `Freeze()`, `MarkNotLatest()`
- Abstract factory pattern (`CreateNextVersionCore` / `CreateSuccessorCore`) keeps derived classes in control of construction

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
- `DomainEntity<TModel>`: Base entity with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `Timestamp`), identity (`Id`), partition key, row key, and domain event tracking.
- `RowKey` is always a new **UUIDv7** (time-ordered, RFC 4122). It is intentionally distinct from `Id`. `PartitionKey` defaults to `Id.ToString()` unless explicitly set. This supports portability across Cosmos DB, Table Storage, and other stores.
- `SecuredEntity<TModel>`: Extends `DomainEntity<TModel>` with `OwnerId`, `TenantId`, and audit fields for user actions (`CreatedBy`, `ModifiedBy`, `DeletedBy`). `PartitionKey` defaults to `TenantId.ToString()` for multi-tenant isolation.
- **Invariant state protection**: Methods like `MarkCreated`, `MarkDeleted`, etc. only set fields if not already set, ensuring entity state is consistent and protected from accidental changes.
- `SecuredVersionedEntity<TModel>`: Extends `SecuredEntity<TModel>` and implements `IVersionable`. All persisted rows are **immutable** — state changes always produce new rows. `PartitionKey` is `TenantId:CanonicalKey`, grouping all versions of a logical entity in the same partition.
- `IVersionable`: Read-only state contract — `CanonicalKey`, `Version`, `PreviousVersionId`, `IsLatest`, `IsPinned`, `IsFrozen`. No mutation methods on the interface.
- Domain events: Implement `IDomainEvent<TModel>` and dispatch with `DomainDispatcher`.

## Versioning Lifecycle & Invariants

`SecuredVersionedEntity<TModel>` enforces the following invariants:

| Rule | Detail |
|------|--------|
| Rows are immutable | Once persisted, a row's fields never change (except `IsLatest` and `IsFrozen` via `MarkNotLatest`/`Freeze`) |
| New version = new row | `CreateNextVersion()` returns a new row with a new `Id`, new UUIDv7 `RowKey`, incremented `Version`, and `PreviousVersionId` pointing to the current row |
| `IsLatest` is caller-managed | The caller must call `MarkNotLatest()` on the previous row and persist both rows **transactionally** |
| Frozen series cannot version | `CreateNextVersion()` throws `InvalidOperationException` when `IsFrozen = true` |
| Successors start fresh | `CreateSuccessor(newCanonicalKey)` starts a new series: `Version = 1`, `PreviousVersionId = null`, same `TenantId`/`OwnerId` |
| Successors are always allowed | `CreateSuccessor()` is permitted even on a frozen series |
| Derived classes own construction | `CreateNextVersionCore()` and `CreateSuccessorCore()` are `abstract` — the concrete class supplies the new instance |

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

// Example: Customizing PartitionKey and RowKey
```csharp
public sealed class TableEntity : DomainEntity<TableEntity>
{
    public TableEntity(Guid id, string partitionKey, string rowKey) : base(id, partitionKey, rowKey) { }
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

### 4. Versioned Entity Lifecycle (Immutable Pattern)

`SecuredVersionedEntity<TModel>` uses the **Template Method** pattern. Your derived class provides `CreateNextVersionCore()` and `CreateSuccessorCore()`; the base class enforces all invariants.

```csharp
using Goodtocode.Domain.Entities;

public sealed class Invoice : SecuredVersionedEntity<Invoice>
{
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = string.Empty;

    // Required by ORM / serialization
    private Invoice() { }

    public Invoice(
        Guid id, string canonicalKey, Guid ownerId, Guid tenantId, Guid createdBy,
        DateTime createdOn, DateTimeOffset timestamp,
        int version, Guid? previousVersionId, bool isLatest, bool isPinned, bool isFrozen,
        decimal amount, string status)
        : base(id, canonicalKey, null, ownerId, tenantId, createdBy,
               createdOn, timestamp, version, previousVersionId, isLatest, isPinned, isFrozen)
    {
        Amount = amount;
        Status = status;
    }

    protected override Invoice CreateNextVersionCore() =>
        new(Guid.NewGuid(), CanonicalKey, OwnerId, TenantId, CreatedBy,
            DateTime.UtcNow, DateTimeOffset.UtcNow,
            Version + 1, Id, isLatest: true, isPinned: false, isFrozen: false,
            Amount, Status);

    protected override Invoice CreateSuccessorCore(string newCanonicalKey) =>
        new(Guid.NewGuid(), newCanonicalKey, OwnerId, TenantId, CreatedBy,
            DateTime.UtcNow, DateTimeOffset.UtcNow,
            1, null, isLatest: true, isPinned: false, isFrozen: false,
            Amount, Status);
}
```

```csharp
// --- Create version 1 ---
var v1 = new Invoice(Guid.NewGuid(), "INV-2026-001", ownerId, tenantId, createdBy,
    DateTime.UtcNow, DateTimeOffset.UtcNow,
    1, null, isLatest: true, isPinned: false, isFrozen: false,
    amount: 100m, status: "Draft");

// --- Produce version 2 (new row, v1 stays unchanged) ---
var v2 = v1.CreateNextVersion();
// Transactionally: persist v2, then mark v1 as no longer latest
v1.MarkNotLatest();
// persist both: v1 (IsLatest=false) and v2 (IsLatest=true)

// --- Freeze the series (no more versions allowed) ---
v2.Freeze();

// --- Successor starts a new canonical key series ---
var successor = v2.CreateSuccessor("INV-2027-001");
// successor: Version=1, PreviousVersionId=null, CanonicalKey="INV-2027-001"
v2.MarkNotLatest();
// persist both: v2 (IsLatest=false) and successor (IsLatest=true)
```

**Key invariants to remember:**
- `RowKey` on every row is a new UUIDv7 — never equal to `Id`
- `PartitionKey` = `TenantId:CanonicalKey` — all versions of the same entity land in the same partition
- `IsLatest` flip is **your responsibility** — call `MarkNotLatest()` on the outgoing row inside your transaction
- `CreateNextVersion()` throws if `IsFrozen = true`
- `CreateSuccessor()` always succeeds, even on a frozen series

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
- `Goodtocode.Domain.Tests/Examples/RowLevelSecurityExample.cs` (row-level security, audit fields, and partition/row key usage)
- `Goodtocode.Domain.Tests/Examples/CommandHandlerWithEventsExample.cs` (command handlers, domain events, dispatcher, and service bus integration)
- `Goodtocode.Domain.Tests/Examples/ExampleDbContext.cs` (**EF Core integration for audit and security fields**)

## Technologies
- [C# .NET](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/)

## Version History

| Version | Date        | Release Notes                                        |
|---------|-------------|------------------------------------------------------|
| 1.0.0   | 2026-Jan-19 | Initial release                                      |
| 1.2.0   | 2026-Mar-14 | Added rowKey support                                 |
| 1.3.0   | 2026-Mar-18 | Versioning, pinning, freezing                        |
| 1.4.0   | 2026-Apr-05 | Versioning lifecycle and invariants                  |
| 1.5.0   | 2026-Apr-19 | Immutable versioning: UUID7 RowKey, CanonicalKey, SecuredVersionedEntity lifecycle (CreateNextVersion, CreateSuccessor, Freeze, MarkNotLatest), abstract factory pattern, IVersionable read-only contract |

## License

This project is licensed with the [MIT license](https://mit-license.org/).

## Contact
- [GitHub Repo](https://github.com/goodtocode)
- [@goodtocode](https://twitter.com/goodtocode)

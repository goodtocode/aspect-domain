

# Goodtocode.Domain

Domain-Driven Design (DDD) base library for .NET Standard 2.0+ and .NET

[![NuGet CI/CD](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-domain/actions/workflows/gtc-domain-nuget.yml)

Goodtocode.Domain provides foundational types for building DDD, clean architecture, and event-driven systems. It includes base classes for domain entities, audit fields, domain events, and secured/multi-tenant entities. The library is designed for extensibility and can be integrated into any .NET Standard 2.0+ or .NET project.

## Features
- Domain entity base with audit fields (`CreatedOn`, `ModifiedOn`, `DeletedOn`, `Timestamp`)
- Domain event pattern for eventual consistency and cross-context communication
- Equality and identity management for aggregate roots
- Secured entity base for multi-tenancy and ownership (`OwnerId`, `TenantId`)
- Extension methods for authorization and ownership queries
- Lightweight, dependency-free, and compatible with .NET Standard 2.0+ and .NET
- Designed for use with EF Core, CosmosDb, custom repositories, and APIs

## Quick-Start Steps
1. Clone this repository
   ```
   git clone https://github.com/goodtocode/aspect-domain.git
   ```
2. Install .NET SDK (latest recommended)
   ```
   winget install Microsoft.DotNet.SDK --silent
   ```
3. Build the solution
   ```
   cd src
   dotnet build Goodtocode.Domain.sln
   ```
4. Run tests
   ```
   cd Goodtocode.Domain.Tests
   dotnet test
   ```

## Install Prerequisites
- [.NET SDK (latest)](https://dotnet.microsoft.com/en-us/download)
- Visual Studio (latest) or VS Code

## Top Use Case Examples

### 1. Basic Domain Entity with Audit Fields
```csharp
using Goodtocode.Domain.DomainEntity;

public class MyEntity : DomainEntity<MyEntity>
{
    public string Name { get; private set; } = string.Empty;
    public int Value { get; private set; }

    public static MyEntity Create(Guid id, string name, int value)
    {
        return new MyEntity
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            Name = name,
            Value = value,
            CreatedOn = DateTime.UtcNow
        };
    }
}
```

### 2. Secured Entity for Multi-Tenant and Ownership Scenarios
```csharp
using Goodtocode.Domain.SecuredEntity;

public class DigitalAgentEntity : SecuredEntity<DigitalAgentEntity>
{
    protected DigitalAgentEntity() { }

    public Guid DigitalAssetId { get; private set; }
    public string? Name { get; private set; } = string.Empty;
    public string? Description { get; private set; } = string.Empty;
    public AgentStatus Status { get; private set; } = AgentStatus.Inactive;
    public ICollection<string> Tags { get; private set; } = [];
    public virtual Guid AgentPersonaId { get; private set; } = Personas.Monitor.Id;

    public static DigitalAgentEntity Create(Guid id, Guid digitalAssetId, Guid agentPersonaId, Guid tenantId, Guid ownerId, AgentStatus status, string? name = null, string? description = null)
    {
        return new DigitalAgentEntity()
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            DigitalAssetId = digitalAssetId,
            AgentPersonaId = agentPersonaId,
            Status = status,
            Name = name,
            Description = description,
            TenantId = tenantId,
            OwnerId = ownerId
        };
    }

    public void Activate() => Status = AgentStatus.Active;
    public void Deactivate() => Status = AgentStatus.Inactive;
    public void Update(string? name, string? description)
    {
        Name = name;
        Description = description;
    }
}
```

### 3. Domain Events for Eventual Consistency
```csharp
using Goodtocode.Domain.DomainEvent;

public class MyCreatedEvent : IDomainEvent<MyEntity>
{
    public MyEntity Entity { get; }
    public MyCreatedEvent(MyEntity entity) => Entity = entity;
}

// Usage in entity
var entity = MyEntity.Create(Guid.NewGuid(), "Test", 42);
entity.AddDomainEvent(new MyCreatedEvent(entity));
```

### 4. Secured Entity Authorization Extensions
```csharp
using Goodtocode.Domain.SecuredEntity;

// Query for entities owned by a user
var ownedAgents = dbContext.Agents.IsOwner(userId);

// Query for entities authorized for a tenant or owner
var authorizedAgents = dbContext.Agents.WhereAuthorized(tenantId, userId);
```

## Technologies
- [C# .NET](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/)

## Version History

| Version | Date        | Release Notes                                    |
|---------|-------------|--------------------------------------------------|
 | 1.1.0   | 2026-Jan-20 | Version bump, CI/CD improvements, props and targets updates |
 | 1.0.0   | 2026-Jan-19 | Initial release                                  |

## License

This project is licensed with the [MIT license](https://mit-license.org/).

## Contact
- [GitHub Repo](https://github.com/goodtocode/aspect-domain)
- [@goodtocode](https://twitter.com/goodtocode)

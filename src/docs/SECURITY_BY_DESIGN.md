# Security-by-Design: Audit Fields and Row-Level Security (RLS)

## Overview

This implementation adds bare minimum security-by-design features to the domain layer:
- **Audit Fields**: `CreatedBy`, `ModifiedBy`, `DeletedBy` - track WHO performed operations
- **TenantId Projection**: `PartitionKey` defaults to `TenantId` for multi-tenant data isolation
- **RLS Support**: Enable row-level security at application and database levels

## Features Implemented

### 1. Audit Fields in `DomainEntity`

Three new fields track user identifiers for all operations:

```csharp
public Guid CreatedBy { get; protected set; }
public Guid? ModifiedBy { get; protected set; }
public Guid? DeletedBy { get; protected set; }
```

**Setter Methods:**
```csharp
entity.SetCreatedBy(userId);
entity.SetModifiedBy(userId);
entity.SetDeletedBy(userId);
```

### 2. PartitionKey Projection in `SecuredEntity`

The `PartitionKey` now automatically returns `TenantId.ToString()`:

```csharp
public new string PartitionKey => TenantId.ToString();
```

**Benefits:**
- Automatic data partitioning by tenant in Cosmos DB
- Improved query performance (queries within partition)
- Physical data isolation for multi-tenant scenarios

### 3. Complete Audit Trail

Track the complete lifecycle of entities:

```csharp
// Create
entity.SetCreatedOn(DateTime.UtcNow);
entity.SetCreatedBy(currentUserId);

// Modify
entity.SetModifiedOn(DateTime.UtcNow);
entity.SetModifiedBy(currentUserId);

// Delete (Soft Delete)
entity.SetDeletedOn(DateTime.UtcNow);
entity.SetDeletedBy(currentUserId);
```

## Usage Examples

### Basic Entity Creation with Audit

```csharp
public class CreatePersonHandler
{
    public async Task<Guid> Handle(CreatePersonCommand command, Guid currentUserId)
    {
        var person = new Person(
            Guid.NewGuid(),
            currentUserId,  // OwnerId
            command.TenantId,
            command.FirstName,
            command.LastName,
            command.Email);

        // Set audit fields
        person.SetCreatedOn(DateTime.UtcNow);
        person.SetCreatedBy(currentUserId);

        await _dbContext.Persons.AddAsync(person);
        await _dbContext.SaveChangesAsync();

        return person.Id;
    }
}
```

### Soft Delete with Audit

```csharp
public async Task Delete(Guid personId, Guid currentUserId)
{
    var person = await _dbContext.Persons.FindAsync(personId);
    
    person.SetDeletedOn(DateTime.UtcNow);
    person.SetDeletedBy(currentUserId);
    
    await _dbContext.SaveChangesAsync();
}
```

## EF Core Configuration

### Global Query Filters (RLS)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Tenant isolation + soft delete filter
    modelBuilder.Entity<Person>().HasQueryFilter(p => 
        p.TenantId == _currentTenantId && 
        p.DeletedOn == null);

    // Performance indexes
    modelBuilder.Entity<Person>()
        .HasIndex(p => new { p.TenantId, p.OwnerId })
        .HasDatabaseName("IX_Person_TenantId_OwnerId");

    modelBuilder.Entity<Person>()
        .HasIndex(p => p.PartitionKey)
        .HasDatabaseName("IX_Person_PartitionKey");
}
```

### Setting Current User Context

```csharp
public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid _currentTenantId;
    private Guid _currentUserId;

    public AppDbContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        
        // Extract from JWT claims
        var user = _httpContextAccessor.HttpContext?.User;
        _currentTenantId = Guid.Parse(user?.FindFirst("tid")?.Value ?? Guid.Empty.ToString());
        _currentUserId = Guid.Parse(user?.FindFirst("oid")?.Value ?? Guid.Empty.ToString());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set audit fields on save
        foreach (var entry in ChangeTracker.Entries<DomainEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedOn(DateTime.UtcNow);
                    entry.Entity.SetCreatedBy(_currentUserId);
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.SetModifiedOn(DateTime.UtcNow);
                    entry.Entity.SetModifiedBy(_currentUserId);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

## SQL Server RLS (Database-Level Security)

For defense-in-depth, implement SQL Server RLS:

```sql
-- Create security function
CREATE FUNCTION Security.fn_TenantSecurityPredicate(@TenantId uniqueidentifier)
    RETURNS TABLE
WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS fn_securitypredicate_result
    WHERE 
        @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)
        OR IS_MEMBER('db_owner') = 1;  -- Allow admins
GO

-- Create security policy
CREATE SECURITY POLICY Security.TenantSecurityPolicy
ADD FILTER PREDICATE Security.fn_TenantSecurityPredicate(TenantId)
ON dbo.Persons,
ADD BLOCK PREDICATE Security.fn_TenantSecurityPredicate(TenantId)
ON dbo.Persons
WITH (STATE = ON);
GO

-- Set session context before queries
EXEC sp_set_session_context 'TenantId', @currentTenantId;
```

## Cosmos DB Configuration

For Cosmos DB, use `PartitionKey`:

```csharp
public class CosmosDbContext
{
    private readonly Container _container;

    public async Task<Person> GetByIdAsync(Guid id, Guid tenantId)
    {
        // Use PartitionKey for efficient queries
        var response = await _container.ReadItemAsync<Person>(
            id.ToString(),
            new PartitionKey(tenantId.ToString()));
            
        return response.Resource;
    }

    public async Task<IEnumerable<Person>> GetByTenantAsync(Guid tenantId)
    {
        // Query within partition
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.deletedOn = null")
            .WithParameter("@tenantId", tenantId);

        var iterator = _container.GetItemQueryIterator<Person>(
            query,
            requestOptions: new QueryRequestOptions 
            { 
                PartitionKey = new PartitionKey(tenantId.ToString()) 
            });

        // ... execute query
    }
}
```

## Compliance & GDPR

The audit fields support compliance requirements:

### GDPR Article 15 - Right to Access

```csharp
public class PersonAuditReport
{
    public Guid PersonId { get; set; }
    public Guid DataSubject { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }
}

public async Task<PersonAuditReport> GenerateAuditReport(Guid personId)
{
    var person = await _dbContext.Persons
        .IgnoreQueryFilters()  // Bypass RLS for audit
        .FirstOrDefaultAsync(p => p.Id == personId);

    return new PersonAuditReport
    {
        PersonId = person.Id,
        DataSubject = person.OwnerId,
        CreatedOn = person.CreatedOn,
        CreatedBy = person.CreatedBy,
        ModifiedOn = person.ModifiedOn,
        ModifiedBy = person.ModifiedBy,
        DeletedOn = person.DeletedOn,
        DeletedBy = person.DeletedBy
    };
}
```

### SOX Compliance - Change Tracking

```csharp
// Track all changes for audit
public class PersonChangeLog
{
    public Guid PersonId { get; set; }
    public string Action { get; set; }  // CREATE, UPDATE, DELETE
    public Guid PerformedBy { get; set; }
    public DateTime PerformedOn { get; set; }
    public string Changes { get; set; }  // JSON of changes
}

public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var changedEntries = ChangeTracker.Entries<Person>()
        .Where(e => e.State != EntityState.Unchanged)
        .ToList();

    foreach (var entry in changedEntries)
    {
        var changeLog = new PersonChangeLog
        {
            PersonId = entry.Entity.Id,
            Action = entry.State.ToString(),
            PerformedBy = _currentUserId,
            PerformedOn = DateTime.UtcNow,
            Changes = JsonSerializer.Serialize(entry.CurrentValues.Properties
                .ToDictionary(p => p.Name, p => entry.CurrentValues[p]))
        };

        await _auditLog.AddAsync(changeLog);
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

## Testing

Comprehensive tests are provided:

- **SecuredEntityAuditTests.cs**: Unit tests for audit fields and PartitionKey
- **RowLevelSecurityExample.cs**: Integration tests demonstrating RLS patterns

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~SecuredEntityAuditTests"
dotnet test --filter "FullyQualifiedName~RowLevelSecurityExample"
```

## Security Best Practices

### ? DO

1. **Always set audit fields** when creating/modifying entities
2. **Use query filters** for tenant isolation in EF Core
3. **Extract user context from JWT claims**, never from client input
4. **Use PartitionKey** for Cosmos DB queries to improve performance
5. **Implement soft delete** with `DeletedOn` / `DeletedBy` fields
6. **Log security events** (failed access attempts, privilege escalation)
7. **Test RLS thoroughly** with different user/tenant combinations

### ? DON'T

1. **Don't trust client-provided TenantId** - always extract from authenticated context
2. **Don't bypass query filters** except for admin/audit scenarios
3. **Don't hard-delete data** - use soft delete for audit trail
4. **Don't expose DeletedOn entities** in regular queries
5. **Don't forget to set CreatedBy** on new entities
6. **Don't allow cross-tenant data access** without explicit authorization

## Migration Guide

### Updating Existing Entities

1. **Add columns to database:**

```sql
ALTER TABLE Persons ADD CreatedBy uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE Persons ADD ModifiedBy uniqueidentifier NULL;
ALTER TABLE Persons ADD DeletedBy uniqueidentifier NULL;
```

2. **Backfill existing data (if needed):**

```sql
-- Set CreatedBy to OwnerId for existing records
UPDATE Persons 
SET CreatedBy = OwnerId 
WHERE CreatedBy = '00000000-0000-0000-0000-000000000000';
```

3. **Update application code** to set audit fields

4. **Add query filters** to DbContext

5. **Test thoroughly** with different user contexts

## Performance Considerations

### Indexing Strategy

```sql
-- Tenant isolation queries
CREATE INDEX IX_Persons_TenantId_DeletedOn 
ON Persons(TenantId, DeletedOn) 
WHERE DeletedOn IS NULL;

-- Owner-level RLS
CREATE INDEX IX_Persons_TenantId_OwnerId_DeletedOn 
ON Persons(TenantId, OwnerId, DeletedOn) 
WHERE DeletedOn IS NULL;

-- Audit queries
CREATE INDEX IX_Persons_CreatedBy ON Persons(CreatedBy);
CREATE INDEX IX_Persons_ModifiedBy ON Persons(ModifiedBy) WHERE ModifiedBy IS NOT NULL;
CREATE INDEX IX_Persons_DeletedBy ON Persons(DeletedBy) WHERE DeletedBy IS NOT NULL;
```

### Cosmos DB Optimization

```csharp
// Use PartitionKey for efficient queries
var feedOptions = new QueryRequestOptions
{
    PartitionKey = new PartitionKey(tenantId.ToString()),
    MaxItemCount = 100
};
```

## Summary

This implementation provides:

? **Audit Trail**: Track who created, modified, or deleted entities  
? **Multi-Tenant Isolation**: PartitionKey defaults to TenantId  
? **RLS Support**: Application and database-level security  
? **Compliance**: GDPR, SOX, HIPAA audit requirements  
? **Soft Delete**: Preserve audit trail after deletion  
? **Performance**: Optimized indexes for RLS queries  

This is a **bare minimum** security-by-design implementation. For production, consider:
- Immutable audit logs
- Field-level encryption
- Temporal tables (SQL Server)
- Advanced threat detection
- Rate limiting
- API-level authorization policies

## References

- [EF Core Global Query Filters](https://docs.microsoft.com/en-us/ef/core/querying/filters)
- [SQL Server Row-Level Security](https://docs.microsoft.com/en-us/sql/relational-databases/security/row-level-security)
- [Cosmos DB Partitioning](https://docs.microsoft.com/en-us/azure/cosmos-db/partitioning-overview)
- [GDPR Compliance](https://gdpr.eu/)

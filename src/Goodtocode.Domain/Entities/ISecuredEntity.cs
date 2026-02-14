namespace Goodtocode.Domain.Entities;

/// <summary>
/// Represents a secured domain entity that is associated with an owner and a tenant.
/// Provides ownership and tenancy information for access control and multi-tenancy scenarios.
/// </summary>
public interface ISecuredEntity<TModel> : IDomainEntity<TModel>, ISecurable
{
    /// <summary>
    /// Gets the owner identifier.
    /// This is typically the unique identifier (OID) of the user or entity that owns this resource.
    /// </summary>
    Guid OwnerId { get; }

    /// <summary>
    /// Gets the tenant identifier.
    /// This is the unique identifier (TID) of the tenant or organization to which this resource belongs.
    /// </summary>
    Guid TenantId { get; }
}
namespace Goodtocode.Domain.Entities;

public interface ISecuredEntity<TModel>
{
    /// <summary>
    /// Gets the owner identifier (formerly OwnerId, typically OID).
    /// </summary>
    Guid OwnerId { get; }
    /// <summary>
    /// Gets the tenant identifier (TID).
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Sets the owner identifier for the entity.
    /// </summary>
    /// <param name="value">The owner identifier (OID).</param>
    void SetOwnerId(Guid value);

    /// <summary>
    /// Sets the tenant identifier for the entity.
    /// </summary>
    /// <param name="value">The tenant identifier (TID).</param>
    void SetTenantId(Guid value);

    /// <summary>
    /// Sets both the owner identifier and tenant identifier for the entity.
    /// </summary>
    /// <param name="ownerId">The owner identifier (OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    void SetSecurityContext(Guid ownerId, Guid tenantId);
}
namespace Goodtocode.Domain.Entities;

/// <summary>
/// Base class for secured domain entities, extending <see cref="DomainEntity{TModel}"/> with external and tenant identifiers.
/// Suitable for multi-tenant and identity-aware scenarios (e.g., OID, TID from EEID/OBO flows).
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class SecuredEntity<TModel> : DomainEntity<TModel>, ISecuredEntity<TModel>
{
    /// <summary>
    /// Gets the owner identifier (formerly OwnerId, typically OID) for the entity.
    /// </summary>
    public Guid OwnerId { get; protected set; } = Guid.Empty; // OID

    /// <summary>
    /// Gets the tenant identifier (TID) for the entity, representing the tenant context.
    /// </summary>
    public Guid TenantId { get; protected set; } = Guid.Empty;    // TID

    /// <summary>
    /// Initializes a new instance of the <see cref="SecuredEntity{TModel}"/> class.
    /// </summary>
    protected SecuredEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected SecuredEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier, owner identifier, and tenant identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId, OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    protected SecuredEntity(Guid id, Guid ownerId, Guid tenantId) : this(id)
    {
        OwnerId = ownerId;
        TenantId = tenantId;
    }

    /// <summary>
    /// Sets the owner identifier for the entity.
    /// </summary>
    /// <param name="value">The owner identifier (OID).</param>
    public void SetOwnerId(Guid value) => OwnerId = value;

    /// <summary>
    /// Sets the tenant identifier for the entity.
    /// </summary>
    /// <param name="value">The tenant identifier (TID).</param>
    public void SetTenantId(Guid value) => TenantId = value;

    /// <summary>
    /// Sets both the owner identifier and tenant identifier for the entity.
    /// </summary>
    /// <param name="ownerId">The owner identifier (OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    public void SetSecurityContext(Guid ownerId, Guid tenantId)
    {
        OwnerId = ownerId;
        TenantId = tenantId;
    }
}
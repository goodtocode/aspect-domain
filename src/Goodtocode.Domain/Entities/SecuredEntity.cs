namespace Goodtocode.Domain.Entities;

/// <summary>
/// Base class for secured domain entities, extending <see cref="DomainEntity{TModel}"/> with external and tenant identifiers.
/// Suitable for multi-tenant and identity-aware scenarios (e.g., OID, TID from EEID/OBO flows).
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class SecuredEntity<TModel> : DomainEntity<TModel>, ISecurable
{
    /// <summary>
    /// Gets the partition key for the entity, defaults to TenantId for multi-tenant data isolation.
    /// </summary>
    public new string PartitionKey => TenantId.ToString();

    /// <summary>
    /// Gets the owner identifier (ObjectId/OID) for the entity.
    /// </summary>
    public Guid OwnerId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Gets the tenant identifier (TID) for the entity, representing the tenant context.
    /// </summary>
    public Guid TenantId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who last modified the entity.
    /// </summary>
    public Guid? ModifiedBy { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who deleted the entity.
    /// </summary>
    public Guid? DeletedBy { get; private set; }

    /// <summary>
    /// Initializes a new instance for ORM/serialization purposes.
    /// </summary>
    protected SecuredEntity() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// OwnerId and TenantId default to Guid.Empty.
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
    protected SecuredEntity(Guid id, Guid ownerId, Guid tenantId) : base(id)
    {
        OwnerId = ownerId;
        TenantId = tenantId;
    }

    /// <summary>
    /// Creates a new secured entity with the specified identifier, owner identifier, and tenant identifier.
    /// Factory method that ensures invariant state is properly protected.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId, OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    /// <typeparam name="TEntity">The concrete type of the secured entity being created.</typeparam>
    /// <returns>A new instance of the secured entity.</returns>
    protected static TEntity Create<TEntity>(Guid id, Guid ownerId, Guid tenantId)
        where TEntity : SecuredEntity<TModel>
    {
        return (TEntity)Activator.CreateInstance(typeof(TEntity), id, ownerId, tenantId)!;
    }

    /// <summary>
    /// Sets the identifier of the user who created the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    public void MarkCreated(Guid ownerId)
    {
        if(CreatedBy != Guid.Empty)
            return;
        CreatedBy = ownerId;
    }

    /// <summary>
    /// Sets the identifier of the user who last modified the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    public void MarkModified(Guid ownerId) => ModifiedBy = ownerId;

    /// <summary>
    /// Sets the identifier of the user who deleted the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    public void MarkDeleted(Guid ownerId)
    {
        if (DeletedBy.HasValue)
            return;
        MarkDeleted();
        DeletedBy = ownerId;        
    }

    /// <summary>
    /// Changes the owner identifier for the entity.
    /// </summary>
    /// <param name="newOwnerId">The new owner identifier (ObjectId/OID).</param>
    public void ChangeOwner(Guid newOwnerId)
    {
        OwnerId = newOwnerId;
    }

    /// <summary>
    /// Changes the tenant identifier for the entity.
    /// </summary>
    /// <param name="newTenantId">The new tenant identifier (TID).</param>
    public void ChangeTenant(Guid newTenantId)
    {
        TenantId = newTenantId;
    }
}
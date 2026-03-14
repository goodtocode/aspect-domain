namespace Goodtocode.Domain.Entities;

/// <summary>
/// Base class for secured domain entities, extending <see cref="DomainEntity{TModel}"/> with external and tenant identifiers.
/// Suitable for multi-tenant and identity-aware scenarios (e.g., OID, TID from EEID/OBO flows).
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class SecuredEntity<TModel> : DomainEntity<TModel>, ISecurable
{
    /// <summary>
    /// Gets the owner identifier (ObjectId/OID) for the entity.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the tenant identifier (TID) for the entity, representing the tenant context.
    /// </summary>
    public Guid TenantId { get; private set; }

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

    // Deterministic, explicit default constructor for serialization/ORM only
    /// <summary>
    /// Initializes a new instance for ORM/serialization purposes.
    /// </summary>
    protected SecuredEntity() : base()
    {
        OwnerId = Guid.Empty;
        TenantId = Guid.Empty;
        CreatedBy = Guid.Empty;
        ModifiedBy = null;
        DeletedBy = null;
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier, owner identifier, tenant identifier,
    /// creator identifier, creation timestamp, and last modified timestamp.
    /// Suitable for deserialization or scenarios where all properties need explicit setting.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId, OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    /// <param name="createdBy">The identifier of the user who created the entity.</param>
    /// <param name="createdOn">The creation timestamp for the entity.</param>
    /// <param name="timestamp">The last modified timestamp for the entity.</param>
    protected SecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp)
        : base(id, tenantId.ToString(), id.ToString(), createdOn, timestamp)
    {
        OwnerId = ownerId;
        TenantId = tenantId;
        CreatedBy = createdBy;
        ModifiedBy = null;
        DeletedBy = null;
    }

    /// <summary>
    /// Initializes a new instance with all keys and audit fields explicitly set.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="partitionKey">The partition key for the entity.</param>
    /// <param name="rowKey">The row key for the entity.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="createdBy">The identifier of the user who created the entity.</param>
    /// <param name="createdOn">The creation timestamp for the entity.</param>
    /// <param name="timestamp">The last modified timestamp for the entity.</param>
    protected SecuredEntity(Guid id, string partitionKey, string? rowKey, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp)
        : base(id, partitionKey, rowKey, createdOn, timestamp)
    {
        OwnerId = ownerId;
        TenantId = tenantId;
        CreatedBy = createdBy;
        ModifiedBy = null;
        DeletedBy = null;
    }

    // Factory for deterministic creation
    /// <summary>
    /// Creates a new secured entity with the specified identifier, owner identifier, and tenant identifier.
    /// Factory method that ensures invariant state is properly protected.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId, OID).</param>
    /// <param name="tenantId">The tenant identifier (TID).</param>
    /// <param name="createdBy">The identifier of the user who created the entity.</param>
    /// <param name="createdOn">The creation timestamp for the entity.</param>
    /// <param name="timestamp">The last modified timestamp for the entity.</param>
    /// <typeparam name="TEntity">The concrete type of the secured entity being created.</typeparam>
    /// <returns>A new instance of the secured entity.</returns>
    protected static TEntity Create<TEntity>(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp)
        where TEntity : SecuredEntity<TModel>
    {
        return (TEntity)Activator.CreateInstance(typeof(TEntity), id, ownerId, tenantId, createdBy, createdOn, timestamp)!;
    }

    /// <summary>
    /// Sets the identifier of the user who created the entity and the creation date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="createdOn">The creation date/time.</param>
    public void MarkCreated(Guid ownerId, DateTime createdOn)
    {
        base.MarkCreated(createdOn);
        if (CreatedBy != Guid.Empty)
            return;
        CreatedBy = ownerId;
    }

    /// <summary>
    /// Sets the identifier of the user who last modified the entity and the modification date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="modifiedOn">The modification date/time.</param>
    public void MarkModified(Guid ownerId, DateTime modifiedOn)
    {
        ModifiedBy = ownerId;
        base.MarkModified(modifiedOn);
    }

    /// <summary>
    /// Sets the identifier of the user who deleted the entity and the deletion date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="deletedOn">The deletion date/time.</param>
    public void MarkDeleted(Guid ownerId, DateTime deletedOn)
    {
        if (DeletedBy.HasValue)
            return;
        base.MarkDeleted(deletedOn);
        DeletedBy = ownerId;
    }

    /// <summary>
    /// Changes the owner identifier for the entity.
    /// </summary>
    /// <param name="newOwnerId">The new owner identifier (ObjectId/OID).</param>
    public void ChangeOwner(Guid newOwnerId)
    {
        if (OwnerId == newOwnerId)
            return;
        OwnerId = newOwnerId;
    }

    /// <summary>
    /// Changes the tenant identifier for the entity.
    /// </summary>
    /// <param name="newTenantId">The new tenant identifier (TID).</param>
    public void ChangeTenant(Guid newTenantId)
    {
        if (TenantId == newTenantId)
            return;
        TenantId = newTenantId;
    }
}
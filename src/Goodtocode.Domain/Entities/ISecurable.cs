namespace Goodtocode.Domain.Entities;

/// <summary>
/// Contract for entities that track user actions (create, modify, delete) and support owner and tenant changes.
/// </summary>
public interface ISecurable
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

    /// <summary>
    /// Identifier of the user who created the entity.
    /// </summary>
    Guid CreatedBy { get; }

    /// <summary>
    /// Identifier of the user who last modified the entity, or null if never modified.
    /// </summary>
    Guid? ModifiedBy { get; }

    /// <summary>
    /// Identifier of the user who deleted the entity, or null if not deleted.
    /// </summary>
    Guid? DeletedBy { get; }

    /// <summary>
    /// Sets the identifier of the user who created the entity and the creation date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="createdOn">The creation date/time.</param>
    void MarkCreated(Guid ownerId, DateTime createdOn);

    /// <summary>
    /// Sets the identifier of the user who last modified the entity and the modification date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="modifiedOn">The modification date/time.</param>
    void MarkModified(Guid ownerId, DateTime modifiedOn);

    /// <summary>
    /// Sets the identifier of the user who deleted the entity and the deletion date/time.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="deletedOn">The deletion date/time.</param>
    void MarkDeleted(Guid ownerId, DateTime deletedOn);

    /// <summary>
    /// Changes the owner identifier for the entity.
    /// </summary>
    /// <param name="newOwnerId">The new owner identifier (ObjectId/OID).</param>
    void ChangeOwner(Guid newOwnerId);

    /// <summary>
    /// Changes the tenant identifier for the entity.
    /// </summary>
    /// <param name="newTenantId">The new tenant identifier (TID).</param>
    void ChangeTenant(Guid newTenantId);
}

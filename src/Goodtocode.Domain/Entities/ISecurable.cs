namespace Goodtocode.Domain.Entities;

/// <summary>
/// Contract for entities that track user actions (create, modify, delete) and support owner and tenant changes.
/// </summary>
public interface ISecurable
{
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
    /// Sets the identifier of the user who created the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    void MarkCreated(Guid ownerId);

    /// <summary>
    /// Sets the identifier of the user who last modified the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    void MarkModified(Guid ownerId);

    /// <summary>
    /// Sets the identifier of the user who deleted the entity.
    /// </summary>
    /// <param name="ownerId">The user identifier.</param>
    void MarkDeleted(Guid ownerId);

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

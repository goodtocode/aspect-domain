namespace Goodtocode.Domain.Entities;

public interface ISecuredEntity<TModel> : IDomainEntity<TModel>
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
    /// Gets the unique identifier of the user who created the entity.
    /// </summary>
    Guid CreatedBy { get; }

    /// <summary>
    /// Gets the identifier of the user who last modified this entity.
    /// </summary>
    Guid? ModifiedBy { get; }

    /// <summary>
    /// Gets the identifier of the user who deleted this entity, or null if not deleted.
    /// </summary>
    Guid? DeletedBy { get; }
}
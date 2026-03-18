namespace Goodtocode.Domain.Entities;

using System;

/// <summary>
/// Base class for versioned domain entities, extending <see cref="SecuredEntity{TModel}"/>.
/// Provides versioning and previous version tracking for immutable/auditable entities.
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class VersionedEntity<TModel> : SecuredEntity<TModel>, IVersionable
{
    /// <summary>
    /// Gets the version number of the entity.
    /// </summary>
    public int Version { get; protected set; } = 1;

    /// <summary>
    /// Gets the identifier of the previous version of this entity, if any.
    /// </summary>
    public Guid? PreviousVersionId { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the entity is pinned.
    /// </summary>
    public bool IsPinned { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the entity is frozen.
    /// </summary>
    public bool IsFrozen { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedEntity{TModel}"/> class.
    /// </summary>
    protected VersionedEntity() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedEntity{TModel}"/> class with specified values.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="partitionKey">The partition key for storage.</param>
    /// <param name="rowKey">The row key for storage (optional).</param>
    /// <param name="ownerId">The identifier of the owner.</param>
    /// <param name="tenantId">The identifier of the tenant.</param>
    /// <param name="createdBy">The identifier of the creator.</param>
    /// <param name="createdOn">The creation date and time.</param>
    /// <param name="timestamp">The timestamp for concurrency.</param>
    /// <param name="version">The version number of the entity.</param>
    /// <param name="previousVersionId">The identifier of the previous version, if any.</param>
    protected VersionedEntity(
        Guid id,
        string partitionKey,
        string? rowKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        int version,
        Guid? previousVersionId = null)
        : base(id, partitionKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
        Version = version;
        PreviousVersionId = previousVersionId;
    }

    /// <summary>
    /// Increments the version number and sets the previous version identifier.
    /// </summary>
    /// <param name="previousVersionId">The identifier of the previous version, or null if not applicable.</param>
    public virtual void BumpVersion(Guid? previousVersionId = null)
    {
        Version++;
        PreviousVersionId = previousVersionId;
    }

    /// <summary>
    /// Pins the entity, marking it as pinned.
    /// </summary>
    public virtual void Pin() => IsPinned = true;

    /// <summary>
    /// Freezes the entity, marking it as frozen. Pins the entity if not already pinned.
    /// </summary>
    public virtual void Freeze()
    {
        if (!IsPinned)
            Pin();
        IsFrozen = true;
    }

    /// <summary>
    /// Thaws the entity, unmarking it as frozen.
    /// </summary>
    public virtual void Thaw() => IsFrozen = false;
}

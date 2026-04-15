using System;

namespace Goodtocode.Domain.Entities;

/// <summary>
/// Base class for secured, versioned, and immutable domain entities.
/// <para>
/// <b>Invariants:</b>
/// <list type="bullet">
///   <item>Persisted rows are immutable.</item>
///   <item>New versions are new rows.</item>
///   <item>Versioning is only possible via <see cref="CreateNextVersion"/>.</item>
///   <item>Frozen series cannot be versioned.</item>
///   <item>Successors require a new <see cref="CanonicalKey"/>.</item>
///   <item><see cref="RowKey"/> is opaque and never meaningful.</item>
///   <item><see cref="PartitionKey"/> is <c>TenantId:CanonicalKey</c> and immutable.</item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class SecuredVersionedEntity<TModel> : SecuredEntity<TModel>, IVersionable
{
    /// <summary>
    /// Gets the logical identity key that anchors this version series within the tenant.
    /// All versions of the same entity share the same CanonicalKey.
    /// PartitionKey is computed as <c>TenantId:CanonicalKey</c>.
    /// </summary>
    public string CanonicalKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the 1-based version number within the <see cref="CanonicalKey"/> series.
    /// </summary>
    public int Version { get; private set; } = 1;

    /// <summary>
    /// Gets the <see cref="DomainEntity{TModel}.Id"/> of the preceding row in this series, or null for the first version.
    /// </summary>
    public Guid? PreviousVersionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this row is the current latest version in its series.
    /// Exactly one row per <see cref="CanonicalKey"/> should have IsLatest = true at any time.
    /// </summary>
    public bool IsLatest { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether this version is pinned.
    /// Changing the pinned state always produces a new row via <see cref="CreateNextVersion"/>.
    /// </summary>
    public bool IsPinned { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this series is frozen.
    /// Set via <see cref="Freeze"/>. When true, <see cref="CreateNextVersion"/> throws.
    /// <see cref="CreateSuccessor"/> remains allowed even when frozen.
    /// </summary>
    public bool IsFrozen { get; private set; }

    /// <summary>
    /// Initializes a new instance for ORM/serialization only. Does not generate keys.
    /// </summary>
    protected SecuredVersionedEntity() : base() { }

    /// <summary>
    /// Initializes a new instance with all fields set explicitly.
    /// Use this constructor for reconstruction from storage or within factory methods.
    /// Pass <c>null</c> for <paramref name="rowKey"/> to auto-generate a new UUIDv7.
    /// <see cref="DomainEntity{TModel}.PartitionKey"/> is computed as <c>TenantId:CanonicalKey</c>.
    /// </summary>
    protected SecuredVersionedEntity(
        Guid id,
        string canonicalKey,
        string? rowKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        int version,
        Guid? previousVersionId,
        bool isLatest,
        bool isPinned,
        bool isFrozen)
        : base(id, $"{tenantId}:{canonicalKey}", rowKey, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
        CanonicalKey = canonicalKey;
        Version = version;
        PreviousVersionId = previousVersionId;
        IsLatest = isLatest;
        IsPinned = isPinned;
        IsFrozen = isFrozen;
    }

    /// <summary>
    /// Freezes this series, preventing new versions from being created.
    /// <see cref="CreateSuccessor"/> remains allowed after freezing.
    /// </summary>
    public void Freeze() => IsFrozen = true;

    /// <summary>
    /// Marks this row as no longer the latest version in its series.
    /// Must be called transactionally by the caller when a new version or successor is created.
    /// </summary>
    public void MarkNotLatest() => IsLatest = false;

    /// <summary>
    /// Creates a new version row in the same series as this entity.
    /// The new row has a new <see cref="DomainEntity{TModel}.Id"/>, a new UUIDv7 <see cref="DomainEntity{TModel}.RowKey"/>,
    /// the same <see cref="CanonicalKey"/>, <see cref="Version"/> incremented by 1,
    /// <see cref="PreviousVersionId"/> set to this row's <see cref="DomainEntity{TModel}.Id"/>,
    /// and <see cref="IsLatest"/> = true.
    /// </summary>
    /// <returns>The new version entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the series is frozen.</exception>
    public TModel CreateNextVersion()
    {
        if (IsFrozen)
            throw new InvalidOperationException($"Cannot create a new version of a frozen entity (CanonicalKey: {CanonicalKey}).");
        return CreateNextVersionCore();
    }

    /// <summary>
    /// Creates a successor entity in a new series identified by <paramref name="newCanonicalKey"/>.
    /// The new row has a new <see cref="DomainEntity{TModel}.Id"/>, a new UUIDv7 <see cref="DomainEntity{TModel}.RowKey"/>,
    /// a new <see cref="DomainEntity{TModel}.PartitionKey"/> (<c>TenantId:newCanonicalKey</c>),
    /// <see cref="Version"/> = 1, <see cref="PreviousVersionId"/> = null, and <see cref="IsLatest"/> = true.
    /// Allowed even when the current series is frozen.
    /// </summary>
    /// <param name="newCanonicalKey">The canonical key for the new successor series.</param>
    /// <returns>The successor entity.</returns>
    public TModel CreateSuccessor(string newCanonicalKey)
    {
        if (string.IsNullOrWhiteSpace(newCanonicalKey))
            throw new ArgumentException("newCanonicalKey cannot be null or whitespace.", nameof(newCanonicalKey));
        var successor = CreateSuccessorCore(newCanonicalKey);
        OnSuccessorCreated((TModel)(object)this, successor);
        return successor;
    }

    /// <summary>
    /// Derived classes implement this to construct the next version row.
    /// The implementation must supply: new Id, null rowKey (UUID7), same CanonicalKey, same TenantId/OwnerId,
    /// Version = <see cref="Version"/> + 1, PreviousVersionId = <see cref="DomainEntity{TModel}.Id"/>,
    /// isLatest = true, isPinned = false, isFrozen = false.
    /// </summary>
    protected abstract TModel CreateNextVersionCore();

    /// <summary>
    /// Derived classes implement this to construct the successor row.
    /// The implementation must supply: new Id, null rowKey (UUID7), the provided newCanonicalKey,
    /// same TenantId/OwnerId, version = 1, previousVersionId = null,
    /// isLatest = true, isPinned = false, isFrozen = false.
    /// </summary>
    protected abstract TModel CreateSuccessorCore(string newCanonicalKey);

    /// <summary>
    /// Optional extension point invoked immediately after a successor is created.
    /// No-op by default. Override to emit domain events or record narrative supersession.
    /// Does not create relationships or persist anything.
    /// </summary>
    /// <param name="predecessor">The entity from which the successor was created (this instance).</param>
    /// <param name="successor">The newly created successor entity.</param>
    protected virtual void OnSuccessorCreated(TModel predecessor, TModel successor)
    {
        // No-op by default. Override in derived types to handle narrative supersession.
    }
}

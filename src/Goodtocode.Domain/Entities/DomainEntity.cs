using Goodtocode.Domain.Events;
using System.Runtime.Serialization;

namespace Goodtocode.Domain.Entities;

/// <summary>
/// Base class for domain entities, providing audit fields, domain events, and identity management.
/// Suitable for DDD and clean architecture implementations with support for EF/SQL, EF/CosmosDb, or custom repositories.
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
public abstract class DomainEntity<TModel> : IDomainEntity<TModel>
{
    private readonly List<IDomainEvent<TModel>> _domainEvents = [];

    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the partition key for the entity, used for CosmosDb or similar stores.
    /// </summary>
    public string PartitionKey { get; private set; }

    /// <summary>
    /// Gets the creation date and time of the entity.
    /// </summary>
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the last modification date and time of the entity.
    /// </summary>
    public DateTime? ModifiedOn { get; private set; }

    /// <summary>
    /// Gets the deletion date and time of the entity, if deleted.
    /// </summary>
    public DateTime? DeletedOn { get; private set; }

    /// <summary>
    /// Gets the timestamp for concurrency and audit purposes.
    /// </summary>
    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the list of domain events associated with this entity.
    /// </summary>
    [IgnoreDataMember]
    public IReadOnlyList<IDomainEvent<TModel>> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEntity{TModel}"/> class.
    /// </summary>
    protected DomainEntity()
    {
        PartitionKey = Id.ToString();
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected DomainEntity(Guid id)
        : this()
    {
        Id = id;
        PartitionKey = id.ToString();
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier and partition key.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="partitionKey">The partition key for the entity.</param>
    protected DomainEntity(Guid id, string partitionKey)
        : this(id)
    {
        PartitionKey = partitionKey;
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier, partition key, creation date, and timestamp.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="partitionKey">The partition key for the entity.</param>
    /// <param name="createdOn">The creation date and time.</param>
    /// <param name="timestamp">The timestamp for concurrency and audit.</param>
    internal DomainEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
        : this(id, partitionKey)
    {
        CreatedOn = createdOn;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Adds a domain event to the entity's event list.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent<TModel> domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Sets the last modification date and time of the entity to the current UTC time.
    /// </summary>
    public void MarkModified() => ModifiedOn = DateTime.UtcNow;

    /// <summary>
    /// Sets the deletion date and time of the entity to the current UTC time.
    /// </summary>
    /// <param name="value">The deletion date and time.</param>
    public void MarkDeleted()
    {
        if (DeletedOn == null)
            DeletedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as not deleted by clearing the deletion date and time.
    /// </summary>
    public void MarkUndeleted()
    {
        if (DeletedOn != null)
            DeletedOn = null;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not DomainEntity<TModel> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetRealType() != other.GetRealType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Determines whether two domain entities are equal.
    /// </summary>
    /// <param name="a">The first entity.</param>
    /// <param name="b">The second entity.</param>
    /// <returns>True if the entities are equal; otherwise, false.</returns>
    public static bool operator ==(DomainEntity<TModel>? a, DomainEntity<TModel>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two domain entities are not equal.
    /// </summary>
    /// <param name="a">The first entity.</param>
    /// <param name="b">The second entity.</param>
    /// <returns>True if the entities are not equal; otherwise, false.</returns>
    public static bool operator !=(DomainEntity<TModel>? a, DomainEntity<TModel>? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Returns a hash code for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + GetRealType().ToString().GetHashCode();
            hash = hash * 23 + Id.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Gets the real type of the entity, accounting for proxy types.
    /// </summary>
    /// <param name="namespaceRoot">Optional namespace root to check for proxy types.</param>
    /// <returns>The actual type of the entity.</returns>
    private Type GetRealType(string namespaceRoot = "")
    {
        var type = GetType();

        if (type.ToString().Contains(namespaceRoot))
            return type.BaseType ?? type.GetType();

        return type;
    }
}
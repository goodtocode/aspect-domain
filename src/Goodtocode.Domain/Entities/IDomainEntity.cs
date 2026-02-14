using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Entities;

/// <summary>
/// Defines the contract for a domain entity with auditing and domain event support.
/// </summary>
/// <typeparam name="TModel">The type of the domain model.</typeparam>
    public interface IDomainEntity<TModel>: IAuditable
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the partition key used for data partitioning in distributed storage systems.
    /// </summary>
    string PartitionKey { get; }

    /// <summary>
    /// Gets the timestamp of the last operation on the entity.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    void AddDomainEvent(IDomainEvent<TModel> domainEvent);

    /// <summary>
    /// Clears all domain events from the entity's event collection.
    /// </summary>
    void ClearDomainEvents();

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the specified object is equal to the current entity; otherwise, false.</returns>
    bool Equals(object obj);

    /// <summary>
    /// Serves as the default hash function for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    int GetHashCode();
}
namespace Goodtocode.Domain.Entities;

/// <summary>
/// Defines the contract for entities that track creation, modification, and deletion timestamps.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedOn { get; }

    /// <summary>
    /// Gets the date and time when the entity was last modified, or null if never modified.
    /// </summary>
    DateTime? ModifiedOn { get; }

    /// <summary>
    /// Gets the date and time when this entity was soft-deleted, or null if not deleted.
    /// </summary>
    DateTime? DeletedOn { get; }

    /// <summary>
    /// Sets the date and time when the entity was last modified.
    /// </summary>
    void MarkModified();

    /// <summary>
    /// Marks the entity as deleted by setting the deletion timestamp, if not already deleted.
    /// </summary>    
    void MarkDeleted();

    /// <summary>
    /// Marks the entity as undeleted by clearing the deletion timestamp, if currently deleted.
    /// </summary>
    void MarkUndeleted();
}
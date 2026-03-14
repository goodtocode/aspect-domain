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
    /// <param name="modifiedOn">The modification date/time.</param>
    void MarkModified(DateTime modifiedOn);

    /// <summary>
    /// Marks the entity as deleted by setting the deletion timestamp, if not already deleted.
    /// </summary>
    /// <param name="deletedOn">The deletion date/time.</param>
    void MarkDeleted(DateTime deletedOn);

    /// <summary>
    /// Marks the entity as undeleted by clearing the deletion timestamp, if currently deleted.
    /// </summary>
    void MarkUndeleted();
}
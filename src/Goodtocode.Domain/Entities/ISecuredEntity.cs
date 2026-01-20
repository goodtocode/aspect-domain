namespace Goodtocode.Domain.Entities;

public interface ISecuredEntity<TModel>
{
    /// <summary>
    /// Gets the owner identifier (formerly OwnerId, typically OID).
    /// </summary>
    Guid OwnerId { get; }
    /// <summary>
    /// Gets the tenant identifier (TID).
    /// </summary>
    Guid TenantId { get; }

}
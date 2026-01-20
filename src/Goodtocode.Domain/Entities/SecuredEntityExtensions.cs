namespace Goodtocode.Domain.Entities;

/// <summary>
/// Provides extension methods for querying and authorizing secured entities.
/// </summary>
public static class SecuredEntityExtensions
{
    /// <summary>
    /// Filters the query to entities owned by the specified owner identifier.
    /// </summary>
    /// <typeparam name="T">The type of the secured entity.</typeparam>
    /// <param name="query">The queryable collection of entities.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId).</param>
    /// <returns>A filtered queryable of entities owned by the specified owner identifier.</returns>
    public static IQueryable<T> IsOwner<T>(this IQueryable<T> query, Guid ownerId) where T : ISecuredEntity<T>
    {
        return query.Where(x => x.OwnerId == ownerId);
    }

    /// <summary>
    /// Filters the query to entities owned by the specified owner identifier.
    /// </summary>
    /// <typeparam name="T">The type of the secured entity.</typeparam>
    /// <param name="query">The queryable collection of entities.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId).</param>
    /// <returns>A filtered queryable of entities owned by the specified owner identifier.</returns>
    public static IQueryable<T> WhereOwner<T>(this IQueryable<T> query, Guid ownerId) where T : ISecuredEntity<T>
    {
        return query.Where(x => x.OwnerId == ownerId);
    }

    /// <summary>
    /// Determines whether the entity is authorized for the specified owner or tenant identifier.
    /// </summary>
    /// <typeparam name="T">The type of the secured entity.</typeparam>
    /// <param name="entity">The entity to check authorization for.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId).</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>True if the entity is authorized; otherwise, false.</returns>
    public static bool IsAuthorized<T>(this T entity, Guid ownerId, Guid tenantId) where T : ISecuredEntity<T>
    {
        return entity.OwnerId == ownerId || entity.TenantId == tenantId;
    }

    /// <summary>
    /// Filters the query to entities authorized for the specified tenant or owner identifier.
    /// </summary>
    /// <typeparam name="T">The type of the secured entity.</typeparam>
    /// <param name="query">The queryable collection of entities.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ownerId">The owner identifier (formerly OwnerId).</param>
    /// <returns>A filtered queryable of entities authorized for the specified tenant or owner identifier.</returns>
    public static IQueryable<T> WhereAuthorized<T>(this IQueryable<T> query, Guid tenantId, Guid ownerId) where T : ISecuredEntity<T>
    {
        return query.Where(x => x.TenantId == tenantId || x.OwnerId == ownerId);
    }
}

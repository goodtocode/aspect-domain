using Goodtocode.Domain.Entities;
using System.Reflection;

namespace Goodtocode.Domain.Tests.TestHelpers;

/// <summary>
/// Helper methods for testing entities, providing reflection-based access to private setters for audit fields.
/// These methods should only be used in tests to bypass invariant state protection.
/// </summary>
public static class EntityTestHelpers
{
    /// <summary>
    /// Sets the ModifiedOn audit field using reflection (for testing only).
    /// </summary>
    public static void SetModifiedOn<TModel>(this DomainEntity<TModel> entity, DateTime? value)
    {
        typeof(DomainEntity<TModel>)
            .GetProperty(nameof(DomainEntity<TModel>.ModifiedOn))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the DeletedOn audit field using reflection (for testing only).
    /// </summary>
    public static void SetDeletedOn<TModel>(this DomainEntity<TModel> entity, DateTime? value)
    {
        typeof(DomainEntity<TModel>)
            .GetProperty(nameof(DomainEntity<TModel>.DeletedOn))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the CreatedBy audit field using reflection (for testing only).
    /// </summary>
    public static void SetCreatedBy<TModel>(this SecuredEntity<TModel> entity, Guid value)
    {
        typeof(SecuredEntity<TModel>)
            .GetProperty(nameof(SecuredEntity<TModel>.CreatedBy))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the ModifiedBy audit field using reflection (for testing only).
    /// </summary>
    public static void SetModifiedBy<TModel>(this SecuredEntity<TModel> entity, Guid? value)
    {
        typeof(SecuredEntity<TModel>)
            .GetProperty(nameof(SecuredEntity<TModel>.ModifiedBy))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the DeletedBy audit field using reflection (for testing only).
    /// </summary>
    public static void SetDeletedBy<TModel>(this SecuredEntity<TModel> entity, Guid? value)
    {
        typeof(SecuredEntity<TModel>)
            .GetProperty(nameof(SecuredEntity<TModel>.DeletedBy))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the OwnerId using reflection (for testing only).
    /// </summary>
    public static void SetOwnerId<TModel>(this SecuredEntity<TModel> entity, Guid value)
    {
        typeof(SecuredEntity<TModel>)
            .GetProperty(nameof(SecuredEntity<TModel>.OwnerId))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets the TenantId using reflection (for testing only).
    /// </summary>
    public static void SetTenantId<TModel>(this SecuredEntity<TModel> entity, Guid value)
    {
        typeof(SecuredEntity<TModel>)
            .GetProperty(nameof(SecuredEntity<TModel>.TenantId))!
            .SetValue(entity, value);
    }

    /// <summary>
    /// Sets both OwnerId and TenantId using reflection (for testing only).
    /// </summary>
    public static void SetSecurityContext<TModel>(this SecuredEntity<TModel> entity, Guid ownerId, Guid tenantId)
    {
        entity.SetOwnerId(ownerId);
        entity.SetTenantId(tenantId);
    }
}

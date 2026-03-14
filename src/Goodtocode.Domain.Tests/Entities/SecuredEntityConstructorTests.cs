using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityConstructorTests
{
    private sealed class TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp) : SecuredEntity<TestSecuredEntity>(id, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
    }

    [TestMethod]
    public void DefaultConstructorInitializesToEmptyGuids()
    {
        var entity = new TestSecuredEntity(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        Assert.IsNotNull(entity);
        Assert.AreEqual(Guid.Empty, entity.OwnerId);
        Assert.AreEqual(Guid.Empty, entity.TenantId);
        Assert.AreEqual(Guid.Empty, entity.CreatedBy);
        Assert.IsNull(entity.ModifiedBy);
        Assert.IsNull(entity.DeletedBy);
    }

    [TestMethod]
    public void ConstructorWithIdSetsIdAndInitializesSecurityToEmptyGuids()
    {
        var id = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        Assert.IsNotNull(entity);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(Guid.Empty, entity.OwnerId);
        Assert.AreEqual(Guid.Empty, entity.TenantId);
        Assert.AreEqual(Guid.Empty, entity.CreatedBy);
        Assert.IsNull(entity.ModifiedBy);
        Assert.IsNull(entity.DeletedBy);
    }

    [TestMethod]
    public void ConstructorSetsOwnerIdAndTenantId()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        Assert.IsNotNull(entity);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreEqual(id, entity.Id);
    }
}

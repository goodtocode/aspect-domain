using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityPropertyTests
{
    private sealed class TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp) : SecuredEntity<TestSecuredEntity>(id, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
    }

    [TestMethod]
    public void PartitionKeyReturnsTenantIdAsString()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        Assert.AreEqual(tenantId.ToString(), entity.PartitionKey);
    }

    [TestMethod]
    public void InheritedPropertiesFromDomainEntityAreAccessible()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(entity.PartitionKey, entity.TenantId.ToString());
    }

    [TestMethod]
    public void ChangeOwnerUpdatesOwnerId()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var ownerId = Guid.NewGuid();
        entity.ChangeOwner(ownerId);
        Assert.AreEqual(ownerId, entity.OwnerId);
    }

    [TestMethod]
    public void ChangeTenantUpdatesTenantId()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var tenantId = Guid.NewGuid();
        entity.ChangeTenant(tenantId);
        Assert.AreEqual(tenantId, entity.TenantId);
    }
}

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

    [TestMethod]
    public void ConstructorWithExplicitPartitionAndRowKeySetsAllProperties()
    {
        var id = Guid.NewGuid();
        var partitionKey = "custom-partition";
        var rowKey = "custom-row";
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdOn = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 2, 1, 10, 0, 0, TimeSpan.Zero);
        var entity = new TestSecuredEntityWithKeys(id, partitionKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(rowKey, entity.RowKey);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreEqual(createdBy, entity.CreatedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
    }

    private sealed class TestSecuredEntityWithKeys : SecuredEntity<TestSecuredEntityWithKeys>
    {
        public TestSecuredEntityWithKeys(Guid id, string partitionKey, string? rowKey, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp) { }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Goodtocode.Domain.Entities;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredVersionedEntityTests
{
    private sealed class TestSecuredVersionedEntity : SecuredVersionedEntity<TestSecuredVersionedEntity>
    {
        public TestSecuredVersionedEntity() : base() { }
        public TestSecuredVersionedEntity(
            Guid id,
            string partitionKey,
            string? rowKey,
            Guid ownerId,
            Guid tenantId,
            Guid createdBy,
            DateTime createdOn,
            DateTimeOffset timestamp,
            int version,
            Guid? previousVersionId = null)
            : base(id, partitionKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp, version, previousVersionId) { }
    }

    [TestMethod]
    public void DefaultConstructorInitializesDefaults()
    {
        var entity = new TestSecuredVersionedEntity();
        Assert.AreEqual(1, entity.Version);
        Assert.IsNull(entity.PreviousVersionId);
        Assert.IsFalse(entity.IsPinned);
        Assert.IsFalse(entity.IsFrozen);
    }

    [TestMethod]
    public void ParameterizedConstructorSetsProperties()
    {
        var id = Guid.NewGuid();
        var partitionKey = "partition";
        var rowKey = "row";
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdOn = DateTime.UtcNow;
        var timestamp = DateTimeOffset.UtcNow;
        var version = 5;
        var previousVersionId = Guid.NewGuid();
        var entity = new TestSecuredVersionedEntity(id, partitionKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp, version, previousVersionId);
        Assert.AreEqual(version, entity.Version);
        Assert.AreEqual(previousVersionId, entity.PreviousVersionId);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(rowKey, entity.RowKey);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreEqual(createdBy, entity.CreatedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
    }

    [TestMethod]
    public void PinSetsIsPinnedTrue()
    {
        var entity = new TestSecuredVersionedEntity();
        entity.Pin();
        Assert.IsTrue(entity.IsPinned);
    }

    [TestMethod]
    public void FreezeSetsIsFrozenTrueAndPinsIfNotPinned()
    {
        var entity = new TestSecuredVersionedEntity();
        Assert.IsFalse(entity.IsPinned);
        Assert.IsFalse(entity.IsFrozen);
        entity.Freeze();
        Assert.IsTrue(entity.IsFrozen);
        Assert.IsTrue(entity.IsPinned);
    }

    [TestMethod]
    public void ThawSetsIsFrozenFalse()
    {
        var entity = new TestSecuredVersionedEntity();
        entity.Freeze();
        Assert.IsTrue(entity.IsFrozen);
        entity.Thaw();
        Assert.IsFalse(entity.IsFrozen);
    }

    [TestMethod]
    public void BumpVersionIncrementsVersionAndSetsPreviousVersionId()
    {
        var entity = new TestSecuredVersionedEntity();
        var prevId = Guid.NewGuid();
        var initialVersion = entity.Version;
        entity.BumpVersion(prevId);
        Assert.AreEqual(initialVersion + 1, entity.Version);
        Assert.AreEqual(prevId, entity.PreviousVersionId);
    }
}

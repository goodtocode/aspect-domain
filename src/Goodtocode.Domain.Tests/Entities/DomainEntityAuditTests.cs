using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityAuditTests
{
    private sealed class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
        public TestEntity() { }
        public TestEntity(Guid id, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, string rowKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, rowKey, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, id.ToString(), createdOn, timestamp) { }
    }

    [TestMethod]
    public void AuditFieldsAreSetCorrectly()
    {
        var id = Guid.NewGuid();
        var now = new DateTime(2024, 1, 4, 12, 0, 0, DateTimeKind.Utc);
        var ts = new DateTimeOffset(2024, 1, 4, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, "pk", now, ts);
        Assert.AreEqual("pk", entity.PartitionKey);
        Assert.AreEqual(now, entity.CreatedOn);
        Assert.AreEqual(ts, entity.Timestamp);
        Assert.AreEqual(entity.Id.ToString(), entity.RowKey);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
    }

    [TestMethod]
    public void AuditFieldsCanBeAccessedViaProperties()
    {
        var id = Guid.NewGuid();
        var partitionKey = "pk";
        var createdOn = new DateTime(2024, 1, 6, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 6, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(id, entity.Id);
    }

    [TestMethod]
    public void MarkModifiedSetsModifiedOn()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 18, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 18, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var modifiedOn = new DateTime(2024, 1, 18, 13, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(modifiedOn);
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void MarkModifiedUpdatesModifiedOnToMoreRecentValue()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 19, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 19, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var firstModified = new DateTime(2024, 1, 19, 13, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(firstModified);
        Assert.AreEqual(firstModified, entity.ModifiedOn);
        var secondModified = new DateTime(2024, 1, 19, 14, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(secondModified);
        Assert.AreEqual(secondModified, entity.ModifiedOn);
        Assert.IsGreaterThan(firstModified, secondModified, "ModifiedOn should be updated to a more recent value");
    }

    [TestMethod]
    public void MarkDeletedSetsDeletedOn()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 20, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var deletedOn = new DateTime(2024, 1, 20, 13, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(deletedOn);
        Assert.AreEqual(deletedOn, entity.DeletedOn);
    }

    [TestMethod]
    public void MarkDeletedDoesNotOverwriteExistingDeletedOn()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 21, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 21, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var firstDeletedOn = new DateTime(2024, 1, 21, 13, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(firstDeletedOn);
        Assert.AreEqual(firstDeletedOn, entity.DeletedOn);
        var secondDeletedOn = new DateTime(2024, 1, 21, 14, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(secondDeletedOn);
        Assert.AreEqual(firstDeletedOn, entity.DeletedOn, "DeletedOn should not be overwritten if already set");
    }

    [TestMethod]
    public void MarkUndeletedClearsDeletedOn()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 22, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 22, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var deletedOn = new DateTime(2024, 1, 22, 13, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(deletedOn);
        Assert.AreEqual(deletedOn, entity.DeletedOn);
        entity.MarkUndeleted();
        Assert.IsNull(entity.DeletedOn);
    }
}

using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityConstructorTests
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
    public void ConstructorWithIdSetsId()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        Assert.IsNotNull(entity);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(id.ToString(), entity.PartitionKey);
        Assert.IsTrue(Guid.TryParse(entity.RowKey, out _), "RowKey should be a valid UUID");
        Assert.AreNotEqual(id.ToString(), entity.RowKey, "RowKey should be UUID7, not the entity Id");
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
    }

    [TestMethod]
    public void ConstructorWithIdAndPartitionKeySetsProperties()
    {
        var id = Guid.NewGuid();
        var partitionKey = "test-partition";
        var createdOn = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 2, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);
        Assert.IsNotNull(entity);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(id.ToString(), entity.RowKey);
    }

    [TestMethod]
    public void ConstructorWithPartitionKeyOnlyMaintainsPartitionKey()
    {
        var id = Guid.NewGuid();
        var partitionKey = "cosmos-partition-key";
        var createdOn = new DateTime(2024, 1, 3, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 3, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(string.Empty, entity.Name);
        Assert.AreEqual(id.ToString(), entity.RowKey);
    }

    [TestMethod]
    public void FullConstructorSetsAllProperties()
    {
        var id = Guid.NewGuid();
        var partitionKey = "full-partition";
        var createdOn = new DateTime(2024, 1, 5, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 5, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
        Assert.AreEqual(id.ToString(), entity.RowKey);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
    }

    [TestMethod]
    public void ConstructorWithIdPartitionKeyAndRowKeySetsRowKey()
    {
        var id = Guid.NewGuid();
        var partitionKey = "test-partition";
        var rowKey = "custom-row-key";
        var createdOn = new DateTime(2024, 1, 23, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 23, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, partitionKey, rowKey, createdOn, timestamp);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(rowKey, entity.RowKey);
    }
}

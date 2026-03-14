using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityEqualityTests
{
    private sealed class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
        public TestEntity(Guid id, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, string rowKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, rowKey, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, id.ToString(), createdOn, timestamp) { }
    }

    [TestMethod]
    public void EqualityEntitiesWithSameIdAreEqual()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 13, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 13, 12, 0, 0, TimeSpan.Zero);
        var a = new TestEntity(id, createdOn, timestamp);
        var b = new TestEntity(id, createdOn, timestamp);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityEntitiesWithDifferentIdAreNotEqual()
    {
        var createdOn = new DateTime(2024, 1, 14, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 14, 12, 0, 0, TimeSpan.Zero);
        var a = new TestEntity(Guid.NewGuid(), createdOn, timestamp);
        var b = new TestEntity(Guid.NewGuid(), createdOn, timestamp);
        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void EqualitySameIdDifferentPropertiesAreEqual()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var a = new TestEntity(id, createdOn, timestamp) { Name = "Entity A" };
        var b = new TestEntity(id, createdOn, timestamp) { Name = "Entity B" };
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void EqualitySameIdDifferentPartitionKeysAreEqual()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 16, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 16, 12, 0, 0, TimeSpan.Zero);
        var a = new TestEntity(id, "partition-a", createdOn, timestamp);
        var b = new TestEntity(id, "partition-b", createdOn, timestamp);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void EqualitySameReferenceIsEqual()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 17, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 17, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        Assert.IsTrue(entity.Equals(entity));
    }

    [TestMethod]
    public void EqualityWithNullReturnsFalse()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 18, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 18, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        Assert.IsFalse(entity.Equals(null));
        Assert.IsFalse(entity == null);
        Assert.IsTrue(entity != null);
    }

    [TestMethod]
    public void EqualityBothNullReturnsTrue()
    {
        TestEntity? a = null;
        TestEntity? b = null;
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityWithEmptyGuidReturnsFalse()
    {
        var createdOn = new DateTime(2024, 1, 19, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 19, 12, 0, 0, TimeSpan.Zero);
        var a = new TestEntity(Guid.Empty, createdOn, timestamp);
        var b = new TestEntity(Guid.Empty, createdOn, timestamp);
        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void EqualityWithDifferentTypeReturnsFalse()
    {
        var createdOn = new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 20, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(Guid.NewGuid(), createdOn, timestamp);
        var notAnEntity = new object();
        Assert.IsFalse(entity.Equals(notAnEntity));
    }
}

using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityTests
{
    private class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
        public TestEntity() { }
        public TestEntity(Guid id) : base(id) { }
        public TestEntity(Guid id, string partitionKey) : base(id, partitionKey) { }
        public TestEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, createdOn, timestamp) { }
    }

    private class TestEvent : IDomainEvent<TestEntity>
    {
        public TestEntity Item => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }

    [TestMethod]
    public void AuditFieldsAreSetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var ts = DateTimeOffset.UtcNow;
        var entity = new TestEntity(Guid.NewGuid(), "pk", now, ts);

        // Act & Assert
        Assert.AreEqual("pk", entity.PartitionKey);
        Assert.AreEqual(now, entity.CreatedOn);
        Assert.AreEqual(ts, entity.Timestamp);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
    }

    [TestMethod]
    public void EqualityEntitiesWithSameIdAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        // Act & Assert
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityEntitiesWithDifferentIdAreNotEqual()
    {
        // Arrange
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void DomainEventsAddAndClearWorks()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt = new TestEvent();

        // Act
        entity.AddDomainEvent(evt);

        // Assert
        Assert.AreEqual(1, entity.DomainEvents.Count);
        Assert.AreSame(evt, entity.DomainEvents[0]);

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.AreEqual(0, entity.DomainEvents.Count);
    }

    [TestMethod]
    public void SetAuditFieldsModifiedAndDeletedOn()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var mod = DateTime.UtcNow;
        var del = DateTime.UtcNow.AddDays(1);

        // Act
        entity.SetModifiedOn(mod);
        entity.SetDeletedOn(del);

        // Assert
        Assert.AreEqual(mod, entity.ModifiedOn);
        Assert.AreEqual(del, entity.DeletedOn);
    }

    [TestMethod]
    public void GetHashCodeIsConsistentForSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        // Act & Assert
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}

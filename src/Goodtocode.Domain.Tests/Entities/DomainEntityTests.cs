using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityTests
{
    private sealed class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
        public TestEntity() { }
        public TestEntity(Guid id) : base(id) { }
        public TestEntity(Guid id, string partitionKey) : base(id, partitionKey) { }
        public TestEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, createdOn, timestamp) { }
    }

    private sealed class TestEvent : IDomainEvent<TestEntity>
    {
        public TestEntity Item => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }

    [TestMethod]
    public void DefaultConstructorInitializesWithEmptyGuid()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.IsNotNull(entity);
        Assert.AreEqual(Guid.Empty, entity.Id);
        Assert.AreEqual(string.Empty, entity.PartitionKey);
        Assert.AreEqual(default, entity.CreatedOn);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
    }

    [TestMethod]
    public void ConstructorWithIdSetsId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        Assert.IsNotNull(entity);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(string.Empty, entity.PartitionKey);
    }

    [TestMethod]
    public void ConstructorWithIdAndPartitionKeySetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partitionKey = "test-partition";

        // Act
        var entity = new TestEntity(id, partitionKey);

        // Assert
        Assert.IsNotNull(entity);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
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
    public void SetCreatedOnUpdatesCreatedOnProperty()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var createdOn = DateTime.UtcNow;

        // Act
        entity.SetCreatedOn(createdOn);

        // Assert
        Assert.AreEqual(createdOn, entity.CreatedOn);
    }

    [TestMethod]
    public void SetModifiedOnUpdatesModifiedOnProperty()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var modifiedOn = DateTime.UtcNow;

        // Act
        entity.SetModifiedOn(modifiedOn);

        // Assert
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void SetDeletedOnUpdatesDeletedOnProperty()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var deletedOn = DateTime.UtcNow;

        // Act
        entity.SetDeletedOn(deletedOn);

        // Assert
        Assert.AreEqual(deletedOn, entity.DeletedOn);
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
    public void DomainEventsCanAddMultipleEvents()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt1 = new TestEvent();
        var evt2 = new TestEvent();

        // Act
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);

        // Assert
        Assert.AreEqual(2, entity.DomainEvents.Count);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
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
    public void EqualitySameReferenceIsEqual()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsTrue(entity.Equals(entity));
        Assert.IsTrue(entity == entity);
    }

    [TestMethod]
    public void EqualityWithNullReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsFalse(entity.Equals(null));
        Assert.IsFalse(entity == null);
        Assert.IsTrue(entity != null);
    }

    [TestMethod]
    public void EqualityBothNullReturnsTrue()
    {
        // Arrange
        TestEntity? a = null;
        TestEntity? b = null;

        // Act & Assert
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityWithEmptyGuidReturnsFalse()
    {
        // Arrange
        var a = new TestEntity(Guid.Empty);
        var b = new TestEntity(Guid.Empty);

        // Act & Assert
        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void EqualityWithDifferentTypeReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var notAnEntity = new object();

        // Act & Assert
        Assert.IsFalse(entity.Equals(notAnEntity));
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

    [TestMethod]
    public void GetHashCodeIsDifferentForDifferentIds()
    {
        // Arrange
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeIsConsistentAcrossMultipleCalls()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        var hash1 = entity.GetHashCode();
        var hash2 = entity.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }
}

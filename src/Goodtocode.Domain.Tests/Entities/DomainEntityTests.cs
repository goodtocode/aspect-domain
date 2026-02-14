using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;
using Goodtocode.Domain.Tests.TestHelpers;

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

    private sealed class TestEvent(DomainEntityTests.TestEntity item) : IDomainEvent<TestEntity>
    {
        public TestEntity Item { get; set; } = item;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    [TestMethod]
    public void DefaultConstructorInitializesWithEmptyGuid()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.IsNotNull(entity);
        Assert.AreNotEqual(Guid.Empty, entity.Id);
        Assert.AreEqual(entity.Id.ToString(), entity.PartitionKey);
        Assert.AreNotEqual(default, entity.CreatedOn);
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
        Assert.AreEqual(id.ToString(), entity.PartitionKey);
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
    public void ConstructorWithPartitionKeyOnlyMaintainsPartitionKey()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partitionKey = "cosmos-partition-key";

        // Act
        var entity = new TestEntity(id, partitionKey);

        // Assert
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(string.Empty, entity.Name);
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
    public void FullConstructorSetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partitionKey = "full-partition";
        var createdOn = DateTime.UtcNow;
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);

        // Assert
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
    }

    [TestMethod]
    public void TimestampIsSetByDefault()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act
        var entity = new TestEntity();
        var afterCreation = DateTimeOffset.UtcNow;

        // Assert
        Assert.IsGreaterThanOrEqualTo(beforeCreation, entity.Timestamp);
        Assert.IsLessThanOrEqualTo(afterCreation, entity.Timestamp);
    }

    [TestMethod]
    public void AuditFieldsCanBeAccessedViaProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partitionKey = "pk";
        var createdOn = DateTime.UtcNow;
        var timestamp = DateTimeOffset.UtcNow;
        var entity = new TestEntity(id, partitionKey, createdOn, timestamp);

        // Act & Assert
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
        Assert.AreEqual(partitionKey, entity.PartitionKey);
        Assert.AreEqual(id, entity.Id);
    }

    [TestMethod]
    public void DomainEventsAddAndClearWorks()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt = new TestEvent(entity);

        // Act
        entity.AddDomainEvent(evt);

        // Assert
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreSame(evt, entity.DomainEvents[0]);

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCanAddMultipleEvents()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt1 = new TestEvent(entity);
        var evt2 = new TestEvent(entity);

        // Act
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);

        // Assert
        Assert.HasCount(2, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
    }

    [TestMethod]
    public void DomainEventsCollectionIsReadOnly()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsInstanceOfType<IReadOnlyList<IDomainEvent<TestEntity>>>(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCollectionStartsEmpty()
    {
        // Arrange & Act
        var entity = new TestEntity(Guid.NewGuid());

        // Assert
        Assert.IsNotNull(entity.DomainEvents);
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCanBeAddedAndClearedMultipleTimes()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt1 = new TestEvent(entity);
        var evt2 = new TestEvent(entity);

        // Act & Assert - First round
        entity.AddDomainEvent(evt1);
        Assert.HasCount(1, entity.DomainEvents);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);

        // Act & Assert - Second round
        entity.AddDomainEvent(evt2);
        Assert.HasCount(1, entity.DomainEvents);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsMaintainOrderOfAddition()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt1 = new TestEvent(entity) { OccurredOn = DateTime.UtcNow };
        var evt2 = new TestEvent(entity) { OccurredOn = DateTime.UtcNow.AddSeconds(1) };
        var evt3 = new TestEvent(entity) { OccurredOn = DateTime.UtcNow.AddSeconds(2) };

        // Act
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        entity.AddDomainEvent(evt3);

        // Assert
        Assert.HasCount(3, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
        Assert.AreSame(evt3, entity.DomainEvents[2]);
    }

    [TestMethod]
    public void DomainEventsNotAffectedByAuditFieldUpdates()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var evt = new TestEvent(entity);
        entity.AddDomainEvent(evt);

        // Act
        entity.MarkModified();
        entity.MarkDeleted();

        // Assert
        Assert.HasCount(1, entity.DomainEvents, "Domain events should not be affected by audit field updates");
        Assert.AreSame(evt, entity.DomainEvents[0]);
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
    public void EqualitySameIdDifferentPropertiesAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestEntity(id) { Name = "Entity A" };
        var b = new TestEntity(id) { Name = "Entity B" };

        // Act & Assert
        Assert.IsTrue(a.Equals(b), "Entities with same Id should be equal regardless of other properties");
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void EqualitySameIdDifferentPartitionKeysAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestEntity(id, "partition-a");
        var b = new TestEntity(id, "partition-b");

        // Act & Assert
        Assert.IsTrue(a.Equals(b), "Entities with same Id should be equal regardless of partition key");
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void EqualitySameReferenceIsEqual()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsTrue(entity.Equals(entity));
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
    public void DomainEventPropertiesAreCorrectlySet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var occurredOn = DateTime.UtcNow.AddHours(1);
        var domainEvent = new TestEvent(entity) { OccurredOn = occurredOn };

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        Assert.HasCount(1, entity.DomainEvents);
        var eventFromEntity = entity.DomainEvents[0];
        Assert.AreEqual(entity, eventFromEntity.Item);
        Assert.AreEqual(occurredOn, eventFromEntity.OccurredOn, "Domain event OccurredOn should match the set value");
    }

    [TestMethod]
    public void MarkModifiedSetsModifiedOn()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        entity.MarkModified();

        // Assert
        Assert.IsNotNull(entity.ModifiedOn);
    }

    [TestMethod]
    public void MarkModifiedSetsModifiedOnToUtcNow()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        Assert.IsNull(entity.ModifiedOn, "ModifiedOn should be null initially");

        // Act
        var before = DateTime.UtcNow;
        entity.MarkModified();
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsNotNull(entity.ModifiedOn, "ModifiedOn should be set after MarkModified");
        Assert.IsTrue(entity.ModifiedOn >= before && entity.ModifiedOn <= after, "ModifiedOn should be set to a value between before and after");
    }

    [TestMethod]
    public void MarkModifiedUpdatesModifiedOnToMoreRecentValue()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        entity.MarkModified();
        var firstModified = entity.ModifiedOn;
        Assert.IsNotNull(firstModified);

        // Act
        System.Threading.Thread.Sleep(10); // Ensure time passes
        entity.MarkModified();
        var secondModified = entity.ModifiedOn;

        // Assert
        Assert.IsNotNull(secondModified);
#pragma warning disable MSTEST0037 // Use proper 'Assert' methods
        Assert.IsTrue(secondModified > firstModified, "ModifiedOn should be updated to a more recent value");
#pragma warning restore MSTEST0037 // Use proper 'Assert' methods
    }

    [TestMethod]
    public void MarkDeletedSetsDeletedOn()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        entity.MarkDeleted();

        // Assert
        Assert.IsNotNull(entity.DeletedOn);
    }

    [TestMethod]
    public void MarkDeletedSetsDeletedOnToUtcNowIfNull()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        Assert.IsNull(entity.DeletedOn, "DeletedOn should be null initially");

        // Act
        var before = DateTime.UtcNow;
        entity.MarkDeleted();
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsNotNull(entity.DeletedOn, "DeletedOn should be set after MarkDeleted");
        Assert.IsTrue(entity.DeletedOn >= before && entity.DeletedOn <= after, "DeletedOn should be set to a value between before and after");
    }

    [TestMethod]
    public void MarkDeletedDoesNotOverwriteExistingDeletedOn()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        entity.MarkDeleted();
        var firstDeletedOn = entity.DeletedOn;
        Assert.IsNotNull(firstDeletedOn);

        // Act
        System.Threading.Thread.Sleep(10); // Ensure time passes
        entity.MarkDeleted();
        var secondDeletedOn = entity.DeletedOn;

        // Assert
        Assert.AreEqual(firstDeletedOn, secondDeletedOn, "DeletedOn should not be overwritten if already set");
    }

    [TestMethod]
    public void MarkUndeletedClearsDeletedOn()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        entity.MarkDeleted();
        Assert.IsNotNull(entity.DeletedOn);

        // Act
        entity.MarkUndeleted();

        // Assert
        Assert.IsNull(entity.DeletedOn);
    }
}

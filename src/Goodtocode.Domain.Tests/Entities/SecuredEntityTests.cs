using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;
using Goodtocode.Domain.Tests.TestHelpers;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityTests
{
    private sealed class TestSecuredEntity : SecuredEntity<TestSecuredEntity>
    {
        public TestSecuredEntity() { }
        public TestSecuredEntity(Guid id) : base(id) { }
        public TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId) : base(id, ownerId, tenantId) { }
    }

    private sealed class TestSecuredEvent(SecuredEntityTests.TestSecuredEntity item) : IDomainEvent<TestSecuredEntity>
    {
        public TestSecuredEntity Item { get; set; } = item;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    [TestMethod]
    public void DefaultConstructorInitializesToEmptyGuids()
    {
        // Arrange & Act
        var entity = new TestSecuredEntity();

        // Assert
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
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestSecuredEntity(id);

        // Assert
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
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var entity = new TestSecuredEntity(id, ownerId, tenantId);

        // Assert
        Assert.IsNotNull(entity);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreEqual(id, entity.Id);
    }

    [TestMethod]
    public void PartitionKeyReturnsTenantIdAsString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId);

        // Act & Assert
        Assert.AreEqual(tenantId.ToString(), entity.PartitionKey);
    }

    [TestMethod]
    public void ChangeOwnerUpdatesOwnerId()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var ownerId = Guid.NewGuid();

        // Act
        entity.ChangeOwner(ownerId);

        // Assert
        Assert.AreEqual(ownerId, entity.OwnerId);
    }

    [TestMethod]
    public void ChangeTenantUpdatesTenantId()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var tenantId = Guid.NewGuid();

        // Act
        entity.ChangeTenant(tenantId);

        // Assert
        Assert.AreEqual(tenantId, entity.TenantId);
    }

    [TestMethod]
    public void MarkCreatedSetsCreatedByOnce()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var userId = Guid.NewGuid();

        // Act
        entity.MarkCreated(userId);

        // Assert
        Assert.AreEqual(userId, entity.CreatedBy);

        // Act
        var anotherUser = Guid.NewGuid();
        entity.MarkCreated(anotherUser);

        // Assert
        Assert.AreEqual(userId, entity.CreatedBy, "CreatedBy should not change after first set");
    }

    [TestMethod]
    public void MarkModifiedSetsModifiedBy()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var userId = Guid.NewGuid();

        // Act
        entity.MarkModified(userId);

        // Assert
        Assert.AreEqual(userId, entity.ModifiedBy);

        // Act
        var anotherUser = Guid.NewGuid();
        entity.MarkModified(anotherUser);

        // Assert
        Assert.AreEqual(anotherUser, entity.ModifiedBy, "ModifiedBy should update to latest");
    }

    [TestMethod]
    public void MarkDeletedSetsDeletedByOnce()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var userId = Guid.NewGuid();

        // Act
        entity.MarkDeleted(userId);

        // Assert
        Assert.AreEqual(userId, entity.DeletedBy);

        // Act
        var anotherUser = Guid.NewGuid();
        entity.MarkDeleted(anotherUser);

        // Assert
        Assert.AreEqual(userId, entity.DeletedBy, "DeletedBy should not change after first set");
    }

    [TestMethod]
    public void EqualityEntitiesWithSameIdAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(id, ownerId, tenantId);
        var b = new TestSecuredEntity(id, ownerId, tenantId);

        // Act & Assert
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityEntitiesWithDifferentIdAreNotEqual()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId);
        var b = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId);

        // Act & Assert
        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void EqualityWithNullReturnsFalse()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsFalse(entity.Equals(null));
        Assert.IsFalse(entity == null);
        Assert.IsTrue(entity != null);
    }

    [TestMethod]
    public void EqualityBothNullReturnsTrue()
    {
        // Arrange
        TestSecuredEntity? a = null;
        TestSecuredEntity? b = null;

        // Act & Assert
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void GetHashCodeIsConsistentForSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());
        var b = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeIsDifferentForDifferentIds()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId);
        var b = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId);

        // Act & Assert
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeIsConsistentAcrossMultipleCalls()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var hash1 = entity.GetHashCode();
        var hash2 = entity.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void InheritedPropertiesFromDomainEntityAreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId);

        // Act & Assert
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(entity.PartitionKey, entity.TenantId.ToString());
    }

    [TestMethod]
    public void DomainEventsAddAndClearWorks()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var evt = new TestSecuredEvent(entity);

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
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var evt1 = new TestSecuredEvent(entity);
        var evt2 = new TestSecuredEvent(entity);

        // Act
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);

        // Assert
        Assert.HasCount(2, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
    }

    [TestMethod]
    public void DomainEventsWorkWithSecurityContext()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId);
        var evt = new TestSecuredEvent(entity);

        // Act
        entity.AddDomainEvent(evt);

        // Assert
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreSame(entity, entity.DomainEvents[0].Item);
    }
}

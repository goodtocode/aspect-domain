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

    private sealed class TestSecuredEvent : IDomainEvent<TestSecuredEntity>
    {
        public TestSecuredEntity Item { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        public TestSecuredEvent(TestSecuredEntity item)
        {
            Item = item;
        }
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
    public void SetOwnerIdUpdatesOwnerId()
    {
        // Arrange
        var entity = new TestSecuredEntity();
        var ownerId = Guid.NewGuid();

        // Act
        entity.SetOwnerId(ownerId);

        // Assert
        Assert.AreEqual(ownerId, entity.OwnerId);
    }

    [TestMethod]
    public void SetTenantIdUpdatesTenantId()
    {
        // Arrange
        var entity = new TestSecuredEntity();
        var tenantId = Guid.NewGuid();

        // Act
        entity.SetTenantId(tenantId);

        // Assert
        Assert.AreEqual(tenantId, entity.TenantId);
    }

    [TestMethod]
    public void SetSecurityContextUpdatesBothOwnerIdAndTenantId()
    {
        // Arrange
        var entity = new TestSecuredEntity();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        entity.SetSecurityContext(ownerId, tenantId);

        // Assert
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
    }

    [TestMethod]
    public void SetSecurityContextCanOverrideExistingValues()
    {
        // Arrange
        var initialOwnerId = Guid.NewGuid();
        var initialTenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(Guid.NewGuid(), initialOwnerId, initialTenantId);
        
        var newOwnerId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();

        // Act
        entity.SetSecurityContext(newOwnerId, newTenantId);

        // Assert
        Assert.AreEqual(newOwnerId, entity.OwnerId);
        Assert.AreEqual(newTenantId, entity.TenantId);
        Assert.AreNotEqual(initialOwnerId, entity.OwnerId);
        Assert.AreNotEqual(initialTenantId, entity.TenantId);
    }

    [TestMethod]
    public void PropertiesAreProtectedSet()
    {
        // Arrange & Act & Assert
        var ownerIdProp = typeof(TestSecuredEntity).GetProperty("OwnerId");
        Assert.IsNotNull(ownerIdProp, "OwnerId property should exist.");
        Assert.IsNotNull(ownerIdProp.SetMethod, "OwnerId property should have a set method.");
        Assert.IsTrue(ownerIdProp.SetMethod.IsFamily, "OwnerId setter should be protected.");

        var tenantIdProp = typeof(TestSecuredEntity).GetProperty("TenantId");
        Assert.IsNotNull(tenantIdProp, "TenantId property should exist.");
        Assert.IsNotNull(tenantIdProp.SetMethod, "TenantId property should have a set method.");
        Assert.IsTrue(tenantIdProp.SetMethod.IsFamily, "TenantId setter should be protected.");
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
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var evt1 = new TestSecuredEvent(entity);
        var evt2 = new TestSecuredEvent(entity);

        // Act
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);

        // Assert
        Assert.AreEqual(2, entity.DomainEvents.Count);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
    }

    [TestMethod]
    public void SetModifiedOnUpdatesModifiedOnProperty()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
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
        var entity = new TestSecuredEntity(Guid.NewGuid());
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
        var entity = new TestSecuredEntity(Guid.NewGuid());
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
    public void EqualitySameIdDifferentSecurityContextAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());
        var b = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.IsTrue(a.Equals(b), "Entities with same Id should be equal regardless of security context");
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void EqualitySameReferenceIsEqual()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());

        // Act & Assert
        Assert.IsTrue(entity.Equals(entity));
        Assert.IsTrue(entity == entity);
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
    public void EqualityWithEmptyGuidReturnsFalse()
    {
        // Arrange
        var a = new TestSecuredEntity(Guid.Empty);
        var b = new TestSecuredEntity(Guid.Empty);

        // Act & Assert
        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void EqualityWithDifferentTypeReturnsFalse()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var notAnEntity = new object();

        // Act & Assert
        Assert.IsFalse(entity.Equals(notAnEntity));
    }

    [TestMethod]
    public void GetHashCodeIsConsistentForSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());
        var b = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "Entities with same Id should have same hash code");
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

        // Act
        var entity = new TestSecuredEntity(id, ownerId, tenantId);

        // Assert
        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(entity.PartitionKey, entity.TenantId.ToString());
        Assert.AreEqual(default, entity.CreatedOn);
        Assert.IsNull(entity.ModifiedOn);
        Assert.IsNull(entity.DeletedOn);
        Assert.IsNotNull(entity.Timestamp);
    }

    [TestMethod]
    public void SecurityContextMaintainedWhenAuditFieldsUpdated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId);
        var modifiedOn = DateTime.UtcNow;

        // Act
        entity.SetModifiedOn(modifiedOn);

        // Assert
        Assert.AreEqual(ownerId, entity.OwnerId, "OwnerId should remain unchanged");
        Assert.AreEqual(tenantId, entity.TenantId, "TenantId should remain unchanged");
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void AuditFieldsMaintainedWhenSecurityContextUpdated()
    {
        // Arrange
        var entity = new TestSecuredEntity(Guid.NewGuid());
        var createdOn = entity.CreatedOn; // Capture the auto-set CreatedOn value
        
        var newOwnerId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();

        // Act
        entity.SetSecurityContext(newOwnerId, newTenantId);

        // Assert
        Assert.AreEqual(newOwnerId, entity.OwnerId);
        Assert.AreEqual(newTenantId, entity.TenantId);
        Assert.AreEqual(createdOn, entity.CreatedOn, "CreatedOn should remain unchanged");
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
        Assert.AreEqual(1, entity.DomainEvents.Count);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreSame(entity, entity.DomainEvents[0].Item);
    }
}

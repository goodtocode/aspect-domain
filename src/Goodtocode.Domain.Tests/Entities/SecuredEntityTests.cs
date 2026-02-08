using Goodtocode.Domain.Entities;

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
        // OwnerId and TenantId are protected set, so cannot be set outside the class
        // This test verifies compile-time protection
        var ownerIdProp = typeof(TestSecuredEntity).GetProperty("OwnerId");
        Assert.IsNotNull(ownerIdProp, "OwnerId property should exist.");
        Assert.IsNotNull(ownerIdProp.SetMethod, "OwnerId property should have a set method.");
        Assert.IsTrue(ownerIdProp.SetMethod.IsFamily, "OwnerId setter should be protected.");

        var tenantIdProp = typeof(TestSecuredEntity).GetProperty("TenantId");
        Assert.IsNotNull(tenantIdProp, "TenantId property should exist.");
        Assert.IsNotNull(tenantIdProp.SetMethod, "TenantId property should have a set method.");
        Assert.IsTrue(tenantIdProp.SetMethod.IsFamily, "TenantId setter should be protected.");
    }
}

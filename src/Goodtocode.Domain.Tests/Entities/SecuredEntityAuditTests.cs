using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Tests.TestHelpers;

namespace Goodtocode.Domain.Tests.Entities;

/// <summary>
/// Tests for audit fields (CreatedBy, ModifiedBy, DeletedBy) and PartitionKey projection in SecuredEntity.
/// Validates security-by-design implementation for RLS and audit tracking.
/// </summary>
[TestClass]
public class SecuredEntityAuditTests
{
    #region Test Entity

    /// <summary>
    /// Test implementation of SecuredEntity for audit field testing
    /// </summary>
    private sealed class TestSecuredEntity : SecuredEntity<TestSecuredEntity>
    {
        public string Name { get; private set; } = string.Empty;

        // EF constructor
        private TestSecuredEntity() { }

        public TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, string name)
            : base(id, ownerId, tenantId)
        {
            Name = name;
        }

        public void UpdateName(string newName)
        {
            Name = newName;
        }
    }

    #endregion

    #region Deletion and Undeletion Tests

    [TestMethod]
    public void MarkDeletedShouldBeForwardOnly()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");
        // Act
        entity.MarkDeleted();
        var deletedOnFirst = entity.DeletedOn;
        entity.MarkDeleted(); // Should not overwrite

        // Assert
        Assert.AreEqual(deletedOnFirst, entity.DeletedOn, "DeletedOn should not be overwritten by subsequent deletes");
    }

    [TestMethod]
    public void MarkUndeletedShouldRestoreEntity()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");
        entity.MarkDeleted();

        // Act
        entity.MarkUndeleted();

        // Assert
        Assert.IsNull(entity.DeletedOn, "DeletedOn should be null after MarkUndeleted");
    }

    [TestMethod]
    public void MarkUndeletedWhenNotDeletedShouldBeNoOp()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkUndeleted();

        // Assert
        Assert.IsNull(entity.DeletedOn, "MarkUndeleted should be a no-op if not deleted");
    }

    [TestMethod]
    public void MarkDeletedAfterUndeleteShouldWork()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");
        entity.MarkDeleted();
        entity.MarkUndeleted();

        // Act
        entity.MarkDeleted();

        // Assert
        Assert.IsNotNull(entity.DeletedOn, "Should allow re-deletion after undeletion");
    }

    #endregion

    #region CreatedBy Tests

    [TestMethod]
    public void CreatedByShouldBeSetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkCreated(userId);

        // Assert
        Assert.AreEqual(userId, entity.CreatedBy);
    }

    [TestMethod]
    public void CreatedByShouldNotBeNullWhenSet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkCreated(userId);

        // Assert
        Assert.AreNotEqual(Guid.Empty, entity.CreatedBy);
    }

    [TestMethod]
    public void CreatedByShouldRemainUnchangedAfterModification()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");
        entity.MarkCreated(creatorId);

        // Act

        entity.UpdateName("Modified Name");
        entity.MarkModified();
        entity.SetModifiedBy(modifierId);

        // Assert
        Assert.AreEqual(creatorId, entity.CreatedBy, "CreatedBy should not change after modification");
        Assert.AreNotEqual(entity.CreatedBy, entity.ModifiedBy, "CreatedBy and ModifiedBy should be different");
    }

    #endregion

    #region ModifiedBy Tests

    [TestMethod]
    public void ModifiedByShouldBeNullInitially()
    {
        // Arrange & Act
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Assert
        Assert.IsNull(entity.ModifiedBy, "ModifiedBy should be null for new entities");
    }

    [TestMethod]
    public void ModifiedByShouldBeSetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkModified();
        entity.SetModifiedBy(userId);

        // Assert
        Assert.AreEqual(userId, entity.ModifiedBy);
    }

    [TestMethod]
    public void ModifiedByShouldUpdateOnMultipleModifications()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkModified();
        entity.SetModifiedBy(user1);
        entity.MarkModified();
        entity.SetModifiedBy(user2);

        // Assert
        Assert.AreEqual(user2, entity.ModifiedBy, "ModifiedBy should reflect the most recent modifier");
    }

    [TestMethod]
    public void ModifiedByCanBeSetToNull()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");
        entity.MarkModified();
        entity.SetModifiedBy(Guid.NewGuid());

        // Act
        entity.SetModifiedBy(null);

        // Assert
        Assert.IsNull(entity.ModifiedBy);
    }

    #endregion

    #region DeletedBy Tests

    [TestMethod]
    public void DeletedByShouldBeNullInitially()
    {
        // Arrange & Act
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Assert
        Assert.IsNull(entity.DeletedBy, "DeletedBy should be null for non-deleted entities");
    }

    [TestMethod]
    public void DeletedByShouldBeSetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act
        entity.MarkDeleted();
        entity.SetDeletedBy(userId);

        // Assert
        Assert.AreEqual(userId, entity.DeletedBy);
        Assert.IsNotNull(entity.DeletedOn, "DeletedOn should be set when entity is deleted");
    }

    [TestMethod]
    public void DeletedByShouldTrackSoftDelete()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Act - Simulate soft delete
        entity.MarkDeleted();
        entity.SetDeletedBy(deleterId);

        // Assert
        Assert.IsNotNull(entity.DeletedOn, "DeletedOn should indicate soft delete");
        Assert.AreEqual(deleterId, entity.DeletedBy, "DeletedBy should track who deleted the entity");
    }

    #endregion

    #region PartitionKey Tests

    [TestMethod]
    public void PartitionKeyShouldDefaultToTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        
        // Act
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            tenantId,
            "Test Entity");

        // Assert
        Assert.AreEqual(tenantId.ToString(), entity.PartitionKey);
    }

    [TestMethod]
    public void PartitionKeyShouldUpdateWhenTenantIdChanges()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            originalTenantId,
            "Test Entity");

        // Act
        entity.SetTenantId(newTenantId);

        // Assert
        Assert.AreEqual(newTenantId.ToString(), entity.PartitionKey);
    }

    [TestMethod]
    public void PartitionKeyShouldProvideDataIsolationForMultipleTenants()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        
        var entity1 = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), tenant1, "Tenant 1 Entity");
        var entity2 = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), tenant2, "Tenant 2 Entity");

        // Assert
        Assert.AreNotEqual(entity1.PartitionKey, entity2.PartitionKey, 
            "Different tenants should have different partition keys");
        Assert.AreEqual(tenant1.ToString(), entity1.PartitionKey);
        Assert.AreEqual(tenant2.ToString(), entity2.PartitionKey);
    }

    [TestMethod]
    public void PartitionKeyShouldBeConsistentAcrossMultipleReads()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            tenantId,
            "Test Entity");

        // Act
        var partitionKey1 = entity.PartitionKey;
        var partitionKey2 = entity.PartitionKey;
        var partitionKey3 = entity.PartitionKey;

        // Assert
        Assert.AreEqual(partitionKey1, partitionKey2);
        Assert.AreEqual(partitionKey2, partitionKey3);
        Assert.AreEqual(tenantId.ToString(), partitionKey1);
    }

    #endregion

    #region Integration Tests - Complete Audit Trail

    [TestMethod]
    public void CompleteAuditTrailShouldTrackEntityLifecycle()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act - Create
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            ownerId,
            tenantId,
            "Initial Name");
        entity.MarkCreated(creatorId);

        // Act - Modify
        entity.UpdateName("Modified Name");
        entity.MarkModified();
        entity.SetModifiedBy(modifierId);

        // Act - Delete
        entity.MarkDeleted();
        entity.SetDeletedBy(deleterId);

        // Assert - Complete audit trail
        Assert.AreEqual(creatorId, entity.CreatedBy, "Should track creator");
        Assert.AreEqual(modifierId, entity.ModifiedBy, "Should track modifier");
        Assert.AreEqual(deleterId, entity.DeletedBy, "Should track deleter");
        Assert.IsNotNull(entity.ModifiedOn, "Should have modification timestamp");
        Assert.IsNotNull(entity.DeletedOn, "Should have deletion timestamp");
    }

    [TestMethod]
    public void MultiTenantScenarioShouldIsolateDataByPartitionKey()
    {
        // Arrange - Multiple tenants
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Act - Create entities for different tenants
        var entity1 = new TestSecuredEntity(
            Guid.NewGuid(),
            user1,
            tenant1,
            "Tenant 1 Data");
        entity1.MarkCreated(user1);

        var entity2 = new TestSecuredEntity(
            Guid.NewGuid(),
            user2,
            tenant2,
            "Tenant 2 Data");
        entity2.MarkCreated(user2);

        // Assert - Data isolation
        Assert.AreEqual(tenant1.ToString(), entity1.PartitionKey);
        Assert.AreEqual(tenant2.ToString(), entity2.PartitionKey);
        Assert.AreNotEqual(entity1.PartitionKey, entity2.PartitionKey);
        Assert.AreEqual(tenant1, entity1.TenantId);
        Assert.AreEqual(tenant2, entity2.TenantId);
    }

    [TestMethod]
    public void RLSScenarioShouldSupportQueryFiltering()
    {
        // Arrange - Simulating EF Core query filter scenario
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();

        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            ownerId,
            tenantId,
            "Secure Data");
        entity.MarkCreated(ownerId);

        // Assert - Fields available for RLS filtering
        Assert.AreEqual(tenantId, entity.TenantId, "TenantId required for tenant-level RLS");
        Assert.AreEqual(ownerId, entity.OwnerId, "OwnerId required for user-level RLS");
        Assert.AreEqual(tenantId.ToString(), entity.PartitionKey, "PartitionKey required for physical isolation");
        
        // Simulate query filter logic
        bool passesTenantFilter = entity.TenantId == tenantId;
        bool passesOwnerFilter = entity.OwnerId == ownerId;
        bool passesUnauthorizedFilter = entity.OwnerId == unauthorizedUserId;

        Assert.IsTrue(passesTenantFilter, "Should pass tenant filter");
        Assert.IsTrue(passesOwnerFilter, "Should pass owner filter");
        Assert.IsFalse(passesUnauthorizedFilter, "Should fail unauthorized user filter");
    }

    [TestMethod]
    public void AuditFieldsShouldSupportComplianceRequirements()
    {
        // Arrange - Simulating compliance scenario (GDPR, SOX, etc.)
        var dataControllerUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            dataControllerUserId,
            tenantId,
            "Sensitive Data");

        // Act - Track who accessed/modified data
        entity.MarkCreated(dataControllerUserId);

        var auditorUserId = Guid.NewGuid();
        entity.MarkModified();
        entity.SetModifiedBy(auditorUserId);

        // Assert - Complete audit trail for compliance
        Assert.IsNotNull(entity.ModifiedBy, "Must track who modified data");
        Assert.IsNotNull(entity.ModifiedOn, "Must track when data was modified");
        
        // Verify different users
        Assert.AreNotEqual(entity.CreatedBy, entity.ModifiedBy, 
            "Should distinguish between creator and modifier");
    }

    [TestMethod]
    public void SoftDeleteShouldPreserveAuditTrail()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "To Be Deleted");
        entity.MarkCreated(creatorId);

        // Act - Soft delete
        entity.MarkDeleted();
        entity.SetDeletedBy(deleterId);

        // Assert - Audit trail preserved after deletion
        Assert.AreEqual(creatorId, entity.CreatedBy, "Creator should be preserved after deletion");
        Assert.IsNotNull(entity.DeletedOn, "Deletion timestamp should be set");
        Assert.AreEqual(deleterId, entity.DeletedBy, "Deleter should be tracked");
        
        // Verify entity is marked as deleted but data is preserved
        Assert.IsNotNull(entity.DeletedOn, "Entity should be marked as deleted");
        Assert.AreEqual("To Be Deleted", entity.Name, "Entity data should be preserved for audit");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void AuditFieldsShouldHandleEmptyGuids()
    {
        // Arrange & Act
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Entity");

        // Assert - Default values
        Assert.AreEqual(Guid.Empty, entity.CreatedBy, "CreatedBy should default to Guid.Empty");
        Assert.IsNull(entity.ModifiedBy, "ModifiedBy should default to null");
        Assert.IsNull(entity.DeletedBy, "DeletedBy should default to null");
    }

    [TestMethod]
    public void PartitionKeyShouldHandleEmptyTenantId()
    {
        // Arrange
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "Test Entity");

        // Act & Assert
        Assert.AreEqual(Guid.Empty.ToString(), entity.PartitionKey);
    }

    [TestMethod]
    public void AuditFieldsShouldAllowSameUserForAllOperations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            "Test Entity");

        // Act - Same user performs all operations
        entity.MarkCreated(userId);
        entity.MarkModified();
        entity.SetModifiedBy(userId);
        entity.MarkDeleted();
        entity.SetDeletedBy(userId);

        // Assert
        Assert.AreEqual(userId, entity.CreatedBy);
        Assert.AreEqual(userId, entity.ModifiedBy);
        Assert.AreEqual(userId, entity.DeletedBy);
    }

    #endregion
}

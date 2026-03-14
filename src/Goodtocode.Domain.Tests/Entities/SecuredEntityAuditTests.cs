using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityAuditTests
{
    private sealed class TestSecuredEntity : SecuredEntity<TestSecuredEntity>
    {
        public string Name { get; private set; } = string.Empty;
        public TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, ownerId, tenantId, createdBy, createdOn, timestamp) { }
        public TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, string name)
            : base(id, ownerId, tenantId, Guid.Empty, default, default) { Name = name; }
        public void UpdateName(string newName) => Name = newName;
    }

    [TestMethod]
    public void MarkCreatedSetsCreatedByOnce()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var userId = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkCreated(userId, createdOn);
        Assert.AreEqual(userId, entity.CreatedBy);
        var anotherUser = Guid.NewGuid();
        entity.MarkCreated(anotherUser, createdOn.AddDays(1));
        Assert.AreEqual(userId, entity.CreatedBy, "CreatedBy should not change after first set");
    }

    [TestMethod]
    public void MarkModifiedSetsModifiedBy()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var userId = Guid.NewGuid();
        var modifiedOn1 = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(userId, modifiedOn1);
        Assert.AreEqual(userId, entity.ModifiedBy);
        var anotherUser = Guid.NewGuid();
        var modifiedOn2 = new DateTime(2024, 1, 3, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(anotherUser, modifiedOn2);
        Assert.AreEqual(anotherUser, entity.ModifiedBy, "ModifiedBy should update to latest");
    }

    [TestMethod]
    public void MarkDeletedSetsDeletedByOnce()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var userId = Guid.NewGuid();
        var deletedOn1 = new DateTime(2024, 1, 4, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(userId, deletedOn1);
        Assert.AreEqual(userId, entity.DeletedBy);
        var anotherUser = Guid.NewGuid();
        var deletedOn2 = new DateTime(2024, 1, 5, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(anotherUser, deletedOn2);
        Assert.AreEqual(userId, entity.DeletedBy, "DeletedBy should not change after first set");
    }

    [TestMethod]
    public void MarkDeletedShouldBeForwardOnly()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        var userId = Guid.NewGuid();
        var deletedOn1 = new DateTime(2024, 1, 6, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(userId, deletedOn1);
        var deletedOnFirst = entity.DeletedOn;
        entity.MarkDeleted(Guid.NewGuid(), deletedOn1.AddDays(1));
        Assert.AreEqual(deletedOnFirst, entity.DeletedOn, "DeletedOn should not be overwritten by subsequent deletes");
    }

    [TestMethod]
    public void MarkUndeletedShouldRestoreEntity()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        var userId = Guid.NewGuid();
        var deletedOn = new DateTime(2024, 1, 7, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkDeleted(userId, deletedOn);
        entity.MarkUndeleted();
        Assert.IsNull(entity.DeletedOn, "DeletedOn should be null after MarkUndeleted");
    }

    [TestMethod]
    public void MarkUndeletedWhenNotDeletedShouldBeNoOp()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkUndeleted();
        Assert.IsNull(entity.DeletedOn, "MarkUndeleted should be a no-op if not deleted");
    }

    [TestMethod]
    public void CreatedByShouldBeSetCorrectly()
    {
        var userId = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkCreated(userId, createdOn);
        Assert.AreEqual(userId, entity.CreatedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
    }

    [TestMethod]
    public void CreatedByShouldNotBeNullWhenSet()
    {
        var userId = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 11, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkCreated(userId, createdOn);
        Assert.AreNotEqual(Guid.Empty, entity.CreatedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
    }

    [TestMethod]
    public void CreatedByShouldRemainUnchangedAfterModification()
    {
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 12, 12, 0, 0, DateTimeKind.Utc);
        var modifiedOn = new DateTime(2024, 1, 13, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkCreated(creatorId, createdOn);
        entity.UpdateName("Modified Name");
        entity.MarkModified(modifierId, modifiedOn);
        Assert.AreEqual(creatorId, entity.CreatedBy, "CreatedBy should not change after modification");
        Assert.AreNotEqual(entity.CreatedBy, entity.ModifiedBy, "CreatedBy and ModifiedBy should be different");
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void ModifiedByShouldBeNullInitially()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        Assert.IsNull(entity.ModifiedBy, "ModifiedBy should be null for new entities");
    }

    [TestMethod]
    public void ModifiedByShouldBeSetCorrectly()
    {
        var userId = Guid.NewGuid();
        var modifiedOn = new DateTime(2024, 1, 14, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkModified(userId, modifiedOn);
        Assert.AreEqual(userId, entity.ModifiedBy);
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void ModifiedByShouldUpdateOnMultipleModifications()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var modifiedOn1 = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var modifiedOn2 = new DateTime(2024, 1, 16, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkModified(user1, modifiedOn1);
        entity.MarkModified(user2, modifiedOn2);
        Assert.AreEqual(user2, entity.ModifiedBy, "ModifiedBy should reflect the most recent modifier");
        Assert.AreEqual(modifiedOn2, entity.ModifiedOn);
    }

    [TestMethod]
    public void DeletedByShouldBeNullInitially()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        Assert.IsNull(entity.DeletedBy, "DeletedBy should be null for non-deleted entities");
    }

    [TestMethod]
    public void DeletedByShouldBeSetCorrectly()
    {
        var userId = Guid.NewGuid();
        var deletedOn = new DateTime(2024, 1, 17, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkDeleted(userId, deletedOn);
        Assert.AreEqual(userId, entity.DeletedBy);
        Assert.IsNotNull(entity.DeletedOn, "DeletedOn should be set when entity is deleted");
        Assert.AreEqual(deletedOn, entity.DeletedOn);
    }

    [TestMethod]
    public void DeletedByShouldTrackSoftDelete()
    {
        var deleterId = Guid.NewGuid();
        var deletedOn = new DateTime(2024, 1, 18, 12, 0, 0, DateTimeKind.Utc);
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        entity.MarkDeleted(deleterId, deletedOn);
        Assert.IsNotNull(entity.DeletedOn, "DeletedOn should indicate soft delete");
        Assert.AreEqual(deleterId, entity.DeletedBy, "DeletedBy should track who deleted the entity");
        Assert.AreEqual(deletedOn, entity.DeletedOn);
    }

    [TestMethod]
    public void AuditFieldsShouldSupportComplianceRequirements()
    {
        var dataControllerUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(Guid.NewGuid(), dataControllerUserId, tenantId, "Sensitive Data");
        var createdOn = new DateTime(2024, 1, 19, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkCreated(dataControllerUserId, createdOn);
        var auditorUserId = Guid.NewGuid();
        var modifiedOn = new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(auditorUserId, modifiedOn);
        Assert.IsNotNull(entity.ModifiedBy, "Must track who modified data");
        Assert.IsNotNull(entity.ModifiedOn, "Must track when data was modified");
        Assert.AreNotEqual(entity.CreatedBy, entity.ModifiedBy, "Should distinguish between creator and modifier");
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
    }

    [TestMethod]
    public void SoftDeleteShouldPreserveAuditTrail()
    {
        var creatorId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "To Be Deleted");
        var createdOn = new DateTime(2024, 1, 21, 12, 0, 0, DateTimeKind.Utc);
        var deletedOn = new DateTime(2024, 1, 22, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkCreated(creatorId, createdOn);
        entity.MarkDeleted(deleterId, deletedOn);
        Assert.AreEqual(creatorId, entity.CreatedBy, "Creator should be preserved after deletion");
        Assert.IsNotNull(entity.DeletedOn, "Deletion timestamp should be set");
        Assert.AreEqual(deleterId, entity.DeletedBy, "Deleter should be tracked");
        Assert.IsNotNull(entity.DeletedOn, "Entity should be marked as deleted");
        Assert.AreEqual("To Be Deleted", entity.Name, "Entity data should be preserved for audit");
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(deletedOn, entity.DeletedOn);
    }

    [TestMethod]
    public void AuditFieldsShouldHandleEmptyGuids()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Entity");
        Assert.AreEqual(Guid.Empty, entity.CreatedBy, "CreatedBy should default to Guid.Empty");
        Assert.IsNull(entity.ModifiedBy, "ModifiedBy should default to null");
        Assert.IsNull(entity.DeletedBy, "DeletedBy should default to null");
    }

    [TestMethod]
    public void AuditFieldsShouldAllowSameUserForAllOperations()
    {
        var userId = Guid.NewGuid();
        var entity = new TestSecuredEntity(Guid.NewGuid(), userId, Guid.NewGuid(), "Test Entity");
        var createdOn = new DateTime(2024, 1, 23, 12, 0, 0, DateTimeKind.Utc);
        var modifiedOn = new DateTime(2024, 1, 24, 12, 0, 0, DateTimeKind.Utc);
        var deletedOn = new DateTime(2024, 1, 25, 12, 0, 0, DateTimeKind.Utc);
        entity.MarkCreated(userId, createdOn);
        entity.MarkModified(userId, modifiedOn);
        entity.MarkDeleted(userId, deletedOn);
        Assert.AreEqual(userId, entity.CreatedBy);
        Assert.AreEqual(userId, entity.ModifiedBy);
        Assert.AreEqual(userId, entity.DeletedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(modifiedOn, entity.ModifiedOn);
        Assert.AreEqual(deletedOn, entity.DeletedOn);
    }
}

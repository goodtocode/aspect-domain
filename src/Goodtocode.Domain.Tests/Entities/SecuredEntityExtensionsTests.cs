using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityExtensionsTests
{
    private sealed class TestSecuredEntity : ISecurable
    {
        public Guid OwnerId { get; set; }
        public Guid TenantId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PartitionKey { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public void ChangeOwner(Guid value) => OwnerId = value;
        public void ChangeTenant(Guid value) => TenantId = value;

        public void MarkCreated(Guid ownerId, DateTime createdOn)
        {
            if (CreatedBy != Guid.Empty || CreatedOn != default)
                return;
            CreatedBy = ownerId;
            CreatedOn = createdOn;
        }
        public void MarkModified(Guid ownerId, DateTime modifiedOn)
        {
            ModifiedBy = ownerId;
            ModifiedOn = modifiedOn;
        }
        public void MarkDeleted(Guid ownerId, DateTime deletedOn)
        {
            if (DeletedBy.HasValue || DeletedOn.HasValue)
                return;
            DeletedBy = ownerId;
            DeletedOn = deletedOn;
        }
    }

    [TestMethod]
    public void IsOwnerFiltersEntitiesByOwnerId()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var entities = new[]
        {
            new TestSecuredEntity { OwnerId = ownerId },
            new TestSecuredEntity { OwnerId = Guid.NewGuid() }
        }.AsQueryable();

        // Act
        var result = entities.WhereOwner(ownerId).ToList();

        // Assert
        Assert.HasCount(1, result);
        Assert.AreEqual(ownerId, result[0].OwnerId);
    }

    [TestMethod]
    public void WhereOwnerFiltersEntitiesByOwnerId()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var entities = new[]
        {
            new TestSecuredEntity { OwnerId = ownerId },
            new TestSecuredEntity { OwnerId = Guid.NewGuid() }
        }.AsQueryable();

        // Act
        var result = entities.WhereOwner(ownerId).ToList();

        // Assert
        Assert.HasCount(1, result);
        Assert.AreEqual(ownerId, result[0].OwnerId);
    }

    [TestMethod]
    public void IsAuthorizedReturnsTrueForOwnerOrTenant()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity { OwnerId = ownerId, TenantId = tenantId };

        // Act & Assert
        Assert.IsTrue(entity.IsAuthorized(ownerId, Guid.NewGuid()));
        Assert.IsTrue(entity.IsAuthorized(Guid.NewGuid(), tenantId));
        Assert.IsTrue(entity.IsAuthorized(ownerId, tenantId));
    }

    [TestMethod]
    public void IsAuthorizedReturnsFalseForNeitherOwnerNorTenant()
    {
        // Arrange
        var entity = new TestSecuredEntity { OwnerId = Guid.NewGuid(), TenantId = Guid.NewGuid() };

        // Act & Assert
        Assert.IsFalse(entity.IsAuthorized(Guid.NewGuid(), Guid.NewGuid()));
    }

    [TestMethod]
    public void WhereAuthorizedFiltersEntitiesByTenantOrOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entities = new[]
        {
            new TestSecuredEntity { OwnerId = ownerId, TenantId = Guid.NewGuid() },
            new TestSecuredEntity { OwnerId = Guid.NewGuid(), TenantId = tenantId },
            new TestSecuredEntity { OwnerId = Guid.NewGuid(), TenantId = Guid.NewGuid() }
        }.AsQueryable();

        // Act
        var result = entities.WhereAuthorized(tenantId, ownerId).ToList();

        // Assert
        Assert.HasCount(2, result);
        Assert.Contains(x => x.OwnerId == ownerId, result);
        Assert.Contains(x => x.TenantId == tenantId, result);
    }
}

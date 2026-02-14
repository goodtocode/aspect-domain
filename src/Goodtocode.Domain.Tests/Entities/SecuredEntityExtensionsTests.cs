using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityExtensionsTests
{
#pragma warning disable CA1822
    private sealed class TestSecuredEntity : ISecurable
    {
        public Guid OwnerId { get; set; }
        public Guid TenantId { get; set; }

        // Implement IDomainEntity members
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PartitionKey { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; } = Guid.Empty;
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;


#pragma warning disable IDE0060 // Remove unused parameter
        public void AddDomainEvent(IDomainEvent<TestSecuredEntity> domainEvent)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // No-op for test stub
        }

        public void ClearDomainEvents()
        {
            // No-op for test stub
        }

        public void ChangeOwner(Guid value)
        {
            OwnerId = value;
        }

        public void ChangeTenant(Guid value)
        {
            TenantId = value;
        }

        public void MarkModified()
        {
            ModifiedOn = DateTime.UtcNow;
        }

        public void MarkDeleted()
        {
            DeletedOn = DateTime.UtcNow;
        }

        public void MarkUndeleted()
        {
            DeletedOn = null;
            DeletedBy = null;
        }

        public void MarkCreated(Guid ownerId)
        {
            CreatedOn = DateTime.UtcNow;
            CreatedBy = ownerId;
        }

        public void MarkModified(Guid ownerId)
        {
            ModifiedOn = DateTime.UtcNow;
            ModifiedBy = ownerId;
        }

        public void MarkDeleted(Guid ownerId)
        {
            DeletedOn = DateTime.UtcNow;
            DeletedBy = ownerId;
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
#pragma warning restore CA1822
}

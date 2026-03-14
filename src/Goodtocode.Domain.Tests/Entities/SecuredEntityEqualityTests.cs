using Goodtocode.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityEqualityTests
{
    private sealed class TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp) : SecuredEntity<TestSecuredEntity>(id, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
    }

    [TestMethod]
    public void EqualityEntitiesWithSameIdAreEqual()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        var b = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void EqualityEntitiesWithDifferentIdAreNotEqual()
    {
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId, Guid.Empty, default, default);
        var b = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId, Guid.Empty, default, default);
        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void EqualityWithNullReturnsFalse()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        Assert.IsFalse(entity.Equals(null));
        Assert.IsFalse(entity == null);
        Assert.IsTrue(entity != null);
    }

    [TestMethod]
    public void EqualityBothNullReturnsTrue()
    {
        TestSecuredEntity? a = null;
        TestSecuredEntity? b = null;
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void GetHashCodeIsConsistentForSameId()
    {
        var id = Guid.NewGuid();
        var a = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, default, default);
        var b = new TestSecuredEntity(id, Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, default, default);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeIsDifferentForDifferentIds()
    {
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var a = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId, Guid.Empty, default, default);
        var b = new TestSecuredEntity(Guid.NewGuid(), ownerId, tenantId, Guid.Empty, default, default);
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeIsConsistentAcrossMultipleCalls()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, default, default);
        var hash1 = entity.GetHashCode();
        var hash2 = entity.GetHashCode();
        Assert.AreEqual(hash1, hash2);
    }
}

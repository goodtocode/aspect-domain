using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredEntityDomainEventTests
{
    private sealed class TestSecuredEntity(Guid id, Guid ownerId, Guid tenantId, Guid createdBy, DateTime createdOn, DateTimeOffset timestamp) : SecuredEntity<TestSecuredEntity>(id, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
    }
    private sealed class TestSecuredEvent(TestSecuredEntity item) : IDomainEvent<TestSecuredEntity>
    {
        public TestSecuredEntity Item { get; set; } = item;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    [TestMethod]
    public void DomainEventsAddAndClearWorks()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var evt = new TestSecuredEvent(entity);
        entity.AddDomainEvent(evt);
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreSame(evt, entity.DomainEvents[0]);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCanAddMultipleEvents()
    {
        var entity = new TestSecuredEntity(Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty, default, default);
        var evt1 = new TestSecuredEvent(entity);
        var evt2 = new TestSecuredEvent(entity);
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        Assert.HasCount(2, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
    }

    [TestMethod]
    public void DomainEventsWorkWithSecurityContext()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredEntity(id, ownerId, tenantId, Guid.Empty, default, default);
        var evt = new TestSecuredEvent(entity);
        entity.AddDomainEvent(evt);
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreSame(entity, entity.DomainEvents[0].Item);
    }
}

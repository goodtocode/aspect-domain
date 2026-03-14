using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class DomainEntityDomainEventTests
{
    private sealed class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
        public TestEntity() { }
        public TestEntity(Guid id, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, string rowKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, rowKey, createdOn, timestamp) { }
        public TestEntity(Guid id, string partitionKey, DateTime createdOn, DateTimeOffset timestamp)
            : base(id, partitionKey, id.ToString(), createdOn, timestamp) { }        
    }
    private sealed class TestEvent(TestEntity item) : IDomainEvent<TestEntity>
    {
        public TestEntity Item { get; set; } = item;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    [TestMethod]
    public void DomainEventsAddAndClearWorks()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 7, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 7, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var evt = new TestEvent(entity);
        entity.AddDomainEvent(evt);
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreSame(evt, entity.DomainEvents[0]);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCanAddMultipleEvents()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 8, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 8, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var evt1 = new TestEvent(entity);
        var evt2 = new TestEvent(entity);
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        Assert.HasCount(2, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
    }

    [TestMethod]
    public void DomainEventsCollectionIsReadOnly()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 9, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 9, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        Assert.IsInstanceOfType<IReadOnlyList<IDomainEvent<TestEntity>>>(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCollectionStartsEmpty()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        Assert.IsNotNull(entity.DomainEvents);
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsCanBeAddedAndClearedMultipleTimes()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 11, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 11, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var evt1 = new TestEvent(entity);
        var evt2 = new TestEvent(entity);
        entity.AddDomainEvent(evt1);
        Assert.HasCount(1, entity.DomainEvents);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);
        entity.AddDomainEvent(evt2);
        Assert.HasCount(1, entity.DomainEvents);
        entity.ClearDomainEvents();
        Assert.IsEmpty(entity.DomainEvents);
    }

    [TestMethod]
    public void DomainEventsMaintainOrderOfAddition()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 12, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 12, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var evt1 = new TestEvent(entity) { OccurredOn = new DateTime(2024, 1, 12, 12, 0, 1, DateTimeKind.Utc) };
        var evt2 = new TestEvent(entity) { OccurredOn = new DateTime(2024, 1, 12, 12, 0, 2, DateTimeKind.Utc) };
        var evt3 = new TestEvent(entity) { OccurredOn = new DateTime(2024, 1, 12, 12, 0, 3, DateTimeKind.Utc) };
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        entity.AddDomainEvent(evt3);
        Assert.HasCount(3, entity.DomainEvents);
        Assert.AreSame(evt1, entity.DomainEvents[0]);
        Assert.AreSame(evt2, entity.DomainEvents[1]);
        Assert.AreSame(evt3, entity.DomainEvents[2]);
    }

    [TestMethod]
    public void DomainEventsNotAffectedByAuditFieldUpdates()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 13, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 13, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var evt = new TestEvent(entity);
        entity.AddDomainEvent(evt);
        var modifiedOn = new DateTime(2024, 1, 13, 13, 0, 0, DateTimeKind.Utc);
        var deletedOn = new DateTime(2024, 1, 13, 14, 0, 0, DateTimeKind.Utc);
        entity.MarkModified(modifiedOn);
        entity.MarkDeleted(deletedOn);
        Assert.HasCount(1, entity.DomainEvents);
        Assert.AreSame(evt, entity.DomainEvents[0]);
    }

    [TestMethod]
    public void DomainEventPropertiesAreCorrectlySet()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTime(2024, 1, 17, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = new DateTimeOffset(2024, 1, 17, 12, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity(id, createdOn, timestamp);
        var occurredOn = new DateTime(2024, 1, 17, 13, 0, 0, DateTimeKind.Utc);
        var domainEvent = new TestEvent(entity) { OccurredOn = occurredOn };
        entity.AddDomainEvent(domainEvent);
        Assert.HasCount(1, entity.DomainEvents);
        var eventFromEntity = entity.DomainEvents[0];
        Assert.AreEqual(entity, eventFromEntity.Item);
        Assert.AreEqual(occurredOn, eventFromEntity.OccurredOn);
    }
}

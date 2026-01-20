using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Events;

[TestClass]
public sealed class DomainEventTests
{
    private class TestEntity : DomainEntity<TestEntity>
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestCreatedEvent(DomainEventTests.TestEntity entity) : IDomainEvent<TestEntity>
    {
        public TestEntity Entity { get; } = entity;

        public TestEntity Item => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }

    [TestMethod]
    public void DomainEventCanBeCreatedWithEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };

        // Act
        var evt = new TestCreatedEvent(entity);

        // Assert
        Assert.AreSame(entity, evt.Entity);
    }

    [TestMethod]
    public void DomainEventCanBeAddedToEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };
        var evt = new TestCreatedEvent(entity);

        // Act
        entity.AddDomainEvent(evt);

        // Assert
        Assert.AreEqual(1, entity.DomainEvents.Count);
        Assert.AreSame(evt, entity.DomainEvents[0]);
    }

    [TestMethod]
    public void DomainEventCanBeClearedFromEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };
        var evt = new TestCreatedEvent(entity);
        entity.AddDomainEvent(evt);

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.AreEqual(0, entity.DomainEvents.Count);
    }
}

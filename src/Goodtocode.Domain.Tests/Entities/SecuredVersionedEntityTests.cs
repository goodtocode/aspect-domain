using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Goodtocode.Domain.Entities;

namespace Goodtocode.Domain.Tests.Entities;

[TestClass]
public sealed class SecuredVersionedEntityTests
{
    private sealed class TestSecuredVersionedEntity : SecuredVersionedEntity<TestSecuredVersionedEntity>
    {
        public TestSecuredVersionedEntity() : base() { }

        public TestSecuredVersionedEntity(
            Guid id,
            string canonicalKey,
            string? rowKey,
            Guid ownerId,
            Guid tenantId,
            Guid createdBy,
            DateTime createdOn,
            DateTimeOffset timestamp,
            int version,
            Guid? previousVersionId,
            bool isLatest,
            bool isPinned,
            bool isFrozen)
            : base(id, canonicalKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp,
                   version, previousVersionId, isLatest, isPinned, isFrozen) { }

        protected override TestSecuredVersionedEntity CreateNextVersionCore() =>
            new(Guid.NewGuid(), CanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                Version + 1, Id, isLatest: true, isPinned: false, isFrozen: false);

        protected override TestSecuredVersionedEntity CreateSuccessorCore(string newCanonicalKey) =>
            new(Guid.NewGuid(), newCanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                1, null, isLatest: true, isPinned: false, isFrozen: false);
    }

    private static TestSecuredVersionedEntity CreateV1(
        string canonicalKey = "key-001",
        bool isLatest = true,
        bool isPinned = false,
        bool isFrozen = false)
    {
        return new TestSecuredVersionedEntity(
            Guid.NewGuid(), canonicalKey, null,
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, DateTimeOffset.UtcNow,
            1, null, isLatest, isPinned, isFrozen);
    }

    [TestMethod]
    public void DefaultConstructorInitializesDefaults()
    {
        var entity = new TestSecuredVersionedEntity();
        Assert.AreEqual(1, entity.Version);
        Assert.IsNull(entity.PreviousVersionId);
        Assert.IsTrue(entity.IsLatest);
        Assert.IsFalse(entity.IsPinned);
        Assert.IsFalse(entity.IsFrozen);
        Assert.AreEqual(string.Empty, entity.CanonicalKey);
    }

    [TestMethod]
    public void ParameterizedConstructorSetsAllProperties()
    {
        var id = Guid.NewGuid();
        var canonicalKey = "product-123";
        var rowKey = Uuid7.New().ToString();
        var ownerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdOn = DateTime.UtcNow;
        var timestamp = DateTimeOffset.UtcNow;
        var previousVersionId = Guid.NewGuid();

        var entity = new TestSecuredVersionedEntity(
            id, canonicalKey, rowKey, ownerId, tenantId, createdBy,
            createdOn, timestamp, 5, previousVersionId,
            isLatest: false, isPinned: true, isFrozen: true);

        Assert.AreEqual(id, entity.Id);
        Assert.AreEqual(canonicalKey, entity.CanonicalKey);
        Assert.AreEqual(rowKey, entity.RowKey);
        Assert.AreEqual($"{tenantId}:{canonicalKey}", entity.PartitionKey);
        Assert.AreEqual(ownerId, entity.OwnerId);
        Assert.AreEqual(tenantId, entity.TenantId);
        Assert.AreEqual(createdBy, entity.CreatedBy);
        Assert.AreEqual(createdOn, entity.CreatedOn);
        Assert.AreEqual(timestamp, entity.Timestamp);
        Assert.AreEqual(5, entity.Version);
        Assert.AreEqual(previousVersionId, entity.PreviousVersionId);
        Assert.IsFalse(entity.IsLatest);
        Assert.IsTrue(entity.IsPinned);
        Assert.IsTrue(entity.IsFrozen);
    }

    [TestMethod]
    public void PartitionKeyIsComputedFromTenantIdAndCanonicalKey()
    {
        var tenantId = Guid.NewGuid();
        var entity = new TestSecuredVersionedEntity(
            Guid.NewGuid(), "invoice-456", null, Guid.NewGuid(), tenantId, Guid.NewGuid(),
            DateTime.UtcNow, DateTimeOffset.UtcNow, 1, null, true, false, false);

        Assert.AreEqual($"{tenantId}:invoice-456", entity.PartitionKey);
    }

    [TestMethod]
    public void RowKeyIsUuid7AndDifferentFromId()
    {
        var entity = CreateV1();
        Assert.IsTrue(Guid.TryParse(entity.RowKey, out _));
        Assert.AreNotEqual(entity.Id.ToString(), entity.RowKey);
    }

    [TestMethod]
    public void FreezeSetsIsFrozenTrue()
    {
        var entity = CreateV1();
        Assert.IsFalse(entity.IsFrozen);
        entity.Freeze();
        Assert.IsTrue(entity.IsFrozen);
    }

    [TestMethod]
    public void MarkNotLatestSetsIsLatestFalse()
    {
        var entity = CreateV1(isLatest: true);
        Assert.IsTrue(entity.IsLatest);
        entity.MarkNotLatest();
        Assert.IsFalse(entity.IsLatest);
    }

    [TestMethod]
    public void CreateNextVersionReturnsNewRowWithIncrementedVersion()
    {
        var entity = CreateV1(canonicalKey: "contract-789");
        var next = entity.CreateNextVersion();

        Assert.AreNotEqual(entity.Id, next.Id);
        Assert.AreNotEqual(entity.RowKey, next.RowKey);
        Assert.AreEqual(entity.CanonicalKey, next.CanonicalKey);
        Assert.AreEqual(entity.TenantId, next.TenantId);
        Assert.AreEqual(entity.OwnerId, next.OwnerId);
        Assert.AreEqual(2, next.Version);
        Assert.AreEqual(entity.Id, next.PreviousVersionId);
        Assert.IsTrue(next.IsLatest);
        Assert.IsFalse(next.IsPinned);
        Assert.IsFalse(next.IsFrozen);
        Assert.AreEqual(entity.PartitionKey, next.PartitionKey);
    }

    [TestMethod]
    public void CreateNextVersionRowKeyIsUuid7()
    {
        var entity = CreateV1();
        var next = entity.CreateNextVersion();
        Assert.IsTrue(Guid.TryParse(next.RowKey, out _));
        Assert.AreNotEqual(next.Id.ToString(), next.RowKey);
    }

    [TestMethod]
    public void CreateNextVersionOnFrozenEntityThrows()
    {
        var entity = CreateV1(isFrozen: true);
        bool threw = false;
        try { entity.CreateNextVersion(); }
        catch (InvalidOperationException) { threw = true; }
        Assert.IsTrue(threw, "Expected InvalidOperationException for frozen entity.");
    }

    [TestMethod]
    public void CreateSuccessorReturnsNewSeriesWithResetVersion()
    {
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var entity = new TestSecuredVersionedEntity(
            Guid.NewGuid(), "old-key", null, ownerId, tenantId, Guid.NewGuid(),
            DateTime.UtcNow, DateTimeOffset.UtcNow, 3, Guid.NewGuid(),
            isLatest: true, isPinned: true, isFrozen: true);

        var successor = entity.CreateSuccessor("new-key");

        Assert.AreNotEqual(entity.Id, successor.Id);
        Assert.AreNotEqual(entity.RowKey, successor.RowKey);
        Assert.AreEqual("new-key", successor.CanonicalKey);
        Assert.AreEqual(tenantId, successor.TenantId);
        Assert.AreEqual(ownerId, successor.OwnerId);
        Assert.AreEqual(1, successor.Version);
        Assert.IsNull(successor.PreviousVersionId);
        Assert.IsTrue(successor.IsLatest);
        Assert.IsFalse(successor.IsPinned);
        Assert.IsFalse(successor.IsFrozen);
        Assert.AreEqual($"{tenantId}:new-key", successor.PartitionKey);
    }

    [TestMethod]
    public void CreateSuccessorIsAllowedWhenFrozen()
    {
        var entity = CreateV1(isFrozen: true);
        Assert.IsTrue(entity.IsFrozen);
        var successor = entity.CreateSuccessor("successor-key");
        Assert.IsNotNull(successor);
        Assert.AreEqual(1, successor.Version);
        Assert.IsFalse(successor.IsFrozen);
    }

    [TestMethod]
    public void CreateSuccessorWithNullOrWhiteSpaceKeyThrows()
    {
        var entity = CreateV1();
        bool threwNull = false;
        try { entity.CreateSuccessor(null!); }
        catch (ArgumentException) { threwNull = true; }
        Assert.IsTrue(threwNull, "Expected ArgumentException for null canonical key.");

        bool threwWhitespace = false;
        try { entity.CreateSuccessor("   "); }
        catch (ArgumentException) { threwWhitespace = true; }
        Assert.IsTrue(threwWhitespace, "Expected ArgumentException for whitespace canonical key.");
    }

    [TestMethod]
    public void MultipleVersionsChainCorrectly()
    {
        var v1 = CreateV1(canonicalKey: "order-001");
        var v2 = v1.CreateNextVersion();
        var v3 = v2.CreateNextVersion();

        Assert.AreEqual(1, v1.Version);
        Assert.AreEqual(2, v2.Version);
        Assert.AreEqual(3, v3.Version);
        Assert.AreEqual(v1.Id, v2.PreviousVersionId);
        Assert.AreEqual(v2.Id, v3.PreviousVersionId);
        Assert.AreEqual(v1.PartitionKey, v3.PartitionKey);
    }

    [TestMethod]
    public void CreateNextVersionDoesNotMutateOriginalEntity()
    {
        var entity = CreateV1(canonicalKey: "doc-001");
        var originalId = entity.Id;
        var originalRowKey = entity.RowKey;
        var originalVersion = entity.Version;
        var originalIsLatest = entity.IsLatest;

        entity.CreateNextVersion();

        Assert.AreEqual(originalId, entity.Id);
        Assert.AreEqual(originalRowKey, entity.RowKey);
        Assert.AreEqual(originalVersion, entity.Version);
        Assert.AreEqual(originalIsLatest, entity.IsLatest);
    }

    [TestMethod]
    public void FreezeIsIdempotent()
    {
        var entity = CreateV1();
        entity.Freeze();
        entity.Freeze();
        Assert.IsTrue(entity.IsFrozen);
    }

    [TestMethod]
    public void CreateSuccessorDoesNotMutateOriginalEntity()
    {
        var entity = CreateV1(canonicalKey: "contract-001");
        var originalId = entity.Id;
        var originalRowKey = entity.RowKey;
        var originalCanonicalKey = entity.CanonicalKey;
        var originalVersion = entity.Version;
        var originalIsLatest = entity.IsLatest;

        entity.CreateSuccessor("contract-002");

        Assert.AreEqual(originalId, entity.Id);
        Assert.AreEqual(originalRowKey, entity.RowKey);
        Assert.AreEqual(originalCanonicalKey, entity.CanonicalKey);
        Assert.AreEqual(originalVersion, entity.Version);
        Assert.AreEqual(originalIsLatest, entity.IsLatest);
    }

    [TestMethod]
    public void CreateNextVersionFromPinnedEntityNewVersionIsNotPinned()
    {
        var entity = CreateV1(isPinned: true);
        Assert.IsTrue(entity.IsPinned);

        var next = entity.CreateNextVersion();

        Assert.IsFalse(next.IsPinned);
    }

    [TestMethod]
    public void ImplementsIVersionableInterface()
    {
        var entity = CreateV1(canonicalKey: "thing-001");
        IVersionable versionable = entity;

        Assert.AreEqual(entity.CanonicalKey, versionable.CanonicalKey);
        Assert.AreEqual(entity.Version, versionable.Version);
        Assert.AreEqual(entity.PreviousVersionId, versionable.PreviousVersionId);
        Assert.AreEqual(entity.IsLatest, versionable.IsLatest);
        Assert.AreEqual(entity.IsPinned, versionable.IsPinned);
        Assert.AreEqual(entity.IsFrozen, versionable.IsFrozen);
    }

    private sealed class HookedSecuredVersionedEntity : SecuredVersionedEntity<HookedSecuredVersionedEntity>
    {
        public (HookedSecuredVersionedEntity? Predecessor, HookedSecuredVersionedEntity? Successor)? LastHookArgs { get; private set; }

        public HookedSecuredVersionedEntity() : base() { }
        public HookedSecuredVersionedEntity(
            Guid id,
            string canonicalKey,
            string? rowKey,
            Guid ownerId,
            Guid tenantId,
            Guid createdBy,
            DateTime createdOn,
            DateTimeOffset timestamp,
            int version,
            Guid? previousVersionId,
            bool isLatest,
            bool isPinned,
            bool isFrozen)
            : base(id, canonicalKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp,
                   version, previousVersionId, isLatest, isPinned, isFrozen) { }

        protected override HookedSecuredVersionedEntity CreateNextVersionCore() =>
            new(Guid.NewGuid(), CanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                Version + 1, Id, isLatest: true, isPinned: false, isFrozen: false);

        protected override HookedSecuredVersionedEntity CreateSuccessorCore(string newCanonicalKey) =>
            new(Guid.NewGuid(), newCanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                1, null, isLatest: true, isPinned: false, isFrozen: false);

        protected override void OnSuccessorCreated(HookedSecuredVersionedEntity predecessor, HookedSecuredVersionedEntity successor)
        {
            LastHookArgs = (predecessor, successor);
        }
    }

    [TestMethod]
    public void OnSuccessorCreatedIsCalledWithCorrectArguments()
    {
        var entity = new HookedSecuredVersionedEntity(
            Guid.NewGuid(), "hook-key", null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, DateTimeOffset.UtcNow, 1, null, true, false, false);

        var successor = entity.CreateSuccessor("hook-key-successor");

        Assert.IsNotNull(entity.LastHookArgs);
        Assert.AreSame(entity, entity.LastHookArgs.Value.Predecessor);
        Assert.AreSame(successor, entity.LastHookArgs.Value.Successor);
        Assert.AreEqual("hook-key-successor", successor.CanonicalKey);
    }

    private sealed class NoopSecuredVersionedEntity : SecuredVersionedEntity<NoopSecuredVersionedEntity>
    {
        public NoopSecuredVersionedEntity() : base() { }
        public NoopSecuredVersionedEntity(
            Guid id,
            string canonicalKey,
            string? rowKey,
            Guid ownerId,
            Guid tenantId,
            Guid createdBy,
            DateTime createdOn,
            DateTimeOffset timestamp,
            int version,
            Guid? previousVersionId,
            bool isLatest,
            bool isPinned,
            bool isFrozen)
            : base(id, canonicalKey, rowKey, ownerId, tenantId, createdBy, createdOn, timestamp,
                   version, previousVersionId, isLatest, isPinned, isFrozen) { }

        protected override NoopSecuredVersionedEntity CreateNextVersionCore() =>
            new(Guid.NewGuid(), CanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                Version + 1, Id, isLatest: true, isPinned: false, isFrozen: false);

        protected override NoopSecuredVersionedEntity CreateSuccessorCore(string newCanonicalKey) =>
            new(Guid.NewGuid(), newCanonicalKey, null, OwnerId, TenantId, CreatedBy,
                DateTime.UtcNow, DateTimeOffset.UtcNow,
                1, null, isLatest: true, isPinned: false, isFrozen: false);
        // No override for OnSuccessorCreated
    }

    [TestMethod]
    public void OnSuccessorCreatedDefaultImplementationDoesNotThrowOrAffectState()
    {
        var entity = new NoopSecuredVersionedEntity(
            Guid.NewGuid(), "noop-key", null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, DateTimeOffset.UtcNow, 1, null, true, false, false);

        var successor = entity.CreateSuccessor("noop-key-successor");
        Assert.IsNotNull(successor);
        Assert.AreEqual("noop-key-successor", successor.CanonicalKey);
        // No exception, no state change expected
    }
}

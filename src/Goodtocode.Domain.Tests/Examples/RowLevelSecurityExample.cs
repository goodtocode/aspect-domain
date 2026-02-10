using Goodtocode.Domain.Entities;
using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Tests.Examples;

/// <summary>
/// Example demonstrating Row-Level Security (RLS) patterns with audit fields.
/// Shows how to implement security-by-design using TenantId, OwnerId, and audit fields.
/// </summary>
[TestClass]
[DoNotParallelize]
public class RowLevelSecurityExample
{
    #region Domain Entity with RLS

    /// <summary>
    /// Document entity demonstrating RLS implementation
    /// </summary>
    private sealed class Document : SecuredEntity<Document>
    {
        public string Title { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public bool IsPublic { get; private set; }

        // EF constructor
        private Document() { }

        public Document(Guid id, Guid ownerId, Guid tenantId, string title, string content, bool isPublic = false)
            : base(id, ownerId, tenantId)
        {
            Title = title;
            Content = content;
            IsPublic = isPublic;
        }

        public void UpdateContent(string newContent, Guid modifiedBy)
        {
            Content = newContent;
            SetModifiedOn(DateTime.UtcNow);
            SetModifiedBy(modifiedBy);
        }

        public void SoftDelete(Guid deletedBy)
        {
            SetDeletedOn(DateTime.UtcNow);
            SetDeletedBy(deletedBy);
        }
    }

    #endregion

    #region Simulated DbContext with RLS Query Filters

    /// <summary>
    /// Simulates EF Core DbContext with RLS query filters
    /// </summary>
    private sealed class SecuredDbContext
    {
        private readonly Guid _currentUserId;
        private readonly Guid _currentTenantId;
        private static readonly List<Document> _allDocuments = [];

        public SecuredDbContext(Guid currentUserId, Guid currentTenantId)
        {
            _currentUserId = currentUserId;
            _currentTenantId = currentTenantId;
        }

        /// <summary>
        /// Clears all documents - used for test isolation
        /// </summary>
        public static void ClearAllDocuments() => _allDocuments.Clear();

        /// <summary>
        /// Simulates EF Core query filter: WHERE TenantId == @TenantId AND DeletedOn IS NULL
        /// </summary>
        public IEnumerable<Document> Documents => _allDocuments
            .Where(d => d.TenantId == _currentTenantId)
            .Where(d => d.DeletedOn == null);

        /// <summary>
        /// Simulates EF Core query filter with owner restriction
        /// </summary>
        public IEnumerable<Document> MyDocuments => _allDocuments
            .Where(d => d.TenantId == _currentTenantId)
            .Where(d => d.OwnerId == _currentUserId || d.IsPublic)
            .Where(d => d.DeletedOn == null);

        /// <summary>
        /// Simulates documents by partition key (for Cosmos DB scenarios)
        /// </summary>
        public IEnumerable<Document> DocumentsByPartition(string partitionKey) => _allDocuments
            .Where(d => d.PartitionKey == partitionKey)
            .Where(d => d.TenantId == _currentTenantId)
            .Where(d => d.DeletedOn == null);

        /// <summary>
        /// Admin access - bypasses RLS for audit/compliance
        /// </summary>
        public IEnumerable<Document> AllDocumentsIncludingDeleted => _allDocuments;

        public void Add(Document document)
        {
            _allDocuments.Add(document);
        }

        public Task<int> SaveChangesAsync() => Task.FromResult(_allDocuments.Count);
    }

    #endregion

    #region Test Infrastructure

    [TestInitialize]
    public void TestInitialize()
    {
        // Clear shared state before each test to prevent test interference
        SecuredDbContext.ClearAllDocuments();
    }

    #endregion

    #region Test Scenarios

    [TestMethod]
    public void RLSTenantIsolationShouldPreventCrossTenantAccess()
    {
        // Arrange - Two tenants
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var dbContext1 = new SecuredDbContext(user1, tenant1);
        var dbContext2 = new SecuredDbContext(user2, tenant2);

        // Create documents for different tenants
        var doc1 = new Document(Guid.NewGuid(), user1, tenant1, "Tenant 1 Doc", "Secret content 1");
        doc1.SetCreatedOn(DateTime.UtcNow);
        doc1.SetCreatedBy(user1);
        dbContext1.Add(doc1);

        var doc2 = new Document(Guid.NewGuid(), user2, tenant2, "Tenant 2 Doc", "Secret content 2");
        doc2.SetCreatedOn(DateTime.UtcNow);
        doc2.SetCreatedBy(user2);
        dbContext2.Add(doc2);

        // Act - Query with RLS filters
        var tenant1Documents = dbContext1.Documents.ToList();
        var tenant2Documents = dbContext2.Documents.ToList();

        // Assert - Tenant isolation
        Assert.AreEqual(1, tenant1Documents.Count, "Tenant 1 should only see their documents");
        Assert.AreEqual(1, tenant2Documents.Count, "Tenant 2 should only see their documents");
        Assert.AreEqual("Tenant 1 Doc", tenant1Documents[0].Title);
        Assert.AreEqual("Tenant 2 Doc", tenant2Documents[0].Title);
    }

    [TestMethod]
    public void RLSOwnerFilterShouldRestrictAccessToOwnedDocuments()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();

        var dbContextOwner1 = new SecuredDbContext(owner1, tenantId);

        // Create documents with different owners in same tenant
        var doc1 = new Document(Guid.NewGuid(), owner1, tenantId, "Owner 1 Doc", "Private content", false);
        doc1.SetCreatedBy(owner1);
        dbContextOwner1.Add(doc1);

        var doc2 = new Document(Guid.NewGuid(), owner2, tenantId, "Owner 2 Doc", "Private content", false);
        doc2.SetCreatedBy(owner2);
        dbContextOwner1.Add(doc2);

        // Act - Query with owner filter
        var owner1Docs = dbContextOwner1.MyDocuments.ToList();

        // Assert - Owner restriction
        Assert.AreEqual(1, owner1Docs.Count, "Should only see own documents");
        Assert.AreEqual(owner1, owner1Docs[0].OwnerId);
        Assert.AreEqual(owner1, owner1Docs[0].CreatedBy);
    }

    [TestMethod]
    public void RLSPublicDocumentsShouldBeVisibleToAllUsersInTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();

        var dbContextOwner2 = new SecuredDbContext(owner2, tenantId);

        // Create public document by owner1
        var publicDoc = new Document(Guid.NewGuid(), owner1, tenantId, "Public Doc", "Public content", true);
        publicDoc.SetCreatedBy(owner1);
        dbContextOwner2.Add(publicDoc);

        var privateDoc = new Document(Guid.NewGuid(), owner1, tenantId, "Private Doc", "Private content", false);
        privateDoc.SetCreatedBy(owner1);
        dbContextOwner2.Add(privateDoc);

        // Act - Query as owner2
        var owner2Docs = dbContextOwner2.MyDocuments.ToList();

        // Assert - Can see public documents from other owners
        Assert.AreEqual(1, owner2Docs.Count, "Should see public documents");
        Assert.IsTrue(owner2Docs[0].IsPublic);
        Assert.AreEqual(owner1, owner2Docs[0].OwnerId, "Document owned by different user but is public");
    }

    [TestMethod]
    public void RLSSoftDeleteShouldExcludeDeletedDocuments()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        var dbContext = new SecuredDbContext(userId, tenantId);

        var doc = new Document(Guid.NewGuid(), userId, tenantId, "To Delete", "Content");
        doc.SetCreatedOn(DateTime.UtcNow);
        doc.SetCreatedBy(userId);
        dbContext.Add(doc);

        // Act - Soft delete
        doc.SoftDelete(deleterId);
        var visibleDocs = dbContext.Documents.ToList();
        var allDocs = dbContext.AllDocumentsIncludingDeleted.ToList();

        // Assert
        Assert.AreEqual(0, visibleDocs.Count, "Soft-deleted documents should not appear in queries");
        Assert.AreEqual(1, allDocs.Count, "Admin query should see deleted documents");
        Assert.IsNotNull(allDocs[0].DeletedOn);
        Assert.AreEqual(deleterId, allDocs[0].DeletedBy, "Should track who deleted");
    }

    [TestMethod]
    public void PartitionKeyShouldEnableCosmosDbPartitioning()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var dbContext = new SecuredDbContext(user1, tenant1);

        var doc1 = new Document(Guid.NewGuid(), user1, tenant1, "Doc 1", "Content 1");
        var doc2 = new Document(Guid.NewGuid(), user2, tenant2, "Doc 2", "Content 2");
        dbContext.Add(doc1);
        dbContext.Add(doc2);

        // Act - Query by partition key (Cosmos DB scenario)
        var tenant1Docs = dbContext.DocumentsByPartition(tenant1.ToString()).ToList();
        var tenant2Docs = dbContext.DocumentsByPartition(tenant2.ToString()).ToList();

        // Assert
        Assert.AreEqual(1, tenant1Docs.Count);
        Assert.AreEqual(0, tenant2Docs.Count, "Different partition");
        Assert.AreEqual(tenant1.ToString(), doc1.PartitionKey);
        Assert.AreEqual(tenant2.ToString(), doc2.PartitionKey);
    }

    [TestMethod]
    public void AuditTrailShouldTrackCompleteDocumentLifecycle()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        var dbContext = new SecuredDbContext(creatorId, tenantId);

        // Act - Create
        var doc = new Document(Guid.NewGuid(), creatorId, tenantId, "Document", "Original content");
        doc.SetCreatedOn(DateTime.UtcNow);
        doc.SetCreatedBy(creatorId);
        dbContext.Add(doc);

        // Act - Edit
        doc.UpdateContent("Modified content", editorId);

        // Act - Delete
        doc.SoftDelete(deleterId);

        // Assert - Complete audit trail
        Assert.AreEqual(creatorId, doc.CreatedBy, "Track creator");
        Assert.AreEqual(editorId, doc.ModifiedBy, "Track editor");
        Assert.AreEqual(deleterId, doc.DeletedBy, "Track deleter");
        Assert.IsNotNull(doc.CreatedOn);
        Assert.IsNotNull(doc.ModifiedOn);
        Assert.IsNotNull(doc.DeletedOn);

        // Assert - RLS still applies
        var visibleDocs = dbContext.Documents.ToList();
        Assert.AreEqual(0, visibleDocs.Count, "Deleted documents not visible");

        // Assert - Admin can see full audit trail
        var allDocs = dbContext.AllDocumentsIncludingDeleted.ToList();
        Assert.AreEqual(1, allDocs.Count);
        Assert.AreEqual(creatorId, allDocs[0].CreatedBy);
        Assert.AreEqual(editorId, allDocs[0].ModifiedBy);
        Assert.AreEqual(deleterId, allDocs[0].DeletedBy);
    }

    [TestMethod]
    public void SecurityContextShouldSupportMultiTenantScenarios()
    {
        // Arrange - Multi-tenant SaaS scenario
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var tenant1User1 = Guid.NewGuid();
        var tenant1User2 = Guid.NewGuid();
        var tenant2User1 = Guid.NewGuid();

        var dbContext1U1 = new SecuredDbContext(tenant1User1, tenant1);
        var dbContext1U2 = new SecuredDbContext(tenant1User2, tenant1);
        var dbContext2U1 = new SecuredDbContext(tenant2User1, tenant2);

        // Create documents
        var doc1 = new Document(Guid.NewGuid(), tenant1User1, tenant1, "T1U1 Doc", "Content", false);
        doc1.SetCreatedBy(tenant1User1);
        dbContext1U1.Add(doc1);

        var doc2 = new Document(Guid.NewGuid(), tenant1User2, tenant1, "T1U2 Doc", "Content", true); // Public
        doc2.SetCreatedBy(tenant1User2);
        dbContext1U1.Add(doc2);

        var doc3 = new Document(Guid.NewGuid(), tenant2User1, tenant2, "T2U1 Doc", "Content", false);
        doc3.SetCreatedBy(tenant2User1);
        dbContext2U1.Add(doc3);

        // Act & Assert - User 1 in Tenant 1
        var t1u1Docs = dbContext1U1.MyDocuments.ToList();
        Assert.AreEqual(2, t1u1Docs.Count, "Should see own doc + public doc in tenant");

        // Act & Assert - User 2 in Tenant 1
        var t1u2Docs = dbContext1U2.MyDocuments.ToList();
        Assert.AreEqual(1, t1u2Docs.Count, "Should only see public doc");

        // Act & Assert - User 1 in Tenant 2
        var t2u1Docs = dbContext2U1.MyDocuments.ToList();
        Assert.AreEqual(1, t2u1Docs.Count, "Should see own doc in tenant 2, but not tenant 1 docs");
    }

    [TestMethod]
    public void ComplianceScenarioShouldSupportGDPRAuditRequirements()
    {
        // Arrange - GDPR scenario: data subject requests audit trail
        var tenantId = Guid.NewGuid();
        var dataSubjectId = Guid.NewGuid();
        var dataProcessorId = Guid.NewGuid();

        var dbContext = new SecuredDbContext(dataSubjectId, tenantId);

        // Create personal data
        var personalDoc = new Document(
            Guid.NewGuid(),
            dataSubjectId,
            tenantId,
            "Personal Data",
            "Sensitive personal information");
        personalDoc.SetCreatedOn(DateTime.UtcNow.AddDays(-100));
        personalDoc.SetCreatedBy(dataSubjectId);
        dbContext.Add(personalDoc);

        // Data processor modifies data
        personalDoc.UpdateContent("Updated personal information", dataProcessorId);

        // Act - Generate audit report (GDPR Art. 15 - Right to Access)
        var auditReport = new
        {
            DataSubject = personalDoc.OwnerId,
            Tenant = personalDoc.TenantId,
            PartitionKey = personalDoc.PartitionKey,
            Created = new { When = personalDoc.CreatedOn, By = personalDoc.CreatedBy },
            Modified = new { When = personalDoc.ModifiedOn, By = personalDoc.ModifiedBy },
            Deleted = new { When = personalDoc.DeletedOn, By = personalDoc.DeletedBy }
        };

        // Assert - Complete audit trail for compliance
        Assert.AreEqual(dataSubjectId, auditReport.DataSubject);
        Assert.AreEqual(tenantId, auditReport.Tenant);
        Assert.AreEqual(tenantId.ToString(), auditReport.PartitionKey);
        Assert.AreEqual(dataSubjectId, auditReport.Created.By);
        Assert.AreEqual(dataProcessorId, auditReport.Modified.By);
        Assert.IsNotNull(auditReport.Created.When);
        Assert.IsNotNull(auditReport.Modified.When);
        Assert.IsNull(auditReport.Deleted.When, "Not deleted yet");
    }

    #endregion

    #region EF Core Configuration Example

    /// <summary>
    /// Example of how this would be configured in EF Core
    /// </summary>
    [TestMethod]
    public void EFCoreConfigurationExample()
    {
        // This is pseudo-code showing how to configure in actual EF Core DbContext:
        
        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global query filter for RLS
            modelBuilder.Entity<Document>().HasQueryFilter(d => 
                d.TenantId == _currentTenantId &&  // Tenant isolation
                d.DeletedOn == null);               // Soft delete filter

            // Indexes for RLS performance
            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.TenantId, d.OwnerId })
                .HasDatabaseName("IX_Documents_TenantId_OwnerId");

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.PartitionKey)
                .HasDatabaseName("IX_Documents_PartitionKey");

            // Audit fields
            modelBuilder.Entity<Document>()
                .Property(d => d.CreatedBy).IsRequired();
            
            modelBuilder.Entity<Document>()
                .Property(d => d.CreatedOn).IsRequired();

            // For SQL Server RLS (optional, defense-in-depth)
            modelBuilder.Entity<Document>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Documents_TenantId", 
                    "TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)"));
        }
        */

        Assert.IsTrue(true, "This test demonstrates configuration patterns");
    }

    #endregion
}

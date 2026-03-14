// Goodtocode.Domain.Tests\Examples\EfCoreIntegrationExample.cs
using Goodtocode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Goodtocode.Domain.Tests.Examples
{
    /// <summary>
    /// Example DbContext showing how to integrate IAuditable and ISecurable with EF Core.
    /// Copy/adapt this pattern in your infrastructure layer.
    /// </summary>
    public class ExampleDbContext(DbContextOptions options, ICurrentUserContext currentUserContext) : DbContext(options)
    {
        private readonly ICurrentUserContext _currentUserContext = currentUserContext;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            SetSecurityFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var now = DateTime.UtcNow; // For deterministic/idempotent audit
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditable auditable)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        auditable.MarkModified(now);
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        auditable.MarkDeleted(now);
                        entry.State = EntityState.Modified;
                    }
                }
            }
        }

        private void SetSecurityFields()
        {
            if (_currentUserContext is null) return;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is ISecurable securable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (securable.OwnerId == Guid.Empty)
                            securable.ChangeOwner(_currentUserContext.OwnerId);
                        if (securable.TenantId == Guid.Empty)
                            securable.ChangeTenant(_currentUserContext.TenantId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Example user context interface for demonstration.
    /// </summary>
    public interface ICurrentUserContext
    {
        Guid OwnerId { get; }
        Guid TenantId { get; }
    }
}
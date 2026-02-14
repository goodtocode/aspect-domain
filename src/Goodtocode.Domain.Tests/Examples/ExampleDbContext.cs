// Goodtocode.Domain.Tests\Examples\EfCoreIntegrationExample.cs
using Goodtocode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Goodtocode.Domain.Tests.Examples
{
    /// <summary>
    /// Example DbContext showing how to integrate IAuditable and ISecurable with EF Core.
    /// Copy/adapt this pattern in your infrastructure layer.
    /// </summary>
    public class ExampleDbContext : DbContext
    {
        private readonly ICurrentUserContext _currentUserContext;

        public ExampleDbContext(DbContextOptions options, ICurrentUserContext currentUserContext)
            : base(options)
        {
            _currentUserContext = currentUserContext;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            SetSecurityFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditable auditable)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        auditable.MarkModified();
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        auditable.MarkDeleted();
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
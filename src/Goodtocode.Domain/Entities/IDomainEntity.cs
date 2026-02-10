using Goodtocode.Domain.Events;

namespace Goodtocode.Domain.Entities;

public interface IDomainEntity<TModel>
{
    Guid Id { get; }
    string PartitionKey { get; }
    DateTime CreatedOn { get; }
    Guid CreatedBy { get; }
    DateTime? ModifiedOn { get; }
    Guid? ModifiedBy { get; }
    DateTime? DeletedOn { get; }
    Guid? DeletedBy { get; }
    DateTimeOffset Timestamp { get; }
    void AddDomainEvent(IDomainEvent<TModel> domainEvent);
    void ClearDomainEvents();
    bool Equals(object obj);
    int GetHashCode();
}
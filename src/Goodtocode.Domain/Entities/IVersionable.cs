namespace Goodtocode.Domain.Entities;

using System;

public interface IVersionable
{
    int Version { get; }
    Guid? PreviousVersionId { get; }
    bool IsPinned { get; }
    bool IsFrozen { get; }
    void Pin();
    void Freeze();
    void Thaw();
}

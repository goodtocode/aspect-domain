namespace Goodtocode.Domain.Entities;

using System;

/// <summary>
/// Read-only state contract for versioned entities.
/// All state changes in versioned entities produce new rows; this interface exposes only observable state.
/// </summary>
public interface IVersionable
{
    /// <summary>
    /// Gets the logical identity key that anchors a version series.
    /// All versions and the series itself share the same CanonicalKey within a tenant.
    /// </summary>
    string CanonicalKey { get; }

    /// <summary>
    /// Gets the 1-based version number within the CanonicalKey series.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Gets the identifier of the preceding row in this series, or null for the first version.
    /// </summary>
    Guid? PreviousVersionId { get; }

    /// <summary>
    /// Gets a value indicating whether this row is the current latest version in its series.
    /// Exactly one row per CanonicalKey should have IsLatest = true.
    /// The caller is responsible for flipping this flag transactionally.
    /// </summary>
    bool IsLatest { get; }

    /// <summary>
    /// Gets a value indicating whether this version is pinned.
    /// Pinning is always versioned: changing IsPinned produces a new row via CreateNextVersion().
    /// </summary>
    bool IsPinned { get; }

    /// <summary>
    /// Gets a value indicating whether this series is frozen.
    /// A frozen series cannot produce new versions; only a successor with a new CanonicalKey is allowed.
    /// </summary>
    bool IsFrozen { get; }
}

using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an audit tracked domain object
/// </summary>
public abstract class TrackDomainObject<TAudit> : DomainObjectBase, IEquatable<TrackDomainObject<TAudit>>
    where TAudit : AuditDomainObject
{
    /// <inheritdoc/>
    protected TrackDomainObject()
    {
    }

    /// <inheritdoc/>
    protected TrackDomainObject(TrackDomainObject<TAudit> copySource) :
        base(copySource)
    {
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(TrackDomainObject<TAudit> compare) =>
        base.Equals(compare);

    /// <summary>
    /// Create new audit object
    /// </summary>
    /// <returns>A new audit object instance</returns>
    public abstract TAudit ToAuditObject();

    /// <summary>
    /// Setup from audit object
    /// </summary>
    /// <returns>A new audit object instance</returns>
    public virtual void FromAuditObject(TAudit audit)
    {
        Status = audit.Status;
        Created = audit.Created;
        Updated = audit.Updated;
    }
}
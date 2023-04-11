
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an audit of a domain object
/// </summary>
public abstract class AuditDomainObject : DomainObjectBase
{
    /// <inheritdoc/>
    protected AuditDomainObject()
    {
    }

    /// <inheritdoc/>
    protected AuditDomainObject(AuditDomainObject copySource) :
        base(copySource)
    {
    }
}
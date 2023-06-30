// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a script audit of a domain object
/// </summary>
public abstract class ScriptAuditDomainObject : AuditDomainObject
{
    /// <summary>
    /// The expressions script
    /// </summary>
    public string Script { get; set; }
        
    /// <summary>
    /// The script version
    /// </summary>
    public string ScriptVersion { get; set; }

    /// <summary>
    /// The binary data
    /// </summary>
    public byte[] Binary { get; set; }

    /// <summary>
    /// The script hash value
    /// </summary>
    public int ScriptHash { get; set; }

    /// <inheritdoc/>
    protected ScriptAuditDomainObject()
    {
    }

    /// <inheritdoc/>
    protected ScriptAuditDomainObject(ScriptAuditDomainObject copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}
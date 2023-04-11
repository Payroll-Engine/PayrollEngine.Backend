using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an audit tracked domain object
/// </summary>
public abstract class ScriptTrackDomainObject<TAudit> : TrackDomainObject<TAudit>, IScriptObject, IEquatable<ScriptTrackDomainObject<TAudit>>
    where TAudit : ScriptAuditDomainObject
{
    /// <inheritdoc />
    public string Script { get; set; }

    /// <inheritdoc />
    public string ScriptVersion { get; set; }

    /// <inheritdoc />
    public byte[] Binary { get; set; }

    /// <inheritdoc />
    public int ScriptHash { get; set; }

    /// <inheritdoc />
    public abstract bool HasExpression { get; }

    /// <inheritdoc />
    public abstract bool HasObjectScripts { get; }

    /// <inheritdoc/>
    protected ScriptTrackDomainObject()
    {
    }

    /// <inheritdoc/>
    protected ScriptTrackDomainObject(ScriptTrackDomainObject<TAudit> copySource) :
        base(copySource)
    {
        Script = copySource.Script;
        ScriptVersion = copySource.ScriptVersion;
        Binary = copySource.Binary;
        ScriptHash = copySource.ScriptHash;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ScriptTrackDomainObject<TAudit> compare) =>
        compare != null &&
        base.Equals(compare) &&
        string.Equals(Script, compare.Script) &&
        // no binary and script hash compare
        string.Equals(ScriptVersion, compare.ScriptVersion);

    /// <summary>
    /// Get supported function types
    /// </summary>
    public virtual void Clear()
    {
        Script = default;
        Binary = default;
        ScriptHash = default;
    }
        
    /// <inheritdoc />
    public abstract List<FunctionType> GetFunctionTypes();

    /// <inheritdoc />
    public abstract IEnumerable<string> GetEmbeddedScriptNames();

    /// <inheritdoc />
    public abstract IDictionary<FunctionType, string> GetFunctionScripts();

    /// <summary>
    /// Setup from audit object
    /// </summary>
    /// <returns>A new audit object instance</returns>
    public override void FromAuditObject(TAudit audit)
    {
        base.FromAuditObject(audit);

        Script = audit.Script;
        ScriptVersion = audit.ScriptVersion;
        Binary = audit.Binary;
        ScriptHash = audit.ScriptHash;
    }
}
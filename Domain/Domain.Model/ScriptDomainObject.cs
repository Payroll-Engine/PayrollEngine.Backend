using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an audit tracked domain object
/// </summary>
public abstract class ScriptDomainObject : DomainObjectBase, IScriptObject, IEquatable<ScriptDomainObject>
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
    public abstract bool HasAnyExpression { get; }

    /// <inheritdoc />
    public abstract bool HasAnyAction { get; }

    /// <inheritdoc />
    public abstract bool HasObjectScripts { get; }

    /// <inheritdoc/>
    protected ScriptDomainObject()
    {
    }

    /// <inheritdoc/>
    protected ScriptDomainObject(ScriptDomainObject copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ScriptDomainObject compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <summary>
    /// Get supported function types
    /// </summary>
    public virtual void Clear()
    {
        Script = null;
        Binary = null;
        ScriptHash = 0;
    }

    /// <inheritdoc />
    public abstract List<FunctionType> GetFunctionTypes();

    /// <inheritdoc />
    public abstract IEnumerable<string> GetEmbeddedScriptNames();

    /// <inheritdoc />
    public abstract string GetFunctionScript(FunctionType functionType);

    /// <inheritdoc />
    public abstract List<string> GetFunctionActions(FunctionType functionType);
}
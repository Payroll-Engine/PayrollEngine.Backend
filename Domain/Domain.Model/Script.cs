using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll script
/// </summary>
public class Script : TrackDomainObject<ScriptAudit>, INamedObject, IDerivableObject, IEquatable<Script>
{
    /// <summary>
    /// The script name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The scripting function types
    /// </summary>
    public List<FunctionType> FunctionTypes { get; set; }

    /// <summary>
    /// The scripting function types
    /// </summary>
    public long FunctionTypeMask
    {
        get => FunctionTypes.ToBitmask();
        set => FunctionTypes = value.ToFunctionTypes();
    }

    /// <summary>
    /// The script value
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <inheritdoc/>
    public Script()
    {
    }

    /// <inheritdoc/>
    public Script(Script copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Script compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override ScriptAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            ScriptId = Id,
            Name = Name,
            FunctionTypes = FunctionTypes,
            FunctionTypeMask = FunctionTypeMask,
            Value = Value
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(ScriptAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.ScriptId;
        Name = audit.Name;
        FunctionTypes = audit.FunctionTypes;
        Value = audit.Value;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
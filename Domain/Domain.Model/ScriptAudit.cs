using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll script audit
/// </summary>
public class ScriptAudit : AuditDomainObject
{
    /// <summary>
    /// The script id
    /// </summary>
    public int ScriptId { get; set; }

    /// <summary>
    /// The script name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The scripting function types as bitmask
    /// </summary>
    public List<FunctionType> FunctionTypes { get; set; }

    /// <summary>
    /// The scripting function types
    /// </summary>
    public long FunctionTypeMask { get; set; }

    /// <summary>
    /// The script value
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <inheritdoc/>
    public ScriptAudit()
    {
    }

    /// <inheritdoc/>
    public ScriptAudit(ScriptAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
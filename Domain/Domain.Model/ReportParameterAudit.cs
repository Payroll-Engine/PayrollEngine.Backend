using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report parameter audit
/// </summary>
public class ReportParameterAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The report parameter id
    /// </summary>
    public int ReportParameterId { get; set; }

    /// <summary>
    /// The report parameter name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized wage type names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The report parameter description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized report parameter descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The parameter mandatory state
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// The parameter value (JSON)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The parameter value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The parameter type
    /// </summary>
    public ReportParameterType ParameterType { get; set; }
    
    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportParameterAudit"/> class
    /// </summary>
    public ReportParameterAudit()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportParameterAudit"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportParameterAudit(ReportParameterAudit copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}
using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report parameter
/// </summary>
public class ReportParameter : TrackDomainObject<ReportParameterAudit>, IDerivableObject,
    INamespaceObject, IDomainAttributeObject, IEquatable<ReportParameter>
{
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
    /// Hidden parameter
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// The parameter value (JSON)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The parameter value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The parameter value type
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
    /// Initializes a new instance of the <see cref="ReportParameter"/> class
    /// </summary>
    public ReportParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportParameter"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportParameter(ReportParameter copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportParameter compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override ReportParameterAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            ReportParameterId = Id,
            Name = Name,
            NameLocalizations = NameLocalizations,
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations,
            Mandatory = Mandatory,
            Hidden = Hidden,
            Value = Value,
            ValueType = ValueType,
            ParameterType = ParameterType,
            OverrideType = OverrideType,
            Attributes = Attributes,
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(ReportParameterAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.ReportParameterId;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations.Copy();
        Description = audit.Description;
        DescriptionLocalizations = audit.DescriptionLocalizations.Copy();
        Mandatory = audit.Mandatory;
        Hidden = audit.Hidden;
        Value = audit.Value;
        ValueType = audit.ValueType;
        ParameterType = audit.ParameterType;
        OverrideType = audit.OverrideType;
        Attributes = audit.Attributes.Copy();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
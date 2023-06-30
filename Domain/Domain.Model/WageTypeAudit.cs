﻿using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll wage type audit
/// </summary>
public class WageTypeAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The wage type id
    /// </summary>
    public int WageTypeId { get; set; }

    /// <summary>
    /// The wage type number
    /// </summary>
    public decimal WageTypeNumber { get; set; }

    /// <summary>
    /// The wage type name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized wage type names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The wage type description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized wage type descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The wage type calendar
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// Associated collectors
    /// </summary>
    public List<string> Collectors { get; set; }

    /// <summary>
    /// Associated collector groups
    /// </summary>
    public List<string> CollectorGroups { get; set; }

    /// <summary>
    /// Expression: calculates of the wage type value
    /// </summary>
    public string ValueExpression { get; set; }

    /// <summary>
    /// Expression: calculates of the wage type result attributes
    /// </summary>
    public string ResultExpression { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The wage type clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public WageTypeAudit()
    {
    }

    /// <inheritdoc/>
    public WageTypeAudit(WageTypeAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
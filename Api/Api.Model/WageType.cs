﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation wage type API object
/// </summary>
public class WageType : ApiObjectBase
{
    /// <summary>
    /// The wage type number (immutable)
    /// </summary>
    [Required]
    public decimal WageTypeNumber { get; set; }

    /// <summary>
    /// The wage type name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized wage type names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The wage type description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized wage type descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The value type, default is value type money
    /// </summary>
    public ValueType ValueType { get; set; }

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
    public override string ToString() =>
        $"{WageTypeNumber:##.####} {Name} {base.ToString()}";
}
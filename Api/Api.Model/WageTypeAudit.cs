// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation wage type audit API object (immutable)
/// </summary>
public class WageTypeAudit : ApiObjectBase
{
    /// <summary>
    /// The wage type id
    /// </summary>
    [Required]
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
    /// The wage type calendar
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// The wage type culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

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
    /// The wage type value actions
    /// </summary>
    public List<string> ValueActions { get; set; }

    /// <summary>
    /// The wage type result actions
    /// </summary>
    public List<string> ResultActions { get; set; }

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
        $"{WageTypeNumber:##.####}.{Name} {base.ToString()}";
}
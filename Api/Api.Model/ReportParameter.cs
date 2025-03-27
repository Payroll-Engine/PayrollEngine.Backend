using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report parameter API object
/// </summary>
public class ReportParameter : ApiObjectBase
{
    /// <summary>
    /// The report parameter name
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
    /// The report parameter description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized report parameter descriptions
    /// </summary>
    [Localization(nameof(Description))]
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

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
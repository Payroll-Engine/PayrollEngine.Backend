using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun parameter API object
/// </summary>
public class PayrunParameter : ApiObjectBase
{
    /// <summary>
    /// The payrun parameter name
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
    /// The payrun parameter description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payrun parameter descriptions
    /// </summary>
    [Localization(nameof(Description))]
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
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
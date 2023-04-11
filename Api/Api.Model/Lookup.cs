using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation lookup API object identified by his unique name.
/// The lookup contains one or more columns and the ValueColumn indicates
/// the column with the resulting value. ValueType specifies the type of the resulting value.
/// </summary>
public class Lookup : ApiObjectBase
{
    /// <summary>
    /// The unique lookup name (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized lookup names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The lookup description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized lookup descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
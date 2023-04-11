using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation lookup audit API object (immutable)
/// </summary>
public class LookupAudit : ApiObjectBase
{
    /// <summary>
    /// The lookup id
    /// </summary>
    [Required]
    public int LookupId { get; set; }

    /// <summary>
    /// The case field name
    /// </summary>
    [Required]
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
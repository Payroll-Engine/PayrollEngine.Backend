using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The division API object
/// </summary>
public class Division : ApiObjectBase
{
    /// <summary>
    /// The division name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized division names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The culture including the calendar, fallback of employee culture
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }

    /// <summary>
    /// The division calendar, fallback is the tenant calendar
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
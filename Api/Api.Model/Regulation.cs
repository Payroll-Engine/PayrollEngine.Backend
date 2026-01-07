using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll regulation API object
/// </summary>
public class Regulation : ApiObjectBase
{
    /// <summary>
    /// The regulation name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized regulation names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The regulation namespace
    /// </summary>
    [StringLength(128)]
    public string Namespace { get; set; }

    /// <summary>
    /// The regulation version, unique per regulation name
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Shared regulation (immutable)
    /// </summary>
    public bool SharedRegulation { get; set; }

    /// <summary>
    /// The date the regulation to be in force, anytime if undefined
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// The regulation owner
    /// </summary>
    [StringLength(128)]
    public string Owner { get; set; }

    /// <summary>
    /// The regulation description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized regulation descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// Required base regulations
    /// </summary>
    public List<string> BaseRegulations { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
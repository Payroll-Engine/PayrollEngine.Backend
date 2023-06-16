using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll API object
/// </summary>
public class Payroll : ApiObjectBase
{
    /// <summary>
    /// The payroll name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized payroll names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The payroll description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payroll descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    [Required]
    public int DivisionId { get; set; }

    /// <summary>
    /// The ISO 3166-1 country code, 0 for undefined
    /// </summary>
    public int Country { get; set; }

    /// <summary>
    /// The case cluster set (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetCase { get; set; }

    /// <summary>
    /// The case field cluster set (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetCaseField { get; set; }

    /// <summary>
    /// The collector cluster set (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetCollector { get; set; }

    /// <summary>
    /// The collector cluster set for retro payrun jobs (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetCollectorRetro { get; set; }

    /// <summary>
    /// The wage type cluster set (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetWageType { get; set; }

    /// <summary>
    /// The wage type cluster set for retro payrun jobs (undefined: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetWageTypeRetro { get; set; }

    /// <summary>
    /// The case value cluster set (undefined: none, *: all)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetCaseValue { get; set; }

    /// <summary>
    /// The wage type period result cluster set (undefined: none)
    /// </summary>
    [StringLength(128)]
    public string ClusterSetWageTypePeriod { get; set; }

    /// <summary>
    /// Cluster sets
    /// </summary>
    public ClusterSet[] ClusterSets { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
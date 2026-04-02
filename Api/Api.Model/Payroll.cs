// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PayrollEngine.Domain.Model;

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
    /// Typed cluster set name references for this payroll.
    /// Use this instead of the individual ClusterSetXxx properties.
    /// </summary>
    public PayrollClusterSets ClusterSet { get; set; }

    // -------------------------------------------------------------------------
    // Legacy individual properties — kept for API backward compatibility.
    // Mapped via the domain model delegates; new clients should use ClusterSet.
    // -------------------------------------------------------------------------

    /// <summary>The case cluster set name (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetCase { get; set; }

    /// <summary>The case field cluster set name (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetCaseField { get; set; }

    /// <summary>The collector cluster set name (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetCollector { get; set; }

    /// <summary>The collector cluster set name for retro payrun jobs (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetCollectorRetro { get; set; }

    /// <summary>The wage type cluster set name (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetWageType { get; set; }

    /// <summary>The wage type cluster set name for retro payrun jobs (undefined: all).</summary>
    [StringLength(128)]
    public string ClusterSetWageTypeRetro { get; set; }

    /// <summary>The case value cluster set name (undefined: none, *: all).</summary>
    [StringLength(128)]
    public string ClusterSetCaseValue { get; set; }

    /// <summary>The wage type period result cluster set name (undefined: none).</summary>
    [StringLength(128)]
    public string ClusterSetWageTypePeriod { get; set; }

    /// <summary>
    /// Cluster sets
    /// </summary>
    public List<ClusterSet> ClusterSets { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}

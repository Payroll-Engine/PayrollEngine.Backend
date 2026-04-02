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
    /// </summary>
    public PayrollClusterSets ClusterSet { get; set; }

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

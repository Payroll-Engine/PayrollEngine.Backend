using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll
/// </summary>
public class Payroll : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<Payroll>
{
    /// <summary>
    /// The payroll name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized payroll names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The payroll description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payroll descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The case cluster set (undefined: all)
    /// </summary>
    public string ClusterSetCase { get; set; }

    /// <summary>
    /// The case field cluster set (undefined: all)
    /// </summary>
    public string ClusterSetCaseField { get; set; }

    /// <summary>
    /// The collector cluster set (undefined: all)
    /// </summary>
    public string ClusterSetCollector { get; set; }

    /// <summary>
    /// The collector cluster set for retro payrun jobs (undefined: all)
    /// </summary>
    public string ClusterSetCollectorRetro { get; set; }

    /// <summary>
    /// The wage type cluster set (undefined: all)
    /// </summary>
    public string ClusterSetWageType { get; set; }

    /// <summary>
    /// The wage type cluster set for retro payrun jobs (undefined: all)
    /// </summary>
    public string ClusterSetWageTypeRetro { get; set; }

    /// <summary>
    /// The case value cluster set (undefined: none, *: all)
    /// </summary>
    public string ClusterSetCaseValue { get; set; }

    /// <summary>
    /// The wage type period result cluster set (undefined: none)
    /// </summary>
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
    public Payroll()
    {
    }

    /// <inheritdoc/>
    public Payroll(Payroll copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Payroll compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <summary>
    /// Get cluster set
    /// </summary>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <returns>The cluster set</returns>
    public ClusterSet GetClusterSet(string clusterSetName)
    {
        if (string.IsNullOrWhiteSpace(clusterSetName))
        {
            throw new ArgumentException(nameof(clusterSetName));
        }
        return ClusterSets.FirstOrDefault(x => string.Equals(clusterSetName, x.Name));
    }

    /// <summary>
    /// Test if cluster set exists
    /// </summary>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <returns>True, if the cluster set exists</returns>
    public bool ClusterSetExists(string clusterSetName) =>
        GetClusterSet(clusterSetName) != null;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}
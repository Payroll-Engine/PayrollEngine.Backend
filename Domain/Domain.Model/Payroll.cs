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
    /// Typed cluster set name references for this payroll.
    /// Persisted as a single JSON column; replaces the individual ClusterSetXxx string columns.
    /// </summary>
    public PayrollClusterSets ClusterSet { get; set; }

    // -------------------------------------------------------------------------
    // Read-only delegates — forward to ClusterSet for backward compatibility.
    // These properties are kept so that existing internal call sites continue to
    // compile unchanged. New code should access payroll.ClusterSet.ClusterSetXxx.
    // -------------------------------------------------------------------------

    /// <summary>The case cluster set name (undefined: all).</summary>
    public string ClusterSetCase
    {
        get => ClusterSet?.ClusterSetCase;
        set => (ClusterSet ??= new()).ClusterSetCase = value;
    }

    /// <summary>The case field cluster set name (undefined: all).</summary>
    public string ClusterSetCaseField
    {
        get => ClusterSet?.ClusterSetCaseField;
        set => (ClusterSet ??= new()).ClusterSetCaseField = value;
    }

    /// <summary>The collector cluster set name (undefined: all).</summary>
    public string ClusterSetCollector
    {
        get => ClusterSet?.ClusterSetCollector;
        set => (ClusterSet ??= new()).ClusterSetCollector = value;
    }

    /// <summary>The collector cluster set name for retro payrun jobs (undefined: all).</summary>
    public string ClusterSetCollectorRetro
    {
        get => ClusterSet?.ClusterSetCollectorRetro;
        set => (ClusterSet ??= new()).ClusterSetCollectorRetro = value;
    }

    /// <summary>The wage type cluster set name (undefined: all).</summary>
    public string ClusterSetWageType
    {
        get => ClusterSet?.ClusterSetWageType;
        set => (ClusterSet ??= new()).ClusterSetWageType = value;
    }

    /// <summary>The wage type cluster set name for retro payrun jobs (undefined: all).</summary>
    public string ClusterSetWageTypeRetro
    {
        get => ClusterSet?.ClusterSetWageTypeRetro;
        set => (ClusterSet ??= new()).ClusterSetWageTypeRetro = value;
    }

    /// <summary>The case value cluster set name (undefined: none, *: all).</summary>
    public string ClusterSetCaseValue
    {
        get => ClusterSet?.ClusterSetCaseValue;
        set => (ClusterSet ??= new()).ClusterSetCaseValue = value;
    }

    /// <summary>The wage type period result cluster set name (undefined: none).</summary>
    public string ClusterSetWageTypePeriod
    {
        get => ClusterSet?.ClusterSetWageTypePeriod;
        set => (ClusterSet ??= new()).ClusterSetWageTypePeriod = value;
    }

    /// <summary>The wage type cycle cache cluster set name (undefined: no cycle cache pre-loading).</summary>
    public string ClusterSetWageTypeCycle
    {
        get => ClusterSet?.ClusterSetWageTypeCycle;
        set => (ClusterSet ??= new()).ClusterSetWageTypeCycle = value;
    }

    /// <summary>The wage type consolidated cycle cache cluster set name (undefined: no pre-loading).</summary>
    public string ClusterSetWageTypeConsolidated
    {
        get => ClusterSet?.ClusterSetWageTypeConsolidated;
        set => (ClusterSet ??= new()).ClusterSetWageTypeConsolidated = value;
    }

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
    /// Get cluster set by name
    /// </summary>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <returns>The cluster set, or null when not found</returns>
    public ClusterSet GetClusterSet(string clusterSetName)
    {
        if (string.IsNullOrWhiteSpace(clusterSetName))
        {
            throw new ArgumentException(nameof(clusterSetName));
        }
        return ClusterSets?.FirstOrDefault(x => string.Equals(clusterSetName, x.Name));
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

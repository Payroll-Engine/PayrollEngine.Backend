using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Cluster set
/// </summary>
public class ClusterSet : IEquatable<ClusterSet>
{
    /// cluster set name for all clusters
    public static readonly string SetNameAll = "*";

    /// <summary>
    /// The filter name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The included clusters
    /// </summary>
    public List<string> IncludeClusters { get; set; }

    /// <summary>
    /// The excluded clusters
    /// </summary>
    public List<string> ExcludeClusters { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ClusterSet compare) =>
        CompareTool.EqualProperties(this, compare);
}
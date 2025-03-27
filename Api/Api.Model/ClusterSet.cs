using System.Collections.Generic;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Cluster set
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class ClusterSet
{
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
}
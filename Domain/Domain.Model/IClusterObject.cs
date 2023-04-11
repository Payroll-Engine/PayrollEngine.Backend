using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Object containing clusters
/// </summary>
public interface IClusterObject
{
    /// <summary>
    /// The clusters
    /// </summary>
    List<string> Clusters { get; set; }
}
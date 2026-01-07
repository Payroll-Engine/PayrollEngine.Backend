using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for clusters />
/// </summary>
public static class ClusterObjectExtensions
{
    /// <summary>
    /// Check if a cluster object is available
    /// </summary>
    /// <param name="clusterObjects">The cluster objects</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>True, if the cluster is available</returns>
    public static bool AvailableCluster(this IEnumerable<IClusterObject> clusterObjects, ClusterSet clusterSet)
    {
        // no objects or missing cluster set
        if (clusterObjects == null || clusterSet == null)
        {
            return false;
        }

        foreach (var clusterObject in clusterObjects)
        {
            if (!clusterObject.AvailableCluster(clusterSet))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if a cluster object is available
    /// </summary>
    /// <param name="clusterObject">The cluster object</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>True, if the cluster is available</returns>
    private static bool AvailableCluster(this IClusterObject clusterObject, ClusterSet clusterSet)
    {
        // no object or missing cluster set
        if (clusterObject == null || clusterSet == null)
        {
            return false;
        }

        // clusters
        var clusterList = new List<string>();
        if (clusterObject.Clusters != null)
        {
            clusterList.AddRange(clusterObject.Clusters.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        // filters
        var includeClusterList = new List<string>();
        if (clusterSet.IncludeClusters != null)
        {
            includeClusterList.AddRange(clusterSet.IncludeClusters.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        var excludeClusterList = new List<string>();
        if (clusterSet.ExcludeClusters != null)
        {
            excludeClusterList.AddRange(clusterSet.ExcludeClusters.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        // no filters
        if (!includeClusterList.Any() && !excludeClusterList.Any())
        {
            return true;
        }

        // include filter
        if (includeClusterList.Any())
        {
            var included = false;
            foreach (var includeCluster in includeClusterList)
            {
                if (clusterList.Contains(includeCluster))
                {
                    // included object
                    included = true;
                    break;
                }
            }

            // not included
            if (!included)
            {
                return false;
            }
        }

        // exclude filter
        if (excludeClusterList.Any() && clusterList.Any())
        {
            foreach (var excludeCluster in excludeClusterList)
            {
                if (clusterList.Contains(excludeCluster))
                {
                    // excluded object
                    return false;
                }
            }
        }

        // unfiltered or included and not excluded
        return true;
    }
}
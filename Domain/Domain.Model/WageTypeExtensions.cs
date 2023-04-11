using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="WageType"/>
/// </summary>
public static class WageTypeExtensions
{
    /// <summary>
    /// Get offset period
    /// </summary>
    /// <param name="wageType">The wage type</param>
    /// <param name="collectorName">The collector name</param>
    /// <param name="collectorGroups">The collector groups</param>
    /// <returns>Offset period</returns>
    public static bool CollectorAvailable(this WageType wageType, string collectorName, IEnumerable<string> collectorGroups = null)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            throw new ArgumentException(nameof(collectorName));
        }

        // collector
        if (wageType.Collectors != null && wageType.Collectors.Contains(collectorName))
        {
            return true;
        }

        // collector groups
        if (wageType.CollectorGroups != null && collectorGroups != null)
        {
            foreach (var collectorGroup in collectorGroups)
            {
                if (wageType.CollectorGroups.Contains(collectorGroup))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides a case
/// </summary>
public interface ICaseProvider
{
    /// <summary>
    /// Get payroll case
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseName">The case name</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The case matching the name at a given time</returns>
    Task<Case> GetCaseAsync(IDbContext context, int payrollId, string caseName,
        OverrideType? overrideType = null, ClusterSet clusterSet = null);
}


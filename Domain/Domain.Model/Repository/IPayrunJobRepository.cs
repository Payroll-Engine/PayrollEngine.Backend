using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payrun jobs
/// </summary>
public interface IPayrunJobRepository : IChildDomainRepository<PayrunJob>
{
    /// <summary>
    /// Query employee payrun jobs
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payrun jobs of the employee</returns>
    Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(IDbContext context, int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Query employee payrun jobs count
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payrun jobs of the employee</returns>
    Task<long> QueryEmployeePayrunJobsCountAsync(IDbContext context, int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Patch the payrun job status
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="jobStatus">The job status to path</param>
    /// <param name="userId">The user id</param>
    /// <param name="reason">The change reason</param>
    /// <returns>The payrun job with the new status</returns>
    Task<PayrunJob> PatchPayrunJobStatusAsync(IDbContext context, int tenantId, int payrunJobId,
        PayrunJobStatus jobStatus, int userId, string reason);
}
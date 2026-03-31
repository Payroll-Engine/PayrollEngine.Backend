using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrunJobService : IChildApplicationService<IPayrunJobRepository, PayrunJob>
{
    PayrunJobServiceSettings Settings { get; }

    /// <summary>
    /// Query employee payrun jobs
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payrun jobs of the employee</returns>
    Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(IDbContext context, int tenantId,
        int employeeId, Query query = null);

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
    /// Import payrun job sets from an external source (archive restore, migration).
    /// Creates the PayrunJob first, then the PayrollResultSets with the new job id.
    /// Returns the number of imported job sets.
    /// Throws on unresolvable references (422) or duplicate jobs (409).
    /// </summary>
    Task<int> ImportPayrunJobSetsAsync(IDbContext context, int tenantId, IEnumerable<PayrunJobSet> jobSets);
}
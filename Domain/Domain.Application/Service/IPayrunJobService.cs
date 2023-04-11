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
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payrun jobs of the employee</returns>
    Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Query employee payrun jobs count
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payrun jobs of the employee</returns>
    Task<long> QueryEmployeePayrunJobsCountAsync(int tenantId, int employeeId, Query query = null);
}
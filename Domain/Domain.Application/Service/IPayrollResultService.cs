using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollResultService : IChildApplicationService<IPayrollResultRepository, PayrollResult>
{
    /// <summary>
    /// Query collector results
    /// </summary>
    /// <param name="resultId">The result id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Collector query results</returns>
    Task<IEnumerable<CollectorResult>> QueryCollectorResultsAsync(int resultId, Query query = null);

    /// <summary>
    /// Query collector custom results
    /// </summary>
    /// <param name="resultId">The result id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Collector custom results</returns>
    Task<IEnumerable<CollectorCustomResult>> QueryCollectorCustomResultsAsync(int resultId, Query query = null);

    /// <summary>
    /// Query wage type results
    /// </summary>
    /// <param name="resultId">The result id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Wage type query results</returns>
    Task<IEnumerable<WageTypeResult>> QueryWageTypeResultsAsync(int resultId, Query query = null);

    /// <summary>
    /// Query wage type custom results
    /// </summary>
    /// <param name="resultId">The result id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Wage type custom query results</returns>
    Task<IEnumerable<WageTypeCustomResult>> QueryWageTypeCustomResultsAsync(int resultId, Query query = null);

    /// <summary>
    /// Query payrun results
    /// </summary>
    /// <param name="resultId">The result id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Case value query results</returns>
    Task<IEnumerable<PayrunResult>> QueryPayrunResultsAsync(int resultId, Query query = null);

    /// <summary>
    /// Query consolidated payroll collector custom results
    /// </summary>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Collector query results</returns>
    Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null);

    /// <summary>
    /// Query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the domain objects, matching the parameters and conditions</returns>
    Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Count query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the domain objects, matching the parameters and conditions</returns>
    Task<long> QueryResultValueCountAsync(int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Query payroll result sets
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Result set query results</returns>
    Task<IEnumerable<PayrollResultSet>> QueryResultSetsAsync(int tenantId, Query query = null);

    /// <summary>
    /// Gets a payroll result set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="resultId">The result identifier</param>
    /// <returns>The payroll result set</returns>
    Task<PayrollResultSet> GetResultSetAsync(int tenantId, int resultId);
}
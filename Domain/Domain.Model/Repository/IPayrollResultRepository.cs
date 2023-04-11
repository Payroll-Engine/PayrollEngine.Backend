using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll result infos
/// </summary>
public interface IPayrollResultRepository : IChildDomainRepository<PayrollResult>
{

    /// <summary>
    /// Get all result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of payroll result values, matching the parameters and conditions</returns>
    Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Count query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Payroll result values count matching the query</returns>
    Task<long> QueryResultValueCountAsync(int tenantId, int employeeId, Query query = null);

    /// <summary>
    /// Get employee wage type results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type values</returns>
    Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null);

    /// <summary>
    /// Get employee wage type custom results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type values</returns>
    Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null);

    /// <summary>
    /// Get employee collector results from a time period
    /// </summary>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector values</returns>
    Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null);

    /// <summary>
    /// Get collector custom results collected within a time period
    /// </summary>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Period collector values</returns>
    Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null);
}
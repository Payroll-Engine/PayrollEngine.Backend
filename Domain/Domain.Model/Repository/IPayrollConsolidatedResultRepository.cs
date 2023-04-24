using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll consolidated result infos
/// </summary>
public interface IPayrollConsolidatedResultRepository : IChildDomainRepository<PayrollResult>
{
    /// <summary>
    /// Get consolidated payroll results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll result query</param>
    /// <returns>Period payroll results</returns>
    Task<ConsolidatedPayrollResult> GetPayrollResultAsync(IDbContext context, PayrollResultQuery query);

    /// <summary>
    /// Get consolidated period wage type results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Period wage type values</returns>
    Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Get consolidated wage type custom results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Custom wage type values</returns>
    Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Get consolidated collector results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Period collector values</returns>
    Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query);

    /// <summary>
    /// Get consolidated collector custom results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Period collector values</returns>
    Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query);

    /// <summary>
    /// Get consolidated payrun results collected within a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payrun result query</param>
    /// <returns>Period wage type values</returns>
    Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(IDbContext context, ConsolidatedPayrunResultQuery query);
}
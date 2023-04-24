using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollConsolidatedResultService : IChildApplicationService<IPayrollConsolidatedResultRepository, PayrollResult>
{
    /// <summary>
    /// Query consolidated payroll results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll result query</param>
    /// <returns>Consolidated payroll query results</returns>
    Task<ConsolidatedPayrollResult> GetPayrollResultAsync(IDbContext context, PayrollResultQuery query);

    /// <summary>
    /// Query consolidated payroll collector results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Collector query results</returns>
    Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query);

    /// <summary>
    /// Query consolidated payroll collector custom results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Collector query results</returns>
    Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query);

    /// <summary>
    /// Query consolidated wage type results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Wage type query results</returns>
    Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Query consolidated wage type custom results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Wage type custom query results</returns>
    Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Query consolidated payroll payrun results
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payrun result query</param>
    /// <returns></returns>
    Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(IDbContext context, ConsolidatedPayrunResultQuery query);
}
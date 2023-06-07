using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides payroll results
/// </summary>
public interface IResultProvider
{
    /// <summary>
    /// Query employee wage type values from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type values</returns>
    Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, WageTypeResultQuery query, int? payrunJobId = null,
    int? parentPayrunJobId = null);

    /// <summary>
    /// Query employee wage type custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type custom results</returns>
    Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, WageTypeResultQuery query,
    int? payrunJobId = null, int? parentPayrunJobId = null);

    /// <summary>
    /// Query employee collector results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector results</returns>
    Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
    int? parentPayrunJobId = null);

    /// <summary>
    /// Query employee collector custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector custom results</returns>
    Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
    int? parentPayrunJobId = null);

    /// <summary>
    /// Query consolidated employee wage type results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type results</returns>
    Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Query consolidated employee wage type custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type custom results</returns>
    Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(IDbContext context,
        ConsolidatedWageTypeResultQuery query);

    /// <summary>
    /// Query consolidated employee collector results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee collector results</returns>
    Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query);

    /// <summary>
    /// Query consolidated employee collector custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Employee collector custom results</returns>
    Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(IDbContext context,
        ConsolidatedCollectorResultQuery query);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides payroll results
/// </summary>
public sealed class ResultProvider
{
    /// <summary>
    /// The payroll result repository
    /// </summary>
    public IPayrollResultRepository PayrollResultRepository { get; }

    /// <summary>
    /// The payroll consolidated result repository
    /// </summary>
    public IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultProvider"/> class
    /// </summary>
    /// <param name="payrollResultRepository">The payroll result repository</param>
    /// <param name="payrollConsolidatedResultRepository">The payroll consolidated result repository</param>
    public ResultProvider(IPayrollResultRepository payrollResultRepository, IPayrollConsolidatedResultRepository payrollConsolidatedResultRepository)
    {
        PayrollResultRepository = payrollResultRepository ?? throw new ArgumentNullException(nameof(payrollResultRepository));
        PayrollConsolidatedResultRepository = payrollConsolidatedResultRepository ?? throw new ArgumentNullException(nameof(payrollConsolidatedResultRepository));
    }

    /// <summary>
    /// Query employee wage type values from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type values</returns>
    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, WageTypeResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee wage type custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type custom results</returns>
    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee collector results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector results</returns>
    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee collector custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector custom results</returns>
    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query consolidated employee wage type results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type results</returns>
    public async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeResultsAsync(context, query);

    /// <summary>
    /// Query consolidated employee wage type custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type custom results</returns>
    public async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeCustomResultsAsync(context, query);

    /// <summary>
    /// Query consolidated employee collector results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee collector results</returns>
    public async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorResultsAsync(context, query);

    /// <summary>
    /// Query consolidated employee collector custom results from a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The collector result query</param>
    /// <returns>Employee collector custom results</returns>
    public async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorCustomResultsAsync(context, query);
}
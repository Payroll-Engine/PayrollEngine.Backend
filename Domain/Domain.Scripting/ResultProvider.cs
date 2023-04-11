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
        PayrollConsolidatedResultRepository = payrollConsolidatedResultRepository ??throw new ArgumentNullException(nameof(payrollConsolidatedResultRepository));
    }

    /// <summary>
    /// Query employee wage type values from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type values</returns>
    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(WageTypeResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee wage type custom results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee wage type custom results</returns>
    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(WageTypeResultQuery query, 
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeCustomResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee collector results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector results</returns>
    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query employee collector custom results from a time period
    /// </summary>
    /// <param name="query">The collector result query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>Employee collector custom results</returns>
    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorCustomResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <summary>
    /// Query consolidated employee wage type results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type results</returns>
    public async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeResultsAsync(query);

    /// <summary>
    /// Query consolidated employee wage type custom results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee wage type custom results</returns>
    public async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeCustomResultsAsync(query);

    /// <summary>
    /// Query consolidated employee collector results from a time period
    /// </summary>
    /// <param name="query">The wage type result query</param>
    /// <returns>Employee collector results</returns>
    public async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorResultsAsync(query);

    /// <summary>
    /// Query consolidated employee collector custom results from a time period
    /// </summary>
    /// <param name="query">The collector result query</param>
    /// <returns>Employee collector custom results</returns>
    public async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorCustomResultsAsync(query);
}
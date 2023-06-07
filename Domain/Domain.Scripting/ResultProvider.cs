using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides payroll results
/// </summary>
public sealed class ResultProvider : IResultProvider
{
    /// <summary>
    /// The payroll result repository
    /// </summary>
    private IPayrollResultRepository PayrollResultRepository { get; }

    /// <summary>
    /// The payroll consolidated result repository
    /// </summary>
    private IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; }

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

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, WageTypeResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetWageTypeCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, CollectorResultQuery query, int? payrunJobId = null,
        int? parentPayrunJobId = null) =>
        await PayrollResultRepository.GetCollectorCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeResultsAsync(context, query);

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetWageTypeCustomResultsAsync(context, query);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorResultsAsync(context, query);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await PayrollConsolidatedResultRepository.GetCollectorCustomResultsAsync(context, query);
}
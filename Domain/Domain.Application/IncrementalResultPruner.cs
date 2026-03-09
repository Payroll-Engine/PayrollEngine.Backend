using System;
using System.Linq;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Removes collector and wage type results that are identical to the already-stored
/// consolidated results for the same period. Used in incremental mode to avoid
/// persisting redundant data.
/// </summary>
internal sealed class IncrementalResultPruner
{
    private IDbContext DbContext { get; }
    private IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; }

    internal IncrementalResultPruner(IDbContext dbContext,
        IPayrollConsolidatedResultRepository consolidatedResultRepository)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        ConsolidatedResultRepository = consolidatedResultRepository ??
            throw new ArgumentNullException(nameof(consolidatedResultRepository));
    }

    /// <summary>
    /// Removes collector and wage type results from <paramref name="payrollResult"/> that are
    /// identical (same value and tags) to the already-stored results for the same period.
    /// </summary>
    /// <param name="tenantId">The tenant whose consolidated results are queried for comparison.</param>
    /// <param name="payrollResult">The current calculation result set; unchanged entries are removed in-place.</param>
    /// <param name="evaluationDate">The evaluation date used to query the consolidated result snapshot.</param>
    internal async Task RemoveUnchangedAsync(int tenantId, PayrollResultSet payrollResult, DateTime evaluationDate)
    {
        // existing collectors by collector name
        var collectorResults = (await ConsolidatedResultRepository.GetCollectorResultsAsync(DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = payrollResult.EmployeeId,
                DivisionId = payrollResult.DivisionId,
                PeriodStarts = new List<DateTime> { payrollResult.PeriodStart },
                EvaluationDate = evaluationDate
            })).ToList();
        if (collectorResults.Any())
        {
            // collect the names of all unchanged collector results, then remove in one pass
            var unchangedCollectors = new HashSet<string>(
                collectorResults
                    .Where(db =>
                    {
                        var current = payrollResult.CollectorResults.FirstOrDefault(
                            x => string.Equals(x.CollectorName, db.CollectorName));
                        return current != null
                            && current.Value == db.Value
                            && CompareTool.EqualLists(current.Tags, db.Tags);
                    })
                    .Select(db => db.CollectorName));
            payrollResult.CollectorResults.RemoveAll(
                x => unchangedCollectors.Contains(x.CollectorName));
        }

        // existing wage types by wage type number
        var wageTypeResults = (await ConsolidatedResultRepository.GetWageTypeResultsAsync(DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = payrollResult.EmployeeId,
                DivisionId = payrollResult.DivisionId,
                PeriodStarts = new List<DateTime> { payrollResult.PeriodStart },
                EvaluationDate = evaluationDate
            })).ToList();
        if (wageTypeResults.Any())
        {
            // collect the wage type numbers of all unchanged results, then remove in one pass
            var unchangedWageTypes = new HashSet<decimal>(
                wageTypeResults
                    .Where(db =>
                    {
                        var current = payrollResult.WageTypeResults.FirstOrDefault(
                            x => x.WageTypeNumber == db.WageTypeNumber);
                        return current != null
                            && current.Value == db.Value
                            && CompareTool.EqualLists(current.Tags, db.Tags);
                    })
                    .Select(db => db.WageTypeNumber));
            payrollResult.WageTypeResults.RemoveAll(
                x => unchangedWageTypes.Contains(x.WageTypeNumber));
        }
    }
}

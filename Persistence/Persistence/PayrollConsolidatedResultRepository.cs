using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrollConsolidatedResultRepository : ChildDomainRepository<PayrollResult>, IPayrollConsolidatedResultRepository
{
    public PayrollConsolidatedResultRepository(IDbContext context) :
        base(DbSchema.Tables.PayrollResult, DbSchema.PayrollResultColumn.TenantId, context)
    {
    }

    /// <inheritdoc />
    public virtual async Task<ConsolidatedPayrollResult> GetPayrollResultAsync(PayrollResultQuery query)
    {
        // collector results
        var collectorResults = (await GetCollectorResultsAsync(
            new(query)
            {
                PeriodStarts = new List<DateTime> { query.Period.Start }
            })).ToList();

        // wage type results
        var wageTypeResults = new List<WageTypeResultSet>();
        var wageTypeResultQuery = new ConsolidatedWageTypeResultQuery(query)
        {
            JobStatus = query.JobStatus,
            Tags = query.Tags?.Distinct().ToList()
        };
        var allWageTypeResults = (await GetWageTypeResultsAsync(wageTypeResultQuery)).ToList();
        foreach (var wageTypeResult in allWageTypeResults)
        {
            // update query wage type number filter
            wageTypeResultQuery.WageTypeNumbers = new[] { wageTypeResult.WageTypeNumber };
            var wageTypeResultSet = new WageTypeResultSet(wageTypeResult)
            {
                CustomResults = (await GetWageTypeCustomResultsAsync(wageTypeResultQuery)).ToList()
            };
            wageTypeResults.Add(wageTypeResultSet);
        }

        // payrun results
        var payrunResults = (await GetPayrunResultsAsync(
            new(query)
            {
                PeriodStarts = new List<DateTime> { query.Period.Start }
            })).ToList();

        return new()
        {
            CollectorResults = collectorResults,
            WageTypeResults = wageTypeResults,
            PayrunResults = payrunResults
        };
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await new ConsolidateWageTypeResultCommand(Connection).GetResultsAsync(query);


    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await new ConsolidateWageTypeCustomResultCommand(Connection).GetResultsAsync(query);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await new ConsolidateCollectorResultCommand(Connection).GetResultsAsync(query);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(
        ConsolidatedCollectorResultQuery query) =>
        await new CollectorCustomResultConsolidateCommand(Connection).GetResultsAsync(query);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(ConsolidatedPayrunResultQuery query) =>
        await new PayrunResultConsolidateCommand(Connection).GetResultsAsync(query);
}
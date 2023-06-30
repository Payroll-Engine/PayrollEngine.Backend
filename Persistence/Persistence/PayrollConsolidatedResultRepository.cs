using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrollConsolidatedResultRepository : ChildDomainRepository<PayrollResult>, IPayrollConsolidatedResultRepository
{
    public PayrollConsolidatedResultRepository() :
        base(DbSchema.Tables.PayrollResult, DbSchema.PayrollResultColumn.TenantId)
    {
    }

    /// <inheritdoc />
    public async Task<ConsolidatedPayrollResult> GetPayrollResultAsync(IDbContext context, PayrollResultQuery query)
    {
        // collector results
        var collectorResults = (await GetCollectorResultsAsync(context,
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
        var allWageTypeResults = (await GetWageTypeResultsAsync(context, wageTypeResultQuery)).ToList();
        foreach (var wageTypeResult in allWageTypeResults)
        {
            // update query wage type number filter
            wageTypeResultQuery.WageTypeNumbers = new[] { wageTypeResult.WageTypeNumber };
            var wageTypeResultSet = new WageTypeResultSet(wageTypeResult)
            {
                CustomResults = (await GetWageTypeCustomResultsAsync(context, wageTypeResultQuery)).ToList()
            };
            wageTypeResults.Add(wageTypeResultSet);
        }

        // payrun results
        var payrunResults = (await GetPayrunResultsAsync(context,
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
    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context,
        ConsolidatedWageTypeResultQuery query)
    {
        return await new ConsolidateWageTypeResultCommand(context).GetResultsAsync(query);
    }


    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context,
        ConsolidatedWageTypeResultQuery query) =>
        await new ConsolidateWageTypeCustomResultCommand(context).GetResultsAsync(query);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context,
        ConsolidatedCollectorResultQuery query) =>
        await new ConsolidateCollectorResultCommand(context).GetResultsAsync(query);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context,
        ConsolidatedCollectorResultQuery query) =>
        await new CollectorCustomResultConsolidateCommand(context).GetResultsAsync(query);

    /// <inheritdoc />
    public async Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(IDbContext context,
        ConsolidatedPayrunResultQuery query) =>
        await new PayrunResultConsolidateCommand(context).GetResultsAsync(query);
}
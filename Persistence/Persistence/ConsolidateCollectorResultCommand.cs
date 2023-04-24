using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class ConsolidateCollectorResultCommand : CollectorResultCommandBase
{
    internal ConsolidateCollectorResultCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<CollectorResult>> GetResultsAsync(ConsolidatedCollectorResultQuery query)
    {
        var parameters = GetQueryParameters(query);

        QueryBegin();

        // retrieve consolidated collector values (stored procedure)
        var values = await DbContext.QueryAsync<CollectorResult>(DbSchema.Procedures.GetConsolidatedCollectorResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query cons collector}} {GetItemsString(query.CollectorNames?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
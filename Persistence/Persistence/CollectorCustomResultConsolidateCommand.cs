using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class CollectorCustomResultConsolidateCommand : CollectorResultCommandBase
{
    internal CollectorCustomResultConsolidateCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<CollectorCustomResult>> GetResultsAsync(ConsolidatedCollectorResultQuery query)
    {
        var parameters = GetQueryParameters(query);

        QueryBegin();

        // retrieve consolidated collector values (stored procedure)
        var values = await DbContext.QueryAsync<CollectorCustomResult>(DbSchema.Procedures.GetConsolidatedCollectorCustomResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query cons collector custom}} {GetItemsString(query.CollectorNames?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
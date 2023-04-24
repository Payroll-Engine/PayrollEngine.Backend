using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class ConsolidateWageTypeResultCommand : WageTypeResultCommandBase
{
    internal ConsolidateWageTypeResultCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<WageTypeResult>> GetResultsAsync(ConsolidatedWageTypeResultQuery query)
    {
        var parameters = GetQueryParameters(query);

        QueryBegin();

        // retrieve employee wage type values (stored procedure)
        var values = await DbContext.QueryAsync<WageTypeResult>(DbSchema.Procedures.GetConsolidatedWageTypeResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query cons wage type}} {GetItemsString(query.WageTypeNumbers?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
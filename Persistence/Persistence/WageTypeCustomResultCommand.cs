using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class WageTypeCustomResultCommand : WageTypeResultCommandBase
{
    internal WageTypeCustomResultCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<WageTypeCustomResult>> GetResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null)
    {
        var parameters = GetQueryParameters(query, payrunJobId, parentPayrunJobId);

        QueryBegin();

        // retrieve employee wage type values (stored procedure)
        var values = await DbContext.QueryAsync<WageTypeCustomResult>(DbSchema.Procedures.GetWageTypeCustomResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query wage type custom}} {GetItemsString(query.WageTypeNumbers?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
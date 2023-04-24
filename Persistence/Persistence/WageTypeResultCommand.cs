using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class WageTypeResultCommand : WageTypeResultCommandBase
{
    internal WageTypeResultCommand(IDbContext context) :
        base(context)
    {
    }

    /// <summary>
    /// Get wage type results
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>The wage type results</returns>
    internal async Task<IEnumerable<WageTypeResult>> GetResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null)
    {
        var parameters = GetQueryParameters(query, payrunJobId, parentPayrunJobId);

        QueryBegin();

        // retrieve employee wage type values (stored procedure)
        var values = await DbContext.QueryAsync<WageTypeResult>(DbSchema.Procedures.GetWageTypeResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query wage type}} {GetItemsString(query.WageTypeNumbers?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class CollectorCustomResultCommand : CollectorResultCommandBase
{
    internal CollectorCustomResultCommand(IDbContext context) :
        base(context)
    {
    }

    /// <summary>
    /// Get collector results
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>The collector results</returns>
    internal async Task<IEnumerable<CollectorCustomResult>> GetResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null)
    {
        var parameters = GetQueryParameters(query, payrunJobId, parentPayrunJobId);

        QueryBegin();

        // retrieve employee collector values (stored procedure)
        var values = await DbContext.QueryAsync<CollectorCustomResult>(DbSchema.Procedures.GetCollectorCustomResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query collector custom}} {GetItemsString(query.CollectorNames?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
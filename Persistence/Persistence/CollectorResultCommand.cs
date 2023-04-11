using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class CollectorResultCommand : CollectorResultCommandBase
{
    internal CollectorResultCommand(IDbConnection connection) :
        base(connection)
    {
    }

    /// <summary>
    /// Get collector results
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>The collector results</returns>
    internal async Task<IEnumerable<CollectorResult>> GetResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null)
    {
        var parameters = GetQueryParameters(query, payrunJobId, parentPayrunJobId);

        QueryBegin();

        // retrieve employee collector values (stored procedure)
        var values = await Connection.QueryAsync<CollectorResult>(DbSchema.Procedures.GetCollectorResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query collector}} {GetItemsString(query.CollectorNames?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
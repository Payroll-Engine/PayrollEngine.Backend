﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class ConsolidateWageTypeCustomResultCommand : WageTypeResultCommandBase
{
    internal ConsolidateWageTypeCustomResultCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<WageTypeCustomResult>> GetResultsAsync(ConsolidatedWageTypeResultQuery query)
    {
        var parameters = GetQueryParameters(query);

        QueryBegin();

        // retrieve wage type custom result values (stored procedure)
        var values = await DbContext.QueryAsync<WageTypeCustomResult>(DbSchema.Procedures.GetConsolidatedWageTypeCustomResults,
            parameters, commandType: CommandType.StoredProcedure);

        QueryEnd(() => $"{{Result query cons wage type custom}} {GetItemsString(query.WageTypeNumbers?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}
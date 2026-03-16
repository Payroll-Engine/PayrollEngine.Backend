using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueResultCommand : DomainRepositoryCommandBase
{
    /// <summary>
    /// Execute item query in case query attributes are present
    /// </summary>
    /// <typeparam name="TItem">The type of results to return.</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="TItem"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    internal async Task<IEnumerable<TItem>> QueryCaseValuesAsync<TItem>(IDbContext context, CaseValueQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.StoredProcedure))
        {
            throw new ArgumentException(nameof(query.StoredProcedure));
        }
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new ArgumentException(nameof(query.Query));
        }

        // attributes requested, use the slow query
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterCaseValueQuery.ParentId, query.ParentId, DbType.Int32);
        // EmployeeId and Culture are only supported by MySql CaseValue pivot SPs
        if (context.CaseValueExtendedParameters)
        {
            parameters.Add(ParameterCaseValueQuery.EmployeeId,
                query.EmployeeId.HasValue ? (object)query.EmployeeId.Value : null, DbType.Int32);
        }
        parameters.Add(ParameterCaseValueQuery.Sql, query.Query);
        parameters.Add(ParameterCaseValueQuery.Attributes,
            query.QueryAttributes?.Any() == true
                ? JsonSerializer.Serialize(query.QueryAttributes) : null);
        if (context.CaseValueExtendedParameters)
        {
            parameters.Add(ParameterCaseValueQuery.Culture,
                string.IsNullOrWhiteSpace(query.Culture) ? null : query.Culture);
        }

        return await context.QueryAsync<TItem>(query.StoredProcedure, parameters,
            commandType: CommandType.StoredProcedure);
    }
}
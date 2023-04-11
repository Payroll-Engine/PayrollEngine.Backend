using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueResultCommand : DomainRepositoryCommandBase
{
    internal CaseValueResultCommand(IDbConnection connection) :
        base(connection)
    {
    }

    /// <summary>
    /// Execute a item query in case query attributes are present
    /// </summary>
    /// <typeparam name="TItem">The type of results to return.</typeparam>
    /// <param name="query">The case value query</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="TItem"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal async Task<IEnumerable<TItem>> QueryCaseValuesAsync<TItem>(CaseValueQuery query)
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
        parameters.Add(DbSchema.ParameterCaseValueQuery.ParentId, query.ParentId);
        if (query.EmployeeId.HasValue)
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.EmployeeId, query.EmployeeId.Value);
        }
        parameters.Add(DbSchema.ParameterCaseValueQuery.Sql, query.Query);
        if (query.QueryAttributes != null && query.QueryAttributes.Any())
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.Attributes,
                System.Text.Json.JsonSerializer.Serialize(query.QueryAttributes));
        }
        if (query.Language.HasValue)
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.Language, query.Language.Value.LanguageCode());
        }

        return await Connection.QueryAsync<TItem>(query.StoredProcedure, parameters,
            commandType: CommandType.StoredProcedure);
    }
}
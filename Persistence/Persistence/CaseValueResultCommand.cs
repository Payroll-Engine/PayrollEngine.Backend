using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

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
        parameters.Add(DbSchema.ParameterCaseValueQuery.ParentId, query.ParentId, DbType.Int32);
        if (query.EmployeeId.HasValue)
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.EmployeeId, query.EmployeeId.Value, DbType.Int32);
        }
        parameters.Add(DbSchema.ParameterCaseValueQuery.Sql, query.Query);
        if (query.QueryAttributes != null && query.QueryAttributes.Any())
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.Attributes,
                System.Text.Json.JsonSerializer.Serialize(query.QueryAttributes));
        }
        if (!string.IsNullOrWhiteSpace(query.Culture))
        {
            parameters.Add(DbSchema.ParameterCaseValueQuery.Culture, query.Culture);
        }

        return await context.QueryAsync<TItem>(query.StoredProcedure, parameters,
            commandType: CommandType.StoredProcedure);
    }
}
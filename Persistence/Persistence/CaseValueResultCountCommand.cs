using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueResultCountCommand : DomainRepositoryCommandBase
{
    internal CaseValueResultCountCommand(IDbConnection connection) :
        base(connection)
    {
    }

    /// <summary>
    /// Execute a item count query in case query attributes are present
    /// </summary>
    /// <param name="query">The case value query</param>
    /// <returns>The record count matching the query criteria</returns>
    internal async Task<long> QueryCaseValuesCountAsync(CaseValueQuery query)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
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

        var counts = (await Connection.QueryAsync<long>(
            sql: query.StoredProcedure,
            param: parameters,
            commandType: CommandType.StoredProcedure)).ToList();
        var count = counts.Sum(x => x);
        return count;
    }
}
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueResultCountCommand : DomainRepositoryCommandBase
{
    /// <summary>
    /// Execute item count query in case query attributes are present
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <returns>The record count matching the query criteria</returns>
    internal async Task<long> QueryCaseValuesCountAsync(IDbContext context, CaseValueQuery query)
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

        var counts = (await context.QueryAsync<long>(
            sql: query.StoredProcedure,
            param: parameters,
            commandType: CommandType.StoredProcedure)).ToList();
        var count = counts.Sum(x => x);
        return count;
    }
}
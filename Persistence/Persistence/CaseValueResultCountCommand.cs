using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

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
        parameters.Add(ParameterCaseValueQuery.ParentId, query.ParentId, DbType.Int32);
        // EmployeeId / DivisionId: always sent by MySQL (CaseValueExtendedParameters) for all pivot SPs.
        // On SQL Server only accepted by GetPayrollResultValues; pass when explicitly set.
        if (context.CaseValueExtendedParameters || query.EmployeeId.HasValue)
        {
            parameters.Add(ParameterCaseValueQuery.EmployeeId, query.EmployeeId, DbType.Int32);
        }
        if (context.CaseValueExtendedParameters || query.DivisionId.HasValue)
        {
            parameters.Add(ParameterCaseValueQuery.DivisionId, query.DivisionId, DbType.Int32);
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

        var counts = (await context.QueryAsync<long>(
            sql: query.StoredProcedure,
            param: parameters,
            commandType: CommandType.StoredProcedure)).ToList();
        var count = counts.Sum(x => x);
        return count;
    }
}
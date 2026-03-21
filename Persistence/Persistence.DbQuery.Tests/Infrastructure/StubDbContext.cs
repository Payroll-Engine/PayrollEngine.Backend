using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using SqlKata.Compilers;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

/// <summary>
/// Minimal IDbContext stub for tests using SQL Server syntax.
/// BuildAttributeQuery and BuildCollectionFromRaw return real SQL fragments
/// matching the Persistence.SqlServer implementation so that
/// AttributeQueryTests and LambdaAndInTests can assert on generated SQL.
/// All other members throw NotSupportedException.
/// </summary>
public sealed class StubDbContext : IDbContext
{
    /// <inheritdoc/>
    public string DateTimeType => "DATETIME2(7)";

    /// <inheritdoc/>
    public string DecimalType => "DECIMAL(28, 6)";

    /// <inheritdoc/>
    public string BuildAttributeQuery(string column, string valueAlias = null)
    {
        var dotIndex = column.IndexOf('.');
        var attribute = dotIndex >= 0 ? column[(dotIndex + 1)..] : column;

        return string.IsNullOrWhiteSpace(valueAlias)
            ? $"(SELECT value FROM OPENJSON(Attributes) WHERE [key] = '{attribute}') AS {column}"
            : $"(SELECT CAST(value AS {valueAlias}) FROM OPENJSON(Attributes) WHERE [key] = '{attribute}') AS {column}";
    }

    /// <inheritdoc/>
    /// <remarks>Mirrors Persistence.SqlServer.DbContext.BuildCollectionFromRaw.</remarks>
    public string BuildCollectionFromRaw(string columnName, bool isScalar, IReadOnlyList<string> propertyNames)
    {
        if (isScalar)
        {
            return $"OPENJSON([{columnName}])";
        }
        var withParts = propertyNames.Select(p => $"[{p}] NVARCHAR(MAX) '$.{p.ToLowerInvariant()}'");
        return $"OPENJSON([{columnName}]) WITH ({string.Join(", ", withParts)})";
    }

    /// <inheritdoc />
    /// <remarks>SQL Server uses OPENJSON EXISTS — returns null to signal the default path.</remarks>
    public (string RawSql, object[] Bindings)? BuildFlatObjectAnyWhere(
        string columnName,
        IReadOnlyList<(string Column, string Op, object Value)> conditions) => null;

    public Compiler QueryCompiler => throw new NotSupportedException();
    public bool StoredProcedureReturnValue => throw new NotSupportedException();
    public bool CaseValueExtendedParameters => throw new NotSupportedException();
    public string LastInsertIdSql => throw new NotSupportedException();
    public string QuoteIdentifier(string name) => throw new NotSupportedException();
    public Task<Exception> TestVersionAsync() => throw new NotSupportedException();
    public Task<Tenant> GetTenantAsync(int tenantId, string tenantIdentifier = null) => throw new NotSupportedException();
    public Exception TransformException(Exception exception) => throw new NotSupportedException();
    public Task<DatabaseInformation> GetDatabaseInformationAsync() => throw new NotImplementedException();
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) => throw new NotSupportedException();
    public Task<T> QueryFirstAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) => throw new NotSupportedException();
    public Task<T> QuerySingleAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) => throw new NotSupportedException();
    public Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) => throw new NotSupportedException();
    public Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) => throw new NotSupportedException();
    public Task BulkInsertAsync(DataTable dataTable) => throw new NotSupportedException();
    public Task UpdateStatisticsAsync() => throw new NotSupportedException();
    public Task UpdateStatisticsTargetedAsync() => throw new NotSupportedException();
}

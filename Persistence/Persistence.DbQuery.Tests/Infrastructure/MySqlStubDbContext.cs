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
/// Minimal IDbContext stub using MySQL syntax.
/// BuildAttributeQuery and BuildCollectionFromRaw return real SQL fragments
/// matching the Persistence.MySql implementation.
/// Used by MySqlLambdaTests to verify JSON_TABLE output.
/// All other members throw NotSupportedException.
/// </summary>
public sealed class MySqlStubDbContext : IDbContext
{
    /// <inheritdoc/>
    public string DateTimeType => "DATETIME(7)";

    /// <inheritdoc/>
    public string DecimalType => "DECIMAL(28, 6)";

    /// <inheritdoc/>
    public string BuildAttributeQuery(string column, string valueAlias = null)
    {
        var dotIndex = column.IndexOf('.');
        var attribute = dotIndex >= 0 ? column[(dotIndex + 1)..] : column;

        return string.IsNullOrWhiteSpace(valueAlias)
            ? $"JSON_UNQUOTE(JSON_EXTRACT(Attributes, '$.{attribute}')) AS {column}"
            : $"CAST(JSON_UNQUOTE(JSON_EXTRACT(Attributes, '$.{attribute}')) AS {valueAlias}) AS {column}";
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Mirrors Persistence.MySql.DbContext.BuildCollectionFromRaw.
    ///
    /// Scalar: JSON_TABLE(`col`, '$[*]' COLUMNS (value VARCHAR(255) PATH '$')) jt
    /// Key/value: JSON_TABLE(`col`, '$[*]' COLUMNS (`Key` VARCHAR(255) PATH '$.key', ...)) jt
    /// </remarks>
    public string BuildCollectionFromRaw(string columnName, bool isScalar, IReadOnlyList<string> propertyNames)
    {
        if (isScalar)
        {
            return $"JSON_TABLE(`{columnName}`, '$[*]' COLUMNS (value VARCHAR(255) PATH '$')) jt";
        }
        var cols = propertyNames.Select(p => $"`{p}` VARCHAR(255) PATH '$.{p.ToLowerInvariant()}'");
        return $"JSON_TABLE(`{columnName}`, '$[*]' COLUMNS ({string.Join(", ", cols)})) jt";
    }

    public Compiler QueryCompiler => throw new NotSupportedException();
    public bool StoredProcedureReturnValue => throw new NotSupportedException();
    public bool CaseValueExtendedParameters => throw new NotSupportedException();
    public string LastInsertIdSql => throw new NotSupportedException();
    public string QuoteIdentifier(string name) => $"`{name}`";
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

/// <summary>
/// Minimal IDbContext stub for tests — only DateTimeType and DecimalType are used
/// during attribute-column query generation (OPENJSON path). All other members throw.
/// </summary>
public sealed class StubDbContext : IDbContext
{
    /// <inheritdoc/>
    public string DateTimeType => "DATETIME2(7)";

    /// <inheritdoc/>
    public string DecimalType => "DECIMAL(28, 6)";

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

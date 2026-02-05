using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>Database context</summary>
public interface IDbContext
{

    #region Control

    /// <summary>The date time type name</summary>
    string DateTimeType { get; }

    /// <summary>The decimal type name</summary>
    string DecimalType { get; }

    /// <summary>Test the required database version</summary>
    Task<Exception> TestVersionAsync();

    /// <summary>Test for valid tenant</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="tenantIdentifier">The tenant identifier</param>
    Task<Tenant> GetTenantAsync(int tenantId, string tenantIdentifier = null);

    /// <summary>Transform a database exception</summary>
    Exception TransformException(Exception exception);

    #endregion

    #region Query

    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    Task<T> QuerySingleAsync<T>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a command asynchronously using Task.
    /// </summary>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="transaction">The transaction to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="T"/>.</returns>
    Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
        int? commandTimeout = null, CommandType? commandType = null);
    
    #endregion

    #region Bulk

    /// <summary>
    /// Insert bulk data.
    /// </summary>
    /// <param name="dataTable">The data to insert.</param>
    /// <remarks>Executed as transaction</remarks>
    System.Threading.Tasks.Task BulkInsertAsync(DataTable dataTable);

    #endregion

}
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using SqlKata.Compilers;

namespace PayrollEngine.Domain.Model;

/// <summary>Database context</summary>
public interface IDbContext
{

    #region Control

    /// <summary>The date time type name</summary>
    string DateTimeType { get; }

    /// <summary>The decimal type name</summary>
    string DecimalType { get; }

    /// <summary>SqlKata compiler for this database provider (SqlServer or MySql)</summary>
    Compiler QueryCompiler { get; }

    /// <summary>Build SQL fragment to extract a named attribute value from a JSON column</summary>
    string BuildAttributeQuery(string column, string valueAlias = null);

    /// <summary>Whether stored procedures support RETURN values (true for SqlServer, false for MySql)</summary>
    bool StoredProcedureReturnValue { get; }

    /// <summary>Whether CaseValue pivot SPs accept EmployeeId and Culture parameters (MySql only)</summary>
    bool CaseValueExtendedParameters { get; }

    /// <summary>Quote a database identifier (table or column name) for this provider</summary>
    string QuoteIdentifier(string name);

    /// <summary>SQL expression that returns the last inserted identity value</summary>
    string LastInsertIdSql { get; }

    /// <summary>Test the required database version</summary>
    Task<Exception> TestVersionAsync();

    /// <summary>Test for valid tenant</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="tenantIdentifier">The tenant identifier</param>
    Task<Tenant> GetTenantAsync(int tenantId, string tenantIdentifier = null);

    /// <summary>Transform a database exception</summary>
    Exception TransformException(Exception exception);

    /// <summary>Get database runtime information (type, name, server version)</summary>
    Task<DatabaseInformation> GetDatabaseInformationAsync();

    #endregion

    #region Query

    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    Task<T> QueryFirstAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    Task<T> QuerySingleAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute a command asynchronously using Task.
    /// </summary>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="T"/>.</returns>
    Task<T> ExecuteScalarAsync<T>(string sql, object param = null,
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

    #region Maintenance

    /// <summary>
    /// Update statistics for all tables using FULLSCAN.
    /// Prevents query plan degradation after large bulk imports (e.g. lookup values)
    /// or after accumulation of many payrun results.
    /// </summary>
    System.Threading.Tasks.Task UpdateStatisticsAsync();

    /// <summary>
    /// Update statistics for high-volume tables only using FULLSCAN.
    /// Covers: LookupValue, payrun result tables, case value tables.
    /// Faster than <see cref="UpdateStatisticsAsync"/> — use for automatic triggers after bulk imports.
    /// </summary>
    System.Threading.Tasks.Task UpdateStatisticsTargetedAsync();

    #endregion

}
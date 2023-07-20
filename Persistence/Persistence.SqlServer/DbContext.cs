﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using Dapper;

namespace PayrollEngine.Persistence.SqlServer;

public class DbContext : IDbContext
{
    private static System.Version MinVersion => new(0, 5, 1);

    // minimum command timeout is 30 seconds
    private const int MinCommandTimeout = 30;
    // maximum command timeout is 30 minutes
    private const int MaxCommandTimeout = 1800;

    /// <summary>The database connection string</summary>
    private string ConnectionString { get; }

    /// <summary>The default command timeout in seconds</summary>
    private int DefaultCommendTimeout { get; }

    /// <summary>
    /// New database connection
    /// </summary>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="defaultCommendTimeout">The default command timeout in seconds</param>
    public DbContext(string connectionString, int defaultCommendTimeout = 120)
    {
        // connection string
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(nameof(connectionString));
        }
        ConnectionString = connectionString;

        // default command timeout range limits
        if (defaultCommendTimeout < MinCommandTimeout)
        {
            defaultCommendTimeout = MinCommandTimeout;
        }
        else if (defaultCommendTimeout > MaxCommandTimeout)
        {
            defaultCommendTimeout = MaxCommandTimeout;
        }
        DefaultCommendTimeout = defaultCommendTimeout;
    }

    #region Control

    /// <inheritdoc />
    public string DateTimeType =>
        $"DATETIME2({SystemSpecification.DateTimeFractionalSecondsPrecision}";

    /// <inheritdoc />
    public string DecimalType =>
        $"DECIMAL({SystemSpecification.DecimalPrecision}, {SystemSpecification.DecimalScale})";

    /// <inheritdoc />
    public async Task<bool> TestVersionAsync()
    {
        try
        {
            // version sql query
            const string sql = $"SELECT COUNT(*) " +
                               $"FROM {nameof(Version)} " +
                               $"WHERE {nameof(Version.MajorVersion)} >= @major" +
                               $"  AND {nameof(Version.MinorVersion)} >= @minor " +
                               $"  AND {nameof(Version.SubVersion)} >= @build";

            // count greater versions
            var connection = new SqlConnection(ConnectionString);
            var count = await connection.QueryFirstAsync<int>(sql, new
            {
                major = MinVersion.Major,
                minor = MinVersion.Major,
                build = MinVersion.Build
            });

            // at least one version is required
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestTenantAsync(string tenantIdentifier, int tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            return true;
        }

        try
        {
            // tenant sql query
            var sql = $"SELECT {nameof(Tenant.Identifier)} " +
                      $"FROM {nameof(Tenant)} " +
                      $"WHERE {nameof(Tenant.Id)} = @id";

            // get test tenant identifier
            var connection = new SqlConnection(ConnectionString);
            var testTenant = (await connection.QueryAsync<Tenant>(sql, new { id = tenantId })).
                    FirstOrDefault();

            // compare tenant identifiers
            var valid = string.Equals(testTenant?.Identifier, tenantIdentifier);
            return valid;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Exception TransformException(Exception exception)
    {
        if (exception is SqlException sqlException)
        {
            var message = exception.GetBaseMessage();

            // sql 2601: Cannot insert duplicate key row
            // see http://www.sql-server-helper.com/error-messages/msg-2601.aspx
            if (sqlException.Number == 2601)
            {
                var startIndex = message.IndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                var endIndex = message.IndexOf(")", StringComparison.InvariantCultureIgnoreCase);
                if (startIndex > 0 && endIndex > startIndex)
                {
                    message = $"Unique index key already exists [{message.Substring(startIndex + 1, endIndex - startIndex - 1)}]";
                }
            }
            return new PersistenceException(message, PersistenceErrorType.UniqueConstraint, exception);
        }

        return exception;
    }

    #endregion

    #region Requests

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout ?? DefaultCommendTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> QueryFirstAsync<T>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout ?? DefaultCommendTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleAsync<T>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QuerySingleAsync<T>(sql, param, transaction, commandTimeout ?? DefaultCommendTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.ExecuteAsync(sql, param, transaction, commandTimeout ?? DefaultCommendTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout ?? DefaultCommendTimeout, commandType);
    }

    /// <summary>New database connection for SQL Server</summary>
    /// <returns>The connection</returns>
    private IDbConnection NewConnection() =>
        new SqlConnection(ConnectionString);

    #endregion

}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using Dapper;

namespace PayrollEngine.Persistence.SqlServer;

public class DbContext : IDbContext
{
    /// <summary>The database connection string</summary>
    private string ConnectionString { get; }

    /// <summary>The default command timeout in seconds</summary>
    private int DefaultCommendTimeout { get; }

    // minimum command timeout is 30 seconds
    private const int MinCommandTimeout = 30;
    // maximum command timeout is 30 minutes
    private const int MaxCommandTimeout = 1800;

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
        } else if (defaultCommendTimeout > MaxCommandTimeout)
        {
            defaultCommendTimeout = MaxCommandTimeout;
        }
        DefaultCommendTimeout = defaultCommendTimeout;
    }

    /// <summary>New database connection for SQL Server</summary>
    /// <returns>The connection</returns>
    private IDbConnection NewConnection() =>
        new SqlConnection(ConnectionString);

    /// <inheritdoc />
    public string DateTimeType =>
        $"DATETIME2({SystemSpecification.DateTimeFractionalSecondsPrecision}";

    /// <inheritdoc />
    public string DecimalType =>
        $"DECIMAL({SystemSpecification.DecimalPrecision}, {SystemSpecification.DecimalScale})";

    #region Operations

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

    #endregion

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
}
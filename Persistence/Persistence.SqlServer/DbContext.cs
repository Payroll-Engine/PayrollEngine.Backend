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
    public string ConnectionString { get; }

    public DbContext(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(nameof(connectionString));
        }
        ConnectionString = connectionString;
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

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.QuerySingleAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = NewConnection();
        return await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
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
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using Microsoft.Data.SqlClient;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.SqlServer;

/// <inheritdoc />
public class DbContext : IDbContext
{
    /// <summary>The current database version</summary>
    private static System.Version MinVersion => new(0, 9, 5);

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
    public async Task<Exception> TestVersionAsync()
    {
        try
        {
            // version sql query
            // order the existing version from new to old
            const string query = $"SELECT TOP 1 {nameof(Version.MajorVersion)}, " +
                                 $"{nameof(Version.MinorVersion)}, " +
                                 $"{nameof(Version.SubVersion)} " +
                                 $"FROM {nameof(Version)} " +
                                 $"ORDER BY {nameof(Version.MajorVersion)} DESC, " +
                                 $"{nameof(Version.MinorVersion)} DESC, " +
                                 $"{nameof(Version.SubVersion)} DESC";

            // test if the newest matches the minimum version criteria
            await using var connection = new SqlConnection(ConnectionString);

            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var major = reader.GetInt32(0);
                var minor = reader.GetInt32(1);
                var sub = reader.GetInt32(2);
                var newestVersion = new System.Version(major, minor, sub);
                if (newestVersion < MinVersion)
                {
                    throw new PayrollException($"Invalid database version {newestVersion}. Should be {MinVersion} or newer.");
                }
            }

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    /// <inheritdoc />
    public async Task<Tenant> GetTenantAsync(int tenantId, string tenantIdentifier = null)
    {
        if (tenantId <= 0)
        {
            return null;
        }

        try
        {
            // tenant sql query
            var sql = $"SELECT * FROM {nameof(Tenant)} " +
                      $"WHERE {nameof(Tenant.Id)} = @id";
            if (!string.IsNullOrWhiteSpace(tenantIdentifier))
            {
                sql += $" AND {nameof(Tenant.Identifier)} = @identifier";
            }

            // get test tenant identifier
            var connection = new SqlConnection(ConnectionString);
            var tenant = (await connection.QueryAsync<Tenant>(sql, new
            {
                id = tenantId,
                identifier = tenantIdentifier
            })).FirstOrDefault();
            return tenant;
        }
        catch
        {
            return null;
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

    #region Query

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

    #endregion

    #region Bulk

    /// <inheritdoc />
    public async Task BulkInsertAsync(DataTable dataTable)
    {
        await using var connection = NewSqlConnection();
        await connection.OpenAsync();

        await using var dbTransaction = connection.BeginTransaction();

        try
        {
            // bulk insert
            using var bulkCopy = new SqlBulkCopy(connection,
                copyOptions: SqlBulkCopyOptions.Default,
                externalTransaction: dbTransaction);
            bulkCopy.DestinationTableName = dataTable.TableName;

            foreach (DataColumn dataTableColumn in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(dataTableColumn.ColumnName, dataTableColumn.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);

            dbTransaction.Commit();
        }
        catch
        {
            dbTransaction.Rollback();
        }
    }

    #endregion

    /// <summary>New database connection for SQL Server</summary>
    /// <returns>The connection</returns>
    private IDbConnection NewConnection() => NewSqlConnection();

    /// <summary>New database connection for SQL Server</summary>
    /// <returns>The connection</returns>
    private SqlConnection NewSqlConnection() =>
        new(ConnectionString);
}
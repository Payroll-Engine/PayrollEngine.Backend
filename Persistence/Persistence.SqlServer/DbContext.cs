using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;
using Microsoft.Data.SqlClient;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.SqlServer;

/// <inheritdoc />
public class DbContext : IDbContext
{
    /// <summary>The current database version</summary>
    private static Version MinVersion => new(0, 9, 6);

    // minimum command timeout is 30 seconds
    private const int MinCommandTimeout = 30;
    // maximum command timeout is 30 minutes
    private const int MaxCommandTimeout = 1800;

    /// <summary>The database connection string</summary>
    private string ConnectionString { get; }

    /// <summary>The default command timeout in seconds</summary>
    private int DefaultCommendTimeout { get; }

    /// <summary>The required database collation</summary>
    private string RequiredCollation { get; }

    /// <summary>The default database collation</summary>
    private const string DefaultCollation = "SQL_Latin1_General_CP1_CS_AS";

    /// <summary>Shared connections per ambient TransactionScope.
    /// Prevents DTC promotion and deadlocks by reusing a single connection
    /// for the lifetime of each TransactionScope.</summary>
    private readonly ConcurrentDictionary<string, SqlConnection> scopedConnections = new();

    /// <summary>
    /// New database connection
    /// </summary>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="defaultCommendTimeout">The default command timeout in seconds</param>
    /// <param name="collation">The required database collation (default: SQL_Latin1_General_CP1_CS_AS)</param>
    public DbContext(string connectionString, int defaultCommendTimeout = 120,
        string collation = DefaultCollation)
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

        // collation
        RequiredCollation = collation ?? DefaultCollation;
    }

    #region Control

    /// <inheritdoc />
    public SqlKata.Compilers.Compiler QueryCompiler { get; } =
        new SqlKata.Compilers.SqlServerCompiler { UseLegacyPagination = false };

    /// <inheritdoc />
    public string BuildAttributeQuery(string column, string valueAlias = null)
    {
        var attribute = column.RemoveAttributePrefix();
        return string.IsNullOrWhiteSpace(valueAlias)
            ? $"(SELECT value FROM OPENJSON(Attributes) WHERE [key] = '{attribute}') AS {column}"
            : $"(SELECT CAST(value AS {valueAlias}) FROM OPENJSON(Attributes) WHERE [key] = '{attribute}') AS {column}";
    }

    /// <inheritdoc />
    /// <remarks>
    /// Scalar array — e.g. Divisions/any(d: d eq 'HR'):
    ///   OPENJSON([Divisions])
    ///   → exposes a built-in [value] column; no WITH clause required.
    ///
    /// Key/value object array — e.g. Attributes/any(a: a/Key eq 'K' and a/Value eq 'V'):
    ///   OPENJSON([Attributes]) WITH ([Key] NVARCHAR(MAX) '$.key', [Value] NVARCHAR(MAX) '$.value')
    ///   → exposes named typed columns matching the lambda property names.
    /// </remarks>
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
    /// <remarks>
    /// SQL Server uses OPENJSON([col]) without a WITH clause for flat JSON objects.
    /// The built-in [key] and [value] columns are available directly — no custom schema needed.
    /// Returns null to instruct FilterClauseBuilder to use the default OPENJSON EXISTS pattern.
    /// </remarks>
    public (string RawSql, object[] Bindings)? BuildFlatObjectAnyWhere(
        string columnName,
        IReadOnlyList<(string Column, string Op, object Value)> conditions) => null;

    /// <inheritdoc />
    public bool StoredProcedureReturnValue => true;

    /// <inheritdoc />
    public bool CaseValueExtendedParameters => false;

    /// <inheritdoc />
    public string QuoteIdentifier(string name) => $"[{name}]";

    /// <inheritdoc />
    public string LastInsertIdSql => "SELECT CAST(SCOPE_IDENTITY() as int);";

    /// <inheritdoc />
    public string DateTimeType =>
        $"DATETIME2({SystemSpecification.DateTimeFractionalSecondsPrecision})";

    /// <inheritdoc />
    public string DecimalType =>
        $"DECIMAL({SystemSpecification.DecimalPrecision}, {SystemSpecification.DecimalScale})";


    /// <inheritdoc />
    public async Task<Exception> TestVersionAsync()
    {
        try
        {
            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            // collation check
            await using var collationCommand = new SqlCommand(
                "SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS NVARCHAR(128))", connection);
            var collation = (string)await collationCommand.ExecuteScalarAsync();
            if (!string.Equals(collation, RequiredCollation, StringComparison.Ordinal))
            {
                throw new PayrollException(
                    $"Invalid database collation {collation}. Expected {RequiredCollation}.");
            }

            // version check
            // order the existing version from new to old
            await using var command = new SqlCommand(
                "SELECT TOP 1 MajorVersion, MinorVersion, SubVersion " +
                "FROM Version " +
                "ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC", connection);

            await using var reader = await command.ExecuteReaderAsync();
            Version maxVersion = null;
            while (await reader.ReadAsync())
            {
                var major = reader.GetInt32(0);
                var minor = reader.GetInt32(1);
                var sub = reader.GetInt32(2);
                var version = new Version(major, minor, sub);
                if (version < MinVersion)
                {
                    var message = $"Invalid database version {version}. Should be {MinVersion} or newer.";
                    Log.Critical(message);
                    throw new PayrollException(message);
                }

                if (maxVersion == null || maxVersion < version)
                {
                    maxVersion = version;
                }
            }

            Log.Debug($"Database version: {maxVersion}");

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
            await using var connection = new SqlConnection(ConnectionString);
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
    public async Task<DatabaseInformation> GetDatabaseInformationAsync()
    {
        var builder = new SqlConnectionStringBuilder(ConnectionString);
        var version = await ExecuteScalarAsync<string>(
            "SELECT CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(50))");
        var edition = await ExecuteScalarAsync<string>(
            "SELECT CAST(SERVERPROPERTY('Edition') AS NVARCHAR(128))");
        return new DatabaseInformation
        {
            Type = "SqlServer",
            Name = builder.InitialCatalog,
            Version = version,
            Edition = edition
        };
    }

    /// <inheritdoc />
    public Exception TransformException(Exception exception)
    {
        if (exception is not SqlException sqlException)
        {
            return exception;
        }

        var message = exception.GetBaseMessage();

        return sqlException.Number switch
        {
            // unique index violation
            // see http://www.sql-server-helper.com/error-messages/msg-2601.aspx
            2601 => new PersistenceException(
                FormatUniqueConstraintMessage(message),
                PersistenceErrorType.UniqueConstraint, exception),

            // primary key violation
            2627 => new PersistenceException(
                FormatUniqueConstraintMessage(message),
                PersistenceErrorType.UniqueConstraint, exception),

            // foreign key / check constraint violation
            547 => new PersistenceException(
                FormatConstraintMessage(message),
                PersistenceErrorType.ConstraintViolation, exception),

            // NOT NULL violation
            515 => new PersistenceException(
                FormatNotNullMessage(message),
                PersistenceErrorType.NotNullViolation, exception),

            _ => exception
        };
    }

    /// <summary>Format user-friendly message for unique constraint violations (SQL 2601/2627)</summary>
    private static string FormatUniqueConstraintMessage(string message)
    {
        // extract duplicate key values from: "...The duplicate key value is (value1, value2)."
        var match = Regex.Match(message, @"\((?<values>[^)]+)\)[^(]*$");
        return match.Success
            ? $"Duplicate entry: the value ({match.Groups["values"].Value}) already exists"
            : "Duplicate entry: a record with the same unique key already exists";
    }

    /// <summary>Format user-friendly message for foreign key/check constraint violations (SQL 547)</summary>
    private static string FormatConstraintMessage(string message)
    {
        var match = Regex.Match(message, @"table ""(?<table>[^""]+)""");
        return match.Success
            ? $"Constraint violation: referenced record in {match.Groups["table"].Value} not found or in use"
            : "Constraint violation: a database constraint was violated";
    }

    /// <summary>Format user-friendly message for NOT NULL violations (SQL 515)</summary>
    private static string FormatNotNullMessage(string message)
    {
        var match = Regex.Match(message, "column '(?<col>[^']+)'");
        return match.Success
            ? $"Required field '{match.Groups["col"].Value}' must not be empty"
            : "A required field is missing";
    }

    #endregion

    #region Query

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.QueryAsync<T>(sql, param, commandTimeout: commandTimeout ?? DefaultCommendTimeout, commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QueryFirstAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.QueryFirstAsync<T>(sql, param, commandTimeout: commandTimeout ?? DefaultCommendTimeout, commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.QuerySingleAsync<T>(sql, param, commandTimeout: commandTimeout ?? DefaultCommendTimeout, commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.ExecuteAsync(sql, param, commandTimeout: commandTimeout ?? DefaultCommendTimeout, commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.ExecuteScalarAsync<T>(sql, param, commandTimeout: commandTimeout ?? DefaultCommendTimeout, commandType: commandType);
    }

    #endregion

    #region Maintenance

    /// <inheritdoc />
    public async Task UpdateStatisticsAsync() =>
        await ExecuteAsync(DbSchema.Procedures.UpdateStatistics,
            commandTimeout: 600, commandType: CommandType.StoredProcedure);

    /// <inheritdoc />
    public async Task UpdateStatisticsTargetedAsync() =>
        await ExecuteAsync(DbSchema.Procedures.UpdateStatisticsTargeted,
            commandTimeout: 120, commandType: CommandType.StoredProcedure);

    #endregion

    #region Bulk

    /// <inheritdoc />
    public async Task BulkInsertAsync(DataTable dataTable)
    {
        var transaction = Transaction.Current;
        if (transaction != null)
        {
            // ambient transaction active → reuse scoped connection to prevent
            // DTC promotion and deadlocks
            var connection = GetOrCreateScopedConnection(transaction);

            using var bulkCopy = new SqlBulkCopy(connection,
                copyOptions: SqlBulkCopyOptions.TableLock,
                externalTransaction: null);
            bulkCopy.DestinationTableName = dataTable.TableName;

            foreach (DataColumn dataTableColumn in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(dataTableColumn.ColumnName, dataTableColumn.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);
        }
        else
        {
            // no ambient transaction → own connection with explicit transaction
            await using var connection = NewSqlConnection();
            await connection.OpenAsync();
            await using var dbTransaction = connection.BeginTransaction();

            using var bulkCopy = new SqlBulkCopy(connection,
                copyOptions: SqlBulkCopyOptions.TableLock,
                externalTransaction: dbTransaction);
            bulkCopy.DestinationTableName = dataTable.TableName;

            foreach (DataColumn dataTableColumn in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(dataTableColumn.ColumnName, dataTableColumn.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);

            dbTransaction.Commit();
        }
    }

    #endregion

    #region Connection Management

    /// <summary>Lease a database connection.
    /// When an ambient TransactionScope exists, returns a shared connection that stays
    /// alive for the scope's lifetime (prevents DTC promotion and deadlocks).
    /// When no ambient scope exists, returns a new caller-owned connection.</summary>
    private ConnectionLease LeaseConnection()
    {
        var transaction = Transaction.Current;
        if (transaction == null)
        {
            // no ambient scope → caller-owned, short-lived connection
            return new(NewSqlConnection(), owned: true);
        }

        // ambient scope → shared connection (one per scope)
        var connection = GetOrCreateScopedConnection(transaction);
        return new(connection, owned: false);
    }

    /// <summary>Get or create the shared SqlConnection for the current ambient transaction.
    /// The connection is opened immediately so Dapper won't close it after each operation,
    /// and is disposed automatically when the TransactionScope completes.</summary>
    private SqlConnection GetOrCreateScopedConnection(Transaction transaction)
    {
        var txId = transaction.TransactionInformation.LocalIdentifier;
        return scopedConnections.GetOrAdd(txId, _ =>
        {
            var connection = NewSqlConnection();
            connection.Open();
            transaction.TransactionCompleted += (_, _) =>
            {
                if (scopedConnections.TryRemove(txId, out var removed))
                {
                    removed.Dispose();
                }
            };
            return connection;
        });
    }

    /// <summary>New database connection for SQL Server</summary>
    private SqlConnection NewSqlConnection() => new(ConnectionString);

    /// <summary>Lightweight connection wrapper. When <c>owned</c> is true, Dispose() closes
    /// the connection. When false (shared within TransactionScope), Dispose() is a no-op.</summary>
    private readonly record struct ConnectionLease(IDbConnection connection, bool owned) : IDisposable
    {
        internal IDbConnection Connection => connection;
        public void Dispose()
        {
            if (owned)
            {
                connection?.Dispose();
            }
        }
    }

    #endregion
}

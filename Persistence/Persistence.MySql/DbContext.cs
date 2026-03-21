using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;
using MySqlConnector;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.MySql;

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
    private const string DefaultCollation = "utf8mb4_unicode_ci";

    /// <summary>Shared connections per ambient TransactionScope.</summary>
    private readonly ConcurrentDictionary<string, MySqlConnection> scopedConnections = new();

    /// <summary>
    /// New database connection
    /// </summary>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="defaultCommendTimeout">The default command timeout in seconds</param>
    /// <param name="collation">The required database collation (default: utf8mb4_unicode_ci)</param>
    public DbContext(string connectionString, int defaultCommendTimeout = 120,
        string collation = DefaultCollation)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(nameof(connectionString));
        }
        ConnectionString = connectionString;

        if (defaultCommendTimeout < MinCommandTimeout)
        {
            defaultCommendTimeout = MinCommandTimeout;
        }
        else if (defaultCommendTimeout > MaxCommandTimeout)
        {
            defaultCommendTimeout = MaxCommandTimeout;
        }
        DefaultCommendTimeout = defaultCommendTimeout;

        RequiredCollation = collation ?? DefaultCollation;
    }

    #region Control

    /// <inheritdoc />
    public SqlKata.Compilers.Compiler QueryCompiler { get; } =
        new SqlKata.Compilers.MySqlCompiler();

    /// <inheritdoc />
    public string BuildAttributeQuery(string column, string valueAlias = null)
    {
        var attribute = column.RemoveAttributePrefix();
        return string.IsNullOrWhiteSpace(valueAlias)
            ? $"JSON_UNQUOTE(JSON_EXTRACT(Attributes, '$.{attribute}')) AS {column}"
            : $"CAST(JSON_UNQUOTE(JSON_EXTRACT(Attributes, '$.{attribute}')) AS {valueAlias}) AS {column}";
    }

    /// <inheritdoc />
    /// <remarks>
    /// Scalar array — e.g. Divisions/any(d: d eq 'HR'):
    ///   JSON_TABLE(`Divisions`, '$[*]' COLUMNS (value VARCHAR(255) PATH '$')) jt
    ///   → exposes a single [value] column per array element; matches SQL Server OPENJSON column name.
    ///
    /// Key/value object array — e.g. Attributes/any(a: a/Key eq 'K' and a/Value eq 'V'):
    ///   JSON_TABLE(`Attributes`, '$[*]' COLUMNS (`Key` VARCHAR(255) PATH '$.key', `Value` VARCHAR(255) PATH '$.value')) jt
    ///   → exposes named columns matching the lambda property names.
    ///
    /// The alias <c>jt</c> is required by MySQL's JSON_TABLE syntax and is consistent across all usages.
    /// VARCHAR(255) covers the typical JSON string values stored in PE collections.
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

    /// <inheritdoc />
    /// <remarks>
    /// MySQL cannot use EXISTS + JSON_TABLE for flat JSON objects; it uses direct JSON functions instead.
    ///
    /// Key-only  (a/Key eq 'Dept'):
    ///   JSON_CONTAINS_PATH(`col`, 'one', CONCAT('$.', ?))
    ///   Checks whether the key exists in the flat JSON object.
    ///
    /// Key+Value (a/Key eq 'Dept' and a/Value eq 'HR'):
    ///   JSON_UNQUOTE(JSON_EXTRACT(`col`, CONCAT('$.', ?))) = ?
    ///   Reads the value at the given key and compares it.
    ///
    /// Value-only (a/Value eq 'HR'):
    ///   JSON_SEARCH(`col`, 'one', ?) IS NOT NULL
    ///   Returns the path of the first occurrence of the value.
    /// </remarks>
    public (string RawSql, object[] Bindings)? BuildFlatObjectAnyWhere(
        string columnName,
        IReadOnlyList<(string Column, string Op, object Value)> conditions)
    {
        var col = $"`{columnName}`";

        string keyVal = null;
        string valueVal = null;

        foreach (var (column, _, value) in conditions)
        {
            if (string.Equals(column, "Key", StringComparison.OrdinalIgnoreCase))
            {
                keyVal = value?.ToString();
            }
            else if (string.Equals(column, "Value", StringComparison.OrdinalIgnoreCase))
            {
                valueVal = value?.ToString();
            }
        }

        if (keyVal != null && valueVal != null)
        {
            return ($"JSON_UNQUOTE(JSON_EXTRACT({col}, CONCAT('$.', ?))) = ?",
                [keyVal, valueVal]);
        }

        if (keyVal != null)
        {
            return ($"JSON_CONTAINS_PATH({col}, 'one', CONCAT('$.', ?))",
                [keyVal]);
        }

        if (valueVal != null)
        {
            return ($"JSON_SEARCH({col}, 'one', ?) IS NOT NULL",
                [valueVal]);
        }

        return null;
    }

    /// <inheritdoc />
    public bool StoredProcedureReturnValue => false;

    /// <inheritdoc />
    public bool CaseValueExtendedParameters => true;

    /// <inheritdoc />
    public string QuoteIdentifier(string name) => $"`{name}`";

    /// <inheritdoc />
    public string LastInsertIdSql => "SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

    /// <inheritdoc />
    public string DateTimeType =>
        $"DATETIME({SystemSpecification.DateTimeFractionalSecondsPrecision})";

    /// <inheritdoc />
    public string DecimalType =>
        $"DECIMAL({SystemSpecification.DecimalPrecision}, {SystemSpecification.DecimalScale})";

    /// <inheritdoc />
    public async Task<Exception> TestVersionAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // collation check
            await using var collationCommand = new MySqlCommand(
                "SELECT DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA " +
                "WHERE SCHEMA_NAME = DATABASE()", connection);
            var collation = (string)await collationCommand.ExecuteScalarAsync();
            if (!string.Equals(collation, RequiredCollation, StringComparison.OrdinalIgnoreCase))
            {
                throw new PayrollException(
                    $"Invalid database collation {collation}. Expected {RequiredCollation}.");
            }

            // version check
            await using var command = new MySqlCommand(
                "SELECT MajorVersion, MinorVersion, SubVersion FROM `Version` " +
                "ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC LIMIT 1",
                connection);

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
            var sql = $"SELECT * FROM Tenant " +
                      $"WHERE Id = @id";
            if (!string.IsNullOrWhiteSpace(tenantIdentifier))
            {
                sql += " AND Identifier = @identifier";
            }

            await using var connection = new MySqlConnection(ConnectionString);
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
        var builder = new MySqlConnectionStringBuilder(ConnectionString);
        var version = await ExecuteScalarAsync<string>("SELECT VERSION()");
        var edition = await ExecuteScalarAsync<string>("SELECT @@version_comment");
        return new DatabaseInformation
        {
            Type = "MySql",
            Name = builder.Database,
            Version = version,
            Edition = edition
        };
    }

    /// <inheritdoc />
    public Exception TransformException(Exception exception)
    {
        if (exception is not MySqlException mysqlException)
        {
            return exception;
        }

        var message = exception.GetBaseMessage();

        return mysqlException.Number switch
        {
            // unique index violation (duplicate entry)
            1062 => new PersistenceException(
                FormatUniqueConstraintMessage(message),
                PersistenceErrorType.UniqueConstraint, exception),

            // foreign key constraint violation
            1451 or 1452 => new PersistenceException(
                FormatConstraintMessage(message),
                PersistenceErrorType.ConstraintViolation, exception),

            // NOT NULL violation
            1048 => new PersistenceException(
                FormatNotNullMessage(message),
                PersistenceErrorType.NotNullViolation, exception),

            _ => exception
        };
    }

    private static string FormatUniqueConstraintMessage(string message)
    {
        // MySQL: "Duplicate entry 'value' for key 'table.index'"
        var match = Regex.Match(message, "Duplicate entry '(?<values>[^']+)'");
        return match.Success
            ? $"Duplicate entry: the value ({match.Groups["values"].Value}) already exists"
            : "Duplicate entry: a record with the same unique key already exists";
    }

    private static string FormatConstraintMessage(string message)
    {
        var match = Regex.Match(message, "CONSTRAINT `(?<constraint>[^`]+)`");
        return match.Success
            ? $"Constraint violation: {match.Groups["constraint"].Value}"
            : "Constraint violation: a database constraint was violated";
    }

    private static string FormatNotNullMessage(string message)
    {
        var match = Regex.Match(message, "Column '(?<col>[^']+)'");
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
        var mappedParam = commandType == CommandType.StoredProcedure ? MapSpParameters(param) : param;
        return await lease.Connection.QueryAsync<T>(sql, mappedParam,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QueryFirstAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.QueryFirstAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.QuerySingleAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        var mappedParam = commandType == CommandType.StoredProcedure ? MapSpParameters(param) : param;
        return await lease.Connection.ExecuteAsync(sql, mappedParam,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        return await lease.Connection.ExecuteScalarAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <summary>
    /// Remap DbParameterCollection keys to MySQL stored procedure parameter names (p_ prefix, camelCase).
    /// SQL Server SPs use @TenantId, MySQL SPs use p_tenantId.
    /// Skips output/return-value parameters (they don't exist in MySQL SPs as IN params).
    /// </summary>
    private static object MapSpParameters(object param)
    {
        if (param is not DbParameterCollection dbParams)
        {
            return param;
        }

        var mapped = new DynamicParameters();
        foreach (var name in dbParams.ParameterNames)
        {
            // skip return-value / output placeholders (e.g. "@sp_return")
            // but allow input parameters that start with "@" (e.g. "@tenantId" from SQL Server conventions)
            var isReturnValue = name.StartsWith("@", StringComparison.Ordinal) &&
                                name.Contains("return", StringComparison.OrdinalIgnoreCase);
            if (isReturnValue)
            {
                continue;
            }

            // strip leading "@" if present (SQL Server convention), then convert to p_camelCase
            var cleanName = name.TrimStart('@');
            // convert PascalCase or camelCase → p_camelCase: "TenantId" → "p_tenantId", "tenantId" → "p_tenantId"
            var mysqlName = "p_" + char.ToLowerInvariant(cleanName[0]) + cleanName[1..];

            // use Get<object> — safe for both null and boxed value types
            object value;
            try { value = dbParams.Get<object>(name); }
            catch { value = null; }

            // SQL Server uses ##GlobalTempTable, MySQL uses plain temp table names
            if (value is string strValue)
            {
                value = strValue.Replace("##", string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            var dbType = dbParams.GetParameterType(name);
            mapped.Add(mysqlName, value, dbType);
        }
        return mapped;
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
        if (dataTable.Rows.Count == 0)
        {
            return;
        }

        // Build INSERT using backtick-quoted column names
        var table = $"`{dataTable.TableName}`";
        // Multi-row INSERT batched by 500 rows (well within MySQL's default 64 MB packet limit)
        const int batchSize = 500;
        var rows = dataTable.Rows.Cast<DataRow>().ToList();
        var cols = dataTable.Columns.Cast<DataColumn>().ToList();
        var colList = string.Join(",", cols.Select(c => $"`{c.ColumnName}`"));

        using var lease = LeaseConnection();
        for (var offset = 0; offset < rows.Count; offset += batchSize)
        {
            var batch = rows.Skip(offset).Take(batchSize).ToList();
            var valuesClauses = new List<string>(batch.Count);
            var batchParams = new DynamicParameters();

            for (var r = 0; r < batch.Count; r++)
            {
                var ri = r; // local copy — avoids captured-variable-modified warning
                var row = batch[ri];
                var placeholders = cols.Select(c => $"@p{ri}_{c.ColumnName}");
                valuesClauses.Add($"({string.Join(",", placeholders)})");
                foreach (var col in cols)
                {
                    batchParams.Add($"@p{ri}_{col.ColumnName}",
                        row[col] == DBNull.Value ? null : row[col]);
                }
            }

            var batchSql = $"INSERT INTO {table} ({colList}) VALUES {string.Join(",", valuesClauses)}";
            await lease.Connection.ExecuteAsync(batchSql, batchParams,
                commandTimeout: DefaultCommendTimeout);
        }
    }

    #endregion

    #region Connection Management

    private ConnectionLease LeaseConnection()
    {
        var transaction = Transaction.Current;
        if (transaction == null)
        {
            return new(NewMySqlConnection(), owned: true);
        }

        var connection = GetOrCreateScopedConnection(transaction);
        return new(connection, owned: false);
    }

    private MySqlConnection GetOrCreateScopedConnection(Transaction transaction)
    {
        var txId = transaction.TransactionInformation.LocalIdentifier;
        return scopedConnections.GetOrAdd(txId, _ =>
        {
            var connection = NewMySqlConnection();
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

    private MySqlConnection NewMySqlConnection() => new(ConnectionString);

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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;
using Npgsql;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.Postgres;

/// <inheritdoc />
public class DbContext : IDbContext
{
    /// <summary>The current database version</summary>
    private static Version MinVersion => new(1, 2, 0);

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
    private const string DefaultCollation = "en_US.UTF-8";

    /// <summary>Shared connections per ambient TransactionScope.</summary>
    private readonly ConcurrentDictionary<string, NpgsqlConnection> scopedConnections = new();

    /// <summary>Routine signatures by case-insensitive routine name.</summary>
    private readonly ConcurrentDictionary<string, RoutineSignature> routineSignatures =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Column types by table name, used to cast bulk insert parameters.</summary>
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> tableColumnTypes = new();

    /// <summary>
    /// New database connection
    /// </summary>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="defaultCommendTimeout">The default command timeout in seconds</param>
    /// <param name="collation">The required database collation (default: en_US.UTF-8)</param>
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
        new SqlKata.Compilers.PostgresCompiler();

    /// <inheritdoc />
    public string BuildAttributeQuery(string column, string valueAlias = null)
    {
        var attribute = column.RemoveAttributePrefix();
        return string.IsNullOrWhiteSpace(valueAlias)
            ? $"(\"Attributes\"::jsonb->>'{attribute}') AS \"{column}\""
            : $"(CAST(\"Attributes\"::jsonb->>'{attribute}' AS {valueAlias})) AS \"{column}\"";
    }

    /// <inheritdoc />
    /// <remarks>
    /// JSON columns are stored as TEXT and are cast to jsonb.
    ///
    /// Scalar array — e.g. Divisions/any(d: d eq 'HR'):
    ///   jsonb_array_elements_text("{columnName}"::jsonb) jt(value)
    ///   → exposes a single [value] column; matches SQL Server OPENJSON column name.
    ///
    /// Key/value object array — e.g. Attributes/any(a: a/Key eq 'K' and a/Value eq 'V'):
    ///   (SELECT e.obj->>'key' AS "Key", e.obj->>'value' AS "Value"
    ///    FROM jsonb_array_elements("{columnName}"::jsonb) AS e(obj)) jt
    ///   → exposes named columns matching the lambda property names.
    ///
    /// The alias <c>jt</c> is consistent across all usages matching the MySQL convention.
    /// </remarks>
    public string BuildCollectionFromRaw(string columnName, bool isScalar, IReadOnlyList<string> propertyNames)
    {
        if (isScalar)
        {
            return $"jsonb_array_elements_text(\"{columnName}\"::jsonb) jt(value)";
        }
        var cols = propertyNames.Select(p => $"e.obj->>'{p.ToLowerInvariant()}' AS \"{p}\"");
        return $"(SELECT {string.Join(", ", cols)} FROM jsonb_array_elements(\"{columnName}\"::jsonb) AS e(obj)) jt";
    }

    /// <inheritdoc />
    /// <remarks>
    /// PostgreSQL uses JSONB functions for flat JSON objects (TEXT column cast to jsonb).
    /// Bindings use the SqlKata <c>?</c> placeholder, so the jsonb <c>?</c> operator is avoided.
    ///
    /// Key-only  (a/Key eq 'Dept'):
    ///   jsonb_exists("{col}"::jsonb, ?)
    ///   Checks whether the key exists in the flat JSONB object.
    ///
    /// Key+Value (a/Key eq 'Dept' and a/Value eq 'HR'):
    ///   ("{col}"::jsonb->>?) = ?
    ///   Reads the value at the given key and compares it.
    ///
    /// Value-only (a/Value eq 'HR'):
    ///   EXISTS (SELECT 1 FROM jsonb_each_text("{col}"::jsonb) kv WHERE kv.value = ?)
    /// </remarks>
    public (string RawSql, object[] Bindings)? BuildFlatObjectAnyWhere(
        string columnName,
        IReadOnlyList<(string Column, string Op, object Value)> conditions)
    {
        var col = $"\"{columnName}\"::jsonb";

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
            return ($"({col}->>?) = ?",
                [keyVal, valueVal]);
        }

        if (keyVal != null)
        {
            return ($"jsonb_exists({col}, ?)",
                [keyVal]);
        }

        if (valueVal != null)
        {
            return ($"EXISTS (SELECT 1 FROM jsonb_each_text({col}) kv WHERE kv.value = ?)",
                [valueVal]);
        }

        return null;
    }

    /// <inheritdoc />
    public bool StoredProcedureReturnValue => false;

    /// <inheritdoc />
    public bool CaseValueExtendedParameters => true;

    /// <inheritdoc />
    public string QuoteIdentifier(string name) => $"\"{name}\"";

    /// <inheritdoc />
    public string LastInsertIdSql =>
        // ReSharper disable once RedundantStringInterpolation
        $"SELECT CAST(lastval() AS BIGINT);";

    /// <inheritdoc />
    public string DateTimeType =>
        $"TIMESTAMP({SystemSpecification.DateTimeFractionalSecondsPrecision})";

    /// <inheritdoc />
    public string DecimalType =>
        $"NUMERIC({SystemSpecification.DecimalPrecision}, {SystemSpecification.DecimalScale})";

    /// <inheritdoc />
    public async Task<Exception> TestVersionAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // collation check
            await using var collationCommand = new NpgsqlCommand(
                "SELECT datcollate FROM pg_database WHERE datname = current_database()", connection);
            var collation = (string)await collationCommand.ExecuteScalarAsync();
            if (!string.Equals(collation?.Replace("-", ""), RequiredCollation.Replace("-", ""), StringComparison.OrdinalIgnoreCase))
            {
                throw new PayrollException(
                    $"Invalid database collation {collation}. Expected {RequiredCollation}.");
            }

            // version check
            await using var command = new NpgsqlCommand(
                "SELECT \"MajorVersion\", \"MinorVersion\", \"SubVersion\" FROM \"Version\" " +
                "ORDER BY \"MajorVersion\" DESC, \"MinorVersion\" DESC, \"SubVersion\" DESC LIMIT 1",
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
            var sql = $"SELECT * FROM \"Tenant\" " +
                      $"WHERE \"Id\" = @id";
            if (!string.IsNullOrWhiteSpace(tenantIdentifier))
            {
                sql += " AND \"Identifier\" = @identifier";
            }

            await using var connection = new NpgsqlConnection(ConnectionString);
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
        var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
        var version = await ExecuteScalarAsync<string>("SELECT version()");
        var edition = await ExecuteScalarAsync<string>(
            "SELECT current_setting('server_version')");
        return new DatabaseInformation
        {
            Type = "Postgres",
            Name = builder.Database,
            Version = version,
            Edition = edition
        };
    }

    /// <inheritdoc />
    public Exception TransformException(Exception exception)
    {
        if (exception is not PostgresException pgException)
        {
            return exception;
        }

        var message = exception.GetBaseMessage();

        return pgException.SqlState switch
        {
            // unique constraint violation (23505)
            "23505" => new PersistenceException(
                FormatUniqueConstraintMessage(message),
                PersistenceErrorType.UniqueConstraint, exception),

            // foreign key constraint violation (23503)
            "23503" => new PersistenceException(
                FormatConstraintMessage(message),
                PersistenceErrorType.ConstraintViolation, exception),

            // NOT NULL violation (23502)
            "23502" => new PersistenceException(
                FormatNotNullMessage(message),
                PersistenceErrorType.NotNullViolation, exception),

            _ => exception
        };
    }

    private static string FormatUniqueConstraintMessage(string message)
    {
        // PostgreSQL: "duplicate key value violates unique constraint \"table_column_key\"\nDetail: Key (col)=(value) already exists."
        var match = Regex.Match(message,
            @"Key \((?<cols>[^)]+)\)=\((?<values>[^)]+)\)", RegexOptions.Singleline);
        return match.Success
            ? $"Duplicate entry: the value ({match.Groups["values"].Value}) already exists"
            : "Duplicate entry: a record with the same unique key already exists";
    }

    private static string FormatConstraintMessage(string message)
    {
        // ReSharper disable once GrammarMistakeInComment
        // PostgreSQL: "update or delete on table \"table\" violates foreign key constraint \"fk_name\" on table \"detail\""
        var match = Regex.Match(message, @"on table ""(?<table>[^""]+)""");
        return match.Success
            ? $"Constraint violation: referenced record in {match.Groups["table"].Value} not found or in use"
            : "Constraint violation: a database constraint was violated";
    }

    private static string FormatNotNullMessage(string message)
    {
        // PostgreSQL: "null value in column \"col\" of relation \"table\" violates not-null constraint"
        var match = Regex.Match(message, @"column ""(?<col>[^""]+)""");
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
        (sql, commandType) = await BuildRoutineCallAsync(lease.Connection, sql, param, commandType);
        return await lease.Connection.QueryAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        (sql, commandType) = await BuildRoutineCallAsync(lease.Connection, sql, param, commandType);
        return await lease.Connection.ExecuteAsync(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QueryFirstAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        (sql, commandType) = await BuildRoutineCallAsync(lease.Connection, sql, param, commandType);
        return await lease.Connection.QueryFirstAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        (sql, commandType) = await BuildRoutineCallAsync(lease.Connection, sql, param, commandType);
        return await lease.Connection.QuerySingleAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using var lease = LeaseConnection();
        (sql, commandType) = await BuildRoutineCallAsync(lease.Connection, sql, param, commandType);
        return await lease.Connection.ExecuteScalarAsync<T>(sql, param,
            commandTimeout: commandTimeout ?? DefaultCommendTimeout,
            commandType: commandType);
    }

    #endregion

    #region Routine Calls

    /// <summary>Input signature of a stored function or procedure</summary>
    private sealed record RoutineSignature(string Name, bool IsFunction,
        IReadOnlyList<string> ArgumentNames, IReadOnlyList<string> ArgumentTypes);

    /// <summary>Convert a stored procedure command into a positional function or procedure call.
    /// Arguments are ordered by the database signature: parameters unknown to the routine
    /// are ignored and missing arguments are passed as typed NULL.</summary>
    private async Task<(string Sql, CommandType? CommandType)> BuildRoutineCallAsync(
        IDbConnection connection, string sql, object param, CommandType? commandType)
    {
        if (commandType != CommandType.StoredProcedure)
        {
            return (sql, commandType);
        }

        var signature = await GetRoutineSignatureAsync(connection, sql);
        var parameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (param is DynamicParameters dynamicParameters)
        {
            foreach (var name in dynamicParameters.ParameterNames)
            {
                parameterNames.Add(name.TrimStart('@'));
            }
        }

        var arguments = new List<string>();
        for (var i = 0; i < signature.ArgumentNames.Count; i++)
        {
            var type = signature.ArgumentTypes[i];
            // procedure arguments use the p_ prefix (p_tenantId for @tenantId)
            var name = signature.ArgumentNames[i];
            if (name != null && !parameterNames.Contains(name) &&
                name.StartsWith("p_", StringComparison.OrdinalIgnoreCase))
            {
                name = name[2..];
            }
            var parameterName = name == null ? null : parameterNames.FirstOrDefault(
                x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
            arguments.Add(parameterName != null
                ? $"@{parameterName}::{type}"
                : $"NULL::{type}");
        }

        var routine = $"\"{signature.Name}\"({string.Join(", ", arguments)})";
        var callSql = signature.IsFunction ? $"SELECT * FROM {routine}" : $"CALL {routine}";
        return (callSql, CommandType.Text);
    }

    /// <summary>Get the routine signature from the system catalog (cached)</summary>
    private async Task<RoutineSignature> GetRoutineSignatureAsync(IDbConnection connection, string routineName)
    {
        if (routineSignatures.TryGetValue(routineName, out var cached))
        {
            return cached;
        }

        // input arguments: modes IN, INOUT and VARIADIC (null modes = all arguments are IN)
        const string query =
            """
            SELECT p.proname AS "Name",
                   p.prokind = 'f' AS "IsFunction",
                   ARRAY(SELECT a.name
                         FROM unnest(COALESCE(p.proargnames, array_fill(NULL::text, ARRAY[p.pronargs::int])),
                                     COALESCE(p.proargmodes, array_fill('i'::"char", ARRAY[p.pronargs::int])))
                              WITH ORDINALITY AS a(name, mode, n)
                         WHERE a.mode IN ('i', 'b', 'v')
                         ORDER BY a.n) AS "ArgumentNames",
                   ARRAY(SELECT format_type(t.oid, NULL)
                         FROM unnest(p.proargtypes::oid[]) WITH ORDINALITY AS t(oid, n)
                         ORDER BY t.n) AS "ArgumentTypes"
            FROM pg_proc p
            INNER JOIN pg_namespace ns ON ns.oid = p.pronamespace
            WHERE ns.nspname = current_schema()
              AND lower(p.proname) = lower(@routineName)
              AND p.prokind IN ('f', 'p')
            ORDER BY (p.proname = @routineName) DESC
            LIMIT 1
            """;
        var row = await connection.QueryFirstOrDefaultAsync<(string Name, bool IsFunction,
            string[] ArgumentNames, string[] ArgumentTypes)>(query, new { routineName });
        if (row.Name == null)
        {
            throw new InvalidOperationException($"Unknown stored routine {routineName}.");
        }

        var signature = new RoutineSignature(row.Name, row.IsFunction, row.ArgumentNames, row.ArgumentTypes);
        routineSignatures.TryAdd(routineName, signature);
        return signature;
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

        var transaction = Transaction.Current;
        if (transaction != null)
        {
            var connection = GetOrCreateScopedConnection(transaction);
            await BulkInsertAsync(connection, dataTable);
        }
        else
        {
            await using var connection = NewNpgsqlConnection();
            await connection.OpenAsync();
            await using var dbTransaction = await connection.BeginTransactionAsync();
            await BulkInsertAsync(connection, dataTable);
            await dbTransaction.CommitAsync();
        }
    }

    /// <summary>Insert the data table rows using batched multi-row INSERT statements</summary>
    private async Task BulkInsertAsync(NpgsqlConnection connection, DataTable dataTable)
    {
        // PostgreSQL COPY is the fastest bulk-insert path but requires server-side file access.
        // Use multi-row INSERT with batched values — practical for cross-platform compat.
        var table = $"\"{dataTable.TableName}\"";
        const int batchSize = 500;
        var rows = dataTable.Rows.Cast<DataRow>().ToList();
        // identity columns are GENERATED ALWAYS and reject explicit values
        var cols = dataTable.Columns.Cast<DataColumn>()
            .Where(c => c.ColumnName != DbSchema.ObjectColumn.Id)
            .ToList();
        var colList = string.Join(",", cols.Select(c => $"\"{c.ColumnName}\""));

        // untyped parameters (e.g. null values) are sent as text and require a cast to the column type
        var columnTypes = await GetColumnTypesAsync(connection, dataTable.TableName);

        for (var offset = 0; offset < rows.Count; offset += batchSize)
        {
            var batch = rows.Skip(offset).Take(batchSize).ToList();
            var valuesClauses = new List<string>(batch.Count);
            var batchParams = new DynamicParameters();

            for (var r = 0; r < batch.Count; r++)
            {
                var ri = r;
                var row = batch[ri];
                var placeholders = cols.Select(c =>
                {
                    var placeholder = $"@p{ri}_{c.ColumnName.Replace(" ", "_")}";
                    return columnTypes.TryGetValue(c.ColumnName, out var type)
                        ? $"{placeholder}::{type}"
                        : placeholder;
                });
                valuesClauses.Add($"({string.Join(",", placeholders)})");
                foreach (var col in cols)
                {
                    batchParams.Add($"@p{ri}_{col.ColumnName.Replace(" ", "_")}",
                        row[col] == DBNull.Value ? null : row[col]);
                }
            }

            var batchSql = $"INSERT INTO {table} ({colList}) VALUES {string.Join(",", valuesClauses)}";
            await connection.ExecuteAsync(batchSql, batchParams,
                commandTimeout: DefaultCommendTimeout);
        }
    }

    /// <summary>Get the column types of a table from the system catalog (cached)</summary>
    private async Task<IReadOnlyDictionary<string, string>> GetColumnTypesAsync(
        NpgsqlConnection connection, string tableName)
    {
        if (tableColumnTypes.TryGetValue(tableName, out var cached))
        {
            return cached;
        }

        // type modifiers are omitted: length limits are enforced by the column itself
        const string query =
            """
            SELECT a.attname AS "Name", format_type(a.atttypid, NULL) AS "Type"
            FROM pg_attribute a
            INNER JOIN pg_class c ON c.oid = a.attrelid
            INNER JOIN pg_namespace ns ON ns.oid = c.relnamespace
            WHERE ns.nspname = current_schema()
              AND c.relname = @tableName
              AND a.attnum > 0
              AND NOT a.attisdropped
            """;
        var columns = await connection.QueryAsync<(string Name, string Type)>(query, new { tableName });
        var columnTypes = columns.ToDictionary(x => x.Name, x => x.Type, StringComparer.Ordinal);
        tableColumnTypes.TryAdd(tableName, columnTypes);
        return columnTypes;
    }

    #endregion

    #region Connection Management

    /// <summary>Lease a database connection.
    /// When an ambient TransactionScope exists, returns a shared connection that stays
    /// alive for the scope's lifetime (prevents escalation and deadlocks).
    /// When no ambient scope exists, returns a new caller-owned connection.</summary>
    private ConnectionLease LeaseConnection()
    {
        var transaction = Transaction.Current;
        if (transaction == null)
        {
            // no ambient scope → caller-owned, short-lived connection
            return new(NewNpgsqlConnection(), owned: true);
        }

        // ambient scope → shared connection (one per scope)
        var connection = GetOrCreateScopedConnection(transaction);
        return new(connection, owned: false);
    }

    /// <summary>Get or create the shared NpgsqlConnection for the current ambient transaction.
    /// The connection is opened immediately so Dapper won't close it after each operation,
    /// and is disposed automatically when the TransactionScope completes.</summary>
    private NpgsqlConnection GetOrCreateScopedConnection(Transaction transaction)
    {
        var txId = transaction.TransactionInformation.LocalIdentifier;
        return scopedConnections.GetOrAdd(txId, _ =>
        {
            var connection = NewNpgsqlConnection();
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

    /// <summary>New database connection for PostgreSQL</summary>
    private NpgsqlConnection NewNpgsqlConnection() => new(ConnectionString);

    /// <summary>Lightweight connection wrapper. When <c>owned</c> is true, Dispose() closes
    /// the connection. When false (shared within TransactionScope), Dispose() is a no-op.</summary>
    // ReSharper disable once InconsistentNaming
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

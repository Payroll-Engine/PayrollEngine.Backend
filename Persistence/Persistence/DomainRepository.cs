using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class DomainRepository<T>(string tableName) : TableRepository(tableName), IDomainRepository
    where T : IDomainObject
{
    // test if the domain object type declares the attribute object interface

    #region Query

    /// <summary>
    /// Execute a query
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    protected async Task<IEnumerable<T>> QueryAsync(IDbContext context, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await QueryAsync<T>(context, sql, param, transaction, commandTimeout, commandType);

    /// <summary>
    /// Execute an item query
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <typeparam name="TItem">The item to query</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    protected async Task<IEnumerable<TItem>> QueryAsync<TItem>(IDbContext context, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await context.QueryAsync<TItem>(sql, param, transaction, commandTimeout, commandType);

    /// <summary>
    /// Execute an single item query
    /// </summary>
    /// <typeparam name="TResult">The type of results to return.</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    protected async Task<TResult> QuerySingleAsync<TResult>(IDbContext context, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await context.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType);

    #endregion

    #region Case Value (Shared)

    /// <summary>
    /// Execute item query in case query attributes are present
    /// </summary>
    /// <typeparam name="TItem">The type of results to return.</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc.) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case-insensitive).
    /// </returns>
    internal async Task<IEnumerable<TItem>> QueryCaseValuesAsync<TItem>(IDbContext context, CaseValueQuery query) =>
        await new CaseValueResultCommand().QueryCaseValuesAsync<TItem>(context, query);

    /// <summary>
    /// Execute item count query in case query attributes are present
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <returns>The record count matching the query criteria</returns>
    internal async Task<long> QueryCaseValueCountAsync(IDbContext context, CaseValueQuery query) =>
        await new CaseValueResultCountCommand().QueryCaseValuesCountAsync(context, query);

    #endregion

    #region Execute

    /// <summary>
    /// Execute a command asynchronously using Task
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute for this query</param>
    /// <param name="param">The parameters to use for this query</param>
    /// <param name="transaction">The transaction to use for this query</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected</returns>
    protected async Task<int> ExecuteAsync(IDbContext context, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await context.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);

    /// <summary>
    /// Execute parameterized SQL that selects a single value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute</param>
    /// <param name="param">The parameters to use for this command</param>
    /// <param name="transaction">The transaction to use for this command</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="T"/></returns>
    protected async Task<TValue> ExecuteScalarAsync<TValue>(IDbContext context, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await context.ExecuteScalarAsync<TValue>(sql, param, transaction, commandTimeout, commandType);

    /// <summary>
    /// Execute parameterized SQL that selects a single value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL to execute</param>
    /// <param name="param">The parameters to use for this command</param>
    /// <param name="transaction">The transaction to use for this command</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <see cref="object"/></returns>
    protected async Task<object> ExecuteScalarAsync(IDbContext context, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await context.ExecuteScalarAsync<object>(sql, param, transaction, commandTimeout, commandType);

    #endregion

    #region Db Scripting

    protected string GetIdColumnName() =>
        GetColumnName(DbSchema.ObjectColumn.Id);

    protected string GetIdColumnName(string tableName) =>
        GetColumnName(tableName, DbSchema.ObjectColumn.Id);

    protected string GetColumnName(string columnName) =>
        GetColumnName(TableName, columnName);

    protected string GetColumnName(string tableName, string columnName) =>
        $"{tableName}.{columnName}";

    #endregion

    #region Database

    /// <summary>
    /// Ensure valid update date
    /// </summary>
    /// <param name="obj"></param>
    protected DateTime GetValidUpdatedDate(T obj) =>
        obj.Updated < obj.Created ? obj.Created : obj.Updated;

    protected virtual void GetObjectData(T obj, DbParameterCollection parameters)
    {
    }

    protected virtual void GetObjectCreateData(T obj, DbParameterCollection parameters)
    {
    }

    protected void GetObjectUpdateData()
    {
    }

    #endregion

    #region Get/Retrieve

    public virtual async Task<bool> ExistsAsync(IDbContext context, int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        // query
        var query = DbQueryFactory.NewCountQuery(TableName, id);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var count = await context.ExecuteScalarAsync<int>(compileQuery);
        return count == 1;
    }

    public virtual async Task<bool> ExistsAsync(IDbContext context, string testColumn, object testValue)
    {
        if (string.IsNullOrWhiteSpace(testColumn))
        {
            throw new ArgumentException(nameof(testColumn));
        }

        var objects = await SelectAsync<T>(context, TableName, testColumn, testValue);
        return objects.Any();
    }

    public async Task<bool> ExistsAnyAsync<TValue>(IDbContext context, string testColumn, IEnumerable<TValue> testValues)
    {
        if (string.IsNullOrWhiteSpace(testColumn))
        {
            throw new ArgumentException(nameof(testColumn));
        }
        if (testValues == null)
        {
            throw new ArgumentNullException(nameof(testValues));
        }

        // query
        var query = DbQueryFactory.NewCountQuery(TableName)
            .WhereIn(testColumn, testValues);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var count = await context.ExecuteScalarAsync<int>(compileQuery);
        return count > 0;
    }

    protected async Task<bool> ExistsAnyAsync<TValue>(IDbContext context, string parentColumn, int parentId,
        string testColumn, IEnumerable<TValue> testValues)
    {
        if (string.IsNullOrWhiteSpace(parentColumn))
        {
            throw new ArgumentException(nameof(parentColumn));
        }
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (string.IsNullOrWhiteSpace(testColumn))
        {
            throw new ArgumentException(nameof(testColumn));
        }
        if (testValues == null)
        {
            throw new ArgumentNullException(nameof(testValues));
        }

        // query
        var query = DbQueryFactory.NewCountQuery(TableName, parentColumn, parentId)
            .WhereIn(testColumn, testValues);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var count = await context.ExecuteScalarAsync<int>(compileQuery);
        return count > 0;
    }

    #endregion

    #region Attributes

    private bool IsAttributeObject { get; } = typeof(IAttributeObject).IsAssignableFrom(typeof(T));

    public virtual async Task<string> GetAttributeAsync(IDbContext context, int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(context, id);
        if (attributeObject?.Attributes == null || !attributeObject.Attributes.TryGetValue(attributeName, out var attribute))
        {
            return null;
        }
        return DefaultJsonSerializer.Serialize(attribute);
    }

    public virtual async Task<bool> ExistsAttributeAsync(IDbContext context, int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(context, id);
        return attributeObject?.Attributes != null && attributeObject.Attributes.ContainsKey(attributeName);
    }

    public virtual async Task<string> SetAttributeAsync(IDbContext context, int id, string attributeName, string value)
    {
        var attributeObject = await GetAttributeObjectAsync(context, id);
        if (attributeObject != null)
        {
            attributeObject.Attributes ??= new();
            attributeObject.Attributes[attributeName] = DefaultJsonSerializer.Deserialize<object>(value);
            await UpdateObjectAttributeAsync(context, attributeObject);
        }
        return await GetAttributeAsync(context, id, attributeName);
    }

    public virtual async Task<bool?> DeleteAttributeAsync(IDbContext context, int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(context, id);
        if (attributeObject?.Attributes == null || !attributeObject.Attributes.Remove(attributeName))
        {
            return null;
        }

        await UpdateObjectAttributeAsync(context, attributeObject);
        return await UpdateObjectAttributeAsync(context, attributeObject);
    }

    private async Task<IDomainAttributeObject> GetAttributeObjectAsync(IDbContext context, int id)
    {
        IDomainAttributeObject attributeObject = null;
        if (IsAttributeObject && id > 0)
        {
            var query = DbQueryFactory.NewQuery(TableName, id).
                Select(
                    DbSchema.ObjectColumn.Id,
                    DbSchema.ObjectColumn.Status,
                    DbSchema.ObjectColumn.Created,
                    DbSchema.ObjectColumn.Updated,
                    DbSchema.AttributeObjectColumn.Attributes);
            var compileQuery = CompileQuery(query);
            attributeObject = (await QueryAsync<T>(context, compileQuery)).FirstOrDefault() as IDomainAttributeObject;
        }
        return attributeObject;
    }

    private async Task<bool> UpdateObjectAttributeAsync(IDbContext context, IDomainAttributeObject attributeObject)
    {
        // do not update inactive objects
        if (attributeObject.Status == ObjectStatus.Inactive)
        {
            return false;
        }

        // refresh update date and attributes
        attributeObject.Updated = Date.Now;
        var attributes = JsonSerializer.SerializeNamedDictionary(attributeObject.Attributes);
        var parameters = new DbParameterCollection();
        parameters.AddUpdated(attributeObject.Updated);
        parameters.Add(DbSchema.AttributeObjectColumn.Attributes, attributes);

        // build sql statement
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), attributeObject.Id);
        var dbQuery = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        // UPDATE execution
        await context.ExecuteAsync(dbQuery, new
        {
            attributeObject.Updated,
            Attributes = attributes
        });
        txScope.Complete();
        return true;
    }

    #endregion
}
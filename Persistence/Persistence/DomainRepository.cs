using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public abstract class DomainRepository<T> : TableRepository, IDomainRepository
    where T : IDomainObject
{
    protected DomainRepository(string tableName, IDbContext context) :
        base(tableName, context)
    {
        // test if the domain object type declares the attribute object interface
        IsAttributeObject = typeof(IAttributeObject).IsAssignableFrom(typeof(T));
    }

    #region Query

    /// <summary>
    /// Execute a query
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public virtual async Task<IEnumerable<T>> QueryAsync(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        await QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);

    /// <summary>
    /// Execute an item query
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <typeparam name="TItem">The item to query</typeparam>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public virtual async Task<IEnumerable<TItem>> QueryAsync<TItem>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        return await Connection.QueryAsync<TItem>(sql, param, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute an single item query
    /// </summary>
    /// <typeparam name="TResult">The type of results to return.</typeparam>
    /// <param name="sql">The SQL to execute for the query</param>
    /// <param name="param">The parameters to pass, if any</param>
    /// <param name="transaction">The transaction to use, if any</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public virtual async Task<TResult> QuerySingleAsync<TResult>(string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        return await Connection.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType);
    }

    #endregion

    #region Case Value (Shared)

    /// <summary>
    /// Execute a item query in case query attributes are present
    /// </summary>
    /// <typeparam name="TItem">The type of results to return.</typeparam>
    /// <param name="query">The case value query</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal virtual async Task<IEnumerable<TItem>> QueryCaseValuesAsync<TItem>(CaseValueQuery query) =>
        await new CaseValueResultCommand(Connection).QueryCaseValuesAsync<TItem>(query);

    /// <summary>
    /// Execute a item count query in case query attributes are present
    /// </summary>
    /// <param name="query">The case value query</param>
    /// <returns>The record count matching the query criteria</returns>
    internal virtual async Task<long> QueryCaseValueCountAsync(CaseValueQuery query) =>
        await new CaseValueResultCountCommand(Connection).QueryCaseValuesCountAsync(query);

    #endregion

    #region Execute

    /// <summary>
    /// Execute a command asynchronously using Task
    /// </summary>
    /// <param name="sql">The SQL to execute for this query</param>
    /// <param name="param">The parameters to use for this query</param>
    /// <param name="transaction">The transaction to use for this query</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected</returns>
    public Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        Connection.ExecuteAsync(new(sql, param, transaction, commandTimeout, commandType));

    /// <summary>
    /// Execute parameterized SQL that selects a single value
    /// </summary>
    /// <param name="sql">The SQL to execute</param>
    /// <param name="param">The parameters to use for this command</param>
    /// <param name="transaction">The transaction to use for this command</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="T"/></returns>
    public Task<TValue> ExecuteScalarAsync<TValue>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        Connection.ExecuteScalarAsync<TValue>(new(sql, param, transaction, commandTimeout, commandType));


    /// <summary>
    /// Execute parameterized SQL that selects a single value
    /// </summary>
    /// <param name="sql">The SQL to execute</param>
    /// <param name="param">The parameters to use for this command</param>
    /// <param name="transaction">The transaction to use for this command</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <see cref="object"/></returns>
    public Task<object> ExecuteScalarAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) =>
        Connection.ExecuteScalarAsync(new(sql, param, transaction, commandTimeout, commandType));

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
    /// <returns></returns>
    protected DateTime GetValidUpdatedDate(T obj) =>
        obj.Updated < obj.Created ? obj.Created : obj.Updated;

    protected virtual void GetObjectData(T obj, DbParameterCollection parameters)
    {
    }

    protected virtual void GetObjectCreateData(T obj, DbParameterCollection parameters)
    {
    }

    protected virtual void GetObjectUpdateData(T obj, DbParameterCollection parameters)
    {
    }

    #endregion

    #region Get/Retrieve

    public virtual async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        // query
        var query = DbQueryFactory.NewCountQuery(TableName, id);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var count = await Connection.ExecuteScalarAsync<int>(compileQuery);
        return count == 1;
    }

    public virtual async Task<bool> ExistsAsync(string testColumn, object testValue)
    {
        if (string.IsNullOrWhiteSpace(testColumn))
        {
            throw new ArgumentException(nameof(testColumn));
        }

        var objects = await SelectAsync<T>(TableName, testColumn, testValue);
        return objects.Any();
    }

    public virtual async Task<bool> ExistsAnyAsync<TValue>(string testColumn, IEnumerable<TValue> testValues)
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
        var count = await Connection.ExecuteScalarAsync<int>(compileQuery);
        return count > 0;
    }

    public virtual async Task<bool> ExistsAnyAsync<TValue>(string parentColumn, int parentId,
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
        var count = await Connection.ExecuteScalarAsync<int>(compileQuery);
        return count > 0;
    }

    #endregion

    #region Attributes

    public bool IsAttributeObject { get; }

    public virtual async Task<string> GetAttributeAsync(int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(id);
        if (attributeObject?.Attributes == null || !attributeObject.Attributes.ContainsKey(attributeName))
        {
            return null;
        }
        return DefaultJsonSerializer.Serialize(attributeObject.Attributes[attributeName]);
    }

    public virtual async Task<bool> ExistsAttributeAsync(int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(id);
        return attributeObject?.Attributes != null && attributeObject.Attributes.ContainsKey(attributeName);
    }

    public virtual async Task<string> SetAttributeAsync(int id, string attributeName, string value)
    {
        var attributeObject = await GetAttributeObjectAsync(id);
        if (attributeObject != null)
        {
            attributeObject.Attributes ??= new();
            attributeObject.Attributes[attributeName] = DefaultJsonSerializer.Deserialize<object>(value);
            await UpdateObjectAttributeAsync(attributeObject);
        }
        return await GetAttributeAsync(id, attributeName);
    }

    public virtual async Task<bool?> DeleteAttributeAsync(int id, string attributeName)
    {
        var attributeObject = await GetAttributeObjectAsync(id);
        if (attributeObject?.Attributes == null || !attributeObject.Attributes.Remove(attributeName))
        {
            return null;
        }

        await UpdateObjectAttributeAsync(attributeObject);
        return await UpdateObjectAttributeAsync(attributeObject);
    }

    private async Task<IDomainAttributeObject> GetAttributeObjectAsync(int id)
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
            attributeObject = (await QueryAsync<T>(compileQuery)).FirstOrDefault() as IDomainAttributeObject;
        }
        return attributeObject;
    }

    private async Task<bool> UpdateObjectAttributeAsync(IDomainAttributeObject attributeObject)
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
        parameters.Add(DbSchema.ObjectColumn.Updated, attributeObject.Updated);
        parameters.Add(DbSchema.AttributeObjectColumn.Attributes, attributes);

        // build sql statement
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.ParameterNames.ToList(), attributeObject.Id);
        var dbQuery = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        await Connection.ExecuteAsync(dbQuery, new
        {
            attributeObject.Updated,
            Attributes = attributes
        });
        txScope.Complete();
        return true;
    }

    #endregion
}
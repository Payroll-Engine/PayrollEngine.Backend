using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

/// <summary>
/// Abstract base repository for child domain objects that belong to a parent entity.
/// Provides parent-scoped query, get, create, update, and delete operations.
/// </summary>
/// <typeparam name="T">Child domain object type (e.g. CaseField under Case, PayrollLayer under Payroll)</typeparam>
public abstract class ChildDomainRepository<T> : DomainRepository<T>, IChildDomainRepository<T>
    where T : IDomainObject
{
    /// Bulk chunk size
    private int BulkChunkSize { get; } = 5000;

    /// <summary>
    /// The parent field name
    /// </summary>
    protected string ParentFieldName { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="parentFieldName">The parent field name</param>
    protected ChildDomainRepository(string tableName, string parentFieldName) :
        base(tableName)
    {
        if (string.IsNullOrWhiteSpace(parentFieldName))
        {
            throw new ArgumentException(nameof(parentFieldName));
        }

        ParentFieldName = parentFieldName;
    }

    #region Query/Get

    /// <summary>
    /// Set up the database query
    /// </summary>
    /// <param name="dbQuery">The database query</param>
    /// <param name="query">The payroll query</param>
    protected virtual void SetupDbQuery(SqlKata.Query dbQuery, Query query)
    {
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(IDbContext context, int parentId, int id)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        // parent check
        if (await GetParentIdAsync(context, id) != parentId)
        {
            return false;
        }

        // item base check
        return await ExistsAsync(context, id);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> QueryAsync(IDbContext context, int parentId, Query query = null)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, ParentFieldName, parentId, query);
        SetupDbQuery(dbQuery, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var items = (await QueryAsync<T>(context, compileQuery)).ToList();

        // notification
        await OnRetrieved(context, parentId, items);

        return items;
    }

    /// <inheritdoc />
    public virtual async Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, ParentFieldName, parentId, query, QueryMode.ItemCount);
        SetupDbQuery(dbQuery, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    /// <inheritdoc />
    public virtual async Task<int?> GetParentIdAsync(IDbContext context, int itemId)
    {
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }

        var query = DbQueryFactory.NewQuery(TableName, itemId)
            .Select(ParentFieldName);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var result = await QueryAsync<int>(context, compileQuery);
        var parentId = result.FirstOrDefault();
        return parentId > 0 ? parentId : null;
    }

    /// <inheritdoc />
    public virtual async Task<T> GetAsync(IDbContext context, int parentId, int itemId)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }
        var item = (await SelectAsync<T>(context, TableName, new()
        {
            { ParentFieldName, parentId },
            { DbSchema.ObjectColumn.Id, itemId }
        })).FirstOrDefault();

        // notification
        if (item != null)
        {
            await OnRetrieved(context, parentId, item);
        }

        return item;
    }

    /// <summary>
    /// Item retrieved handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The retrieved item</param>
    protected virtual Task OnRetrieved(IDbContext context, int parentId, T item) =>
        Task.FromResult<object>(null);

    /// <summary>
    /// Items retrieved handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="items">The retrieved items</param>
    protected async Task OnRetrieved(IDbContext context, int parentId, IEnumerable<T> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                await OnRetrieved(context, parentId, item);
            }
        }
    }

    #endregion

    #region Create

    /// <inheritdoc />
    public virtual async Task<T> CreateAsync(IDbContext context, int parentId, T item)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        var inserted = await InsertObject(context, parentId, item);
        txGuard.Complete();
        return inserted ? item : default;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> CreateAsync(IDbContext context, int parentId, IEnumerable<T> items)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var createdObjects = new List<T>();
        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        foreach (var obj in items)
        {
            if (await InsertObject(context, parentId, obj))
            {
                createdObjects.Add(obj);
            }
        }
        txScope.Complete();

        return createdObjects;
    }

    /// <inheritdoc />
    public virtual async Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<T> items)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var count = 0;

        // bulk transaction across all chunks
        using var txScope = TransactionFactory.NewTransactionScope();

        // chunk process
        var chunkSize = Math.Min(1000, BulkChunkSize);
        foreach (var chunk in items.Chunk(chunkSize))
        {

            // collect data
            var now = Date.Now;
            var parameterSet = chunk.AsParallel()
                .Select(result =>
                {
                    // set created/updated dates (same as InsertObject)
                    result.InitCreatedDate(now);

                    var parameters = new DbParameterCollection();
                    GetCreateData(parentId, result, parameters);
                    return parameters;
                })
                .ToList();

            // convert to data table per chunk
            var dataTable = parameterSet.ToDataTable(TableName);

            // insert data table
            try
            {
                await context.BulkInsertAsync(dataTable);
            }
            catch (Exception exception)
            {
                var transformed = context.TransformException(exception);
                if (transformed != exception)
                {
                    throw transformed;
                }
                throw;
            }

            count += chunk.Length;
        }

        txScope.Complete();
        stopwatch.Stop();
        Log.Trace($"Bulk import {count} items in {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Insert new item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    /// <returns>True for inserted item</returns>
    private async Task<bool> InsertObject(IDbContext context, int parentId, T item)
    {
        // create and update date
        item.InitCreatedDate(Date.Now);

        var parameters = new DbParameterCollection();
        GetCreateData(parentId, item, parameters);

        // build query statement
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbInsert(TableName, parameters.GetNames());
        queryBuilder.AppendIdentitySelect();
        var query = queryBuilder.ToString();

        // db insert
        try
        {
            item.Id = (int)await ExecuteScalarAsync(context, query, parameters);
        }
        catch (Exception exception)
        {
            var transformException = context.TransformException(exception);
            if (transformException != null)
            {
                throw transformException;
            }
            throw;
        }

        await OnCreatedAsync(context, parentId, item);
        return true;
    }

    /// <summary>
    /// Get item create data
    /// </summary>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    /// <param name="parameters">The database parameters</param>
    protected virtual void GetCreateData(int parentId, T item, DbParameterCollection parameters)
    {
        GetObjectData(item, parameters);
        GetObjectCreateData(item, parameters);
        if (!parameters.HasAny)
        {
            throw new PayrollException($"Missing object data for object {item}.");
        }

        parameters.AddStatus(item.Status);
        parameters.AddCreated(item.Created);
        parameters.AddUpdated(GetValidUpdatedDate(item));
        // parent
        parameters.Add(ParentFieldName, parentId, DbType.Int32);
    }

    /// <summary>
    /// Item created handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    protected virtual Task OnCreatedAsync(IDbContext context, int parentId, T item) => Task.FromResult(0);

    #endregion

    #region Update

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(IDbContext context, int parentId, T item)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // update date
        item.Updated = Date.Now;

        var parameters = new DbParameterCollection();
        GetUpdateData(item, parameters);

        // build update statement
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), item.Id);
        var query = queryBuilder.ToString();

        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        // db update
        await ExecuteAsync(context, query, parameters);
        // children
        await OnUpdatedAsync(context, parentId, item);
        txGuard.Complete();

        return item;
    }

    /// <summary>
    /// Item updated handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to update</param>
    protected virtual Task OnUpdatedAsync(IDbContext context, int parentId, T item) =>
        Task.FromResult<object>(null);

    /// <summary>
    /// Get item updated data
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <param name="parameters">The database parameters</param>
    private void GetUpdateData(T item, DbParameterCollection parameters)
    {
        GetObjectData(item, parameters);
        GetObjectUpdateData();
        parameters.AddStatus(item.Status);
        parameters.AddUpdated(GetValidUpdatedDate(item));
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }

        var deleted = false;
        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        if (await OnDeletingAsync(context, itemId))
        {
            // item
            var query = DbQueryFactory.NewDeleteQuery(TableName, itemId);
            var compileQuery = CompileQuery(query);

            // DELETE execution
            deleted = (await ExecuteAsync(context, compileQuery)) > 0;
        }
        txGuard.Complete();
        return deleted;
    }

    /// <summary>
    /// Item deleting request
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="itemId">The item id</param>
    /// <returns>True for a valid item</returns>
    protected virtual Task<bool> OnDeletingAsync(IDbContext context, int itemId) =>
        Task.FromResult(true);

    #endregion

}
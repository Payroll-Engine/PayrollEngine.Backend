﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class ChildDomainRepository<T> : DomainRepository<T>, IChildDomainRepository<T>
    where T : IDomainObject
{
    /// <summary>
    /// The parent field name
    /// </summary>
    public string ParentFieldName { get; }

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
    /// Setup the database query
    /// </summary>
    /// <param name="dbQuery">The database query</param>
    /// <param name="query">The payroll query</param>
    protected virtual void SetupDbQuery(SqlKata.Query dbQuery, Query query)
    {
    }

    /// <summary>
    /// Query items
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="query">The payroll query</param>
    /// <returns>The items matching the query criteria</returns>
    public virtual async Task<IEnumerable<T>> QueryAsync(IDbContext context, int parentId, Query query = null)
    {
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

    /// <summary>
    /// Query items count
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="query">The payroll query</param>
    /// <returns>The count of items, matching the query criteria</returns>
    public virtual async Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, ParentFieldName, parentId, query, QueryMode.ItemCount);
        SetupDbQuery(dbQuery, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    /// <summary>
    /// Get the parent record id
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="itemId">The item id</param>
    /// <returns>The parent item id</returns>
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
        return result.FirstOrDefault();
    }

    /// <summary>
    /// Get item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="itemId">The item id</param>
    /// <returns>The item</returns>
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
    protected virtual async Task OnRetrieved(IDbContext context, int parentId, IEnumerable<T> items)
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

    /// <summary>
    /// Create item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    /// <returns>Created item</returns>
    public virtual async Task<T> CreateAsync(IDbContext context, int parentId, T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        var inserted = await InsertObject(context, parentId, item);
        txScope.Complete();
        return inserted ? item : default;
    }

    /// <summary>
    /// Create items
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="items">The items to create</param>
    /// <returns>Created items</returns>
    public virtual async Task<IEnumerable<T>> CreateAsync(IDbContext context, int parentId, IEnumerable<T> items)
    {
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

    /// <summary>
    /// Create items within a bulk operation
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="items">The items to create</param>
    public virtual async Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<T> items)
    {
        var objectList = items.ToList();
        if (!objectList.Any())
        {
            return;
        }

        // bulk insert
        string query = null;
        var dataObjects = new List<DbParameterCollection>();
        foreach (var result in objectList)
        {
            var parameters = new DbParameterCollection();
            GetCreateData(parentId, result, parameters);
            dataObjects.Add(parameters);
            query ??= DbQueryFactory.NewBulkInsertQuery(TableName, parameters.GetNames());
        }
        await ExecuteAsync(context, query, dataObjects);
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

        if (await OnCreatingAsync(context, parentId, item))
        {
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

        return false;
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
            throw new PayrollException($"Missing object data for object {item}");
        }

        parameters.Add(DbSchema.ObjectColumn.Status, item.Status);
        parameters.Add(DbSchema.ObjectColumn.Created, item.Created);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(item));
        // parent
        parameters.Add(ParentFieldName, parentId);
    }

    /// <summary>
    /// Item creating request
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    /// <returns>True for a valid item</returns>
    protected virtual Task<bool> OnCreatingAsync(IDbContext context, int parentId, T item) => 
        Task.FromResult(true);

    /// <summary>
    /// Item created handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to create</param>
    protected virtual Task OnCreatedAsync(IDbContext context, int parentId, T item) => Task.FromResult(0);

    #endregion

    #region Update

    /// <summary>
    /// Update existing item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to update</param>
    /// <returns>The updated item</returns>
    public virtual async Task<T> UpdateAsync(IDbContext context, int parentId, T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // update date
        item.Updated = Date.Now;
        if (await OnUpdatingAsync(context, parentId, item))
        {
            var parameters = new DbParameterCollection();
            GetUpdateData(item, parameters);

            // build update statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), item.Id);
            var query = queryBuilder.ToString();

            // transaction
            using var txScope = TransactionFactory.NewTransactionScope();
            // db update
            await ExecuteAsync(context, query, parameters);
            // children
            await OnUpdatedAsync(context, parentId, item);
            txScope.Complete();
        }
        return item;
    }

    /// <summary>
    /// Item updating request
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="item">The item to update</param>
    /// <returns>True for a valid item</returns>
    protected virtual Task<bool> OnUpdatingAsync(IDbContext context, int parentId, T item) =>
        Task.FromResult(true);

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
    protected virtual void GetUpdateData(T item, DbParameterCollection parameters)
    {
        GetObjectData(item, parameters);
        GetObjectUpdateData(item, parameters);
        parameters.Add(DbSchema.ObjectColumn.Status, item.Status);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(item));
    }

    #endregion

    #region Delete

    /// <summary>
    /// Delete existing item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="itemId">The item id</param>
    /// <returns>The updated item</returns>
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
        using var txScope = TransactionFactory.NewTransactionScope();
        if (await OnDeletingAsync(context, parentId, itemId))
        {
            // item
            var query = DbQueryFactory.NewDeleteQuery(TableName, itemId);
            var compileQuery = CompileQuery(query);

            // DELETE execution
            deleted = (await ExecuteAsync(context, compileQuery)) > 0;
            await OnDeletedAsync(context, parentId, itemId);
        }
        txScope.Complete();
        return deleted;
    }

    /// <summary>
    /// Item deleting request
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="itemId">The item id</param>
    /// <returns>True for a valid item</returns>
    protected virtual Task<bool> OnDeletingAsync(IDbContext context, int parentId, int itemId) =>
        Task.FromResult(true);

    /// <summary>
    /// Item deleted handler
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent id</param>
    /// <param name="itemId">The item id</param>
    protected virtual Task OnDeletedAsync(IDbContext context, int parentId, int itemId) =>
        Task.FromResult<object>(null);

    #endregion

}
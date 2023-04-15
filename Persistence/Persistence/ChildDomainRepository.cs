using System;
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
    public string ParentFieldName { get; }

    protected ChildDomainRepository(string tableName, string parentFieldName, IDbContext context) :
        base(tableName, context)
    {
        if (string.IsNullOrWhiteSpace(parentFieldName))
        {
            throw new ArgumentException(nameof(parentFieldName));
        }

        ParentFieldName = parentFieldName;
    }

    #region Query/Get

    protected virtual void SetupDbQuery(SqlKata.Query dbQuery, Query query)
    {
    }

    public virtual async Task<IEnumerable<T>> QueryAsync(int parentId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(Context, TableName, ParentFieldName, parentId, query);
        SetupDbQuery(dbQuery, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var items = (await QueryAsync<T>(compileQuery)).ToList();

        // notification
        await OnRetrieved(parentId, items);

        return items;
    }

    public virtual async Task<long> QueryCountAsync(int parentId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(Context, TableName, ParentFieldName, parentId, query, QueryMode.ItemCount);
        SetupDbQuery(dbQuery, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(compileQuery);
        return count;
    }

    public virtual async Task<int?> GetParentIdAsync(int itemId)
    {
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }

        var query = DbQueryFactory.NewQuery(TableName, itemId)
            .Select(ParentFieldName);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var result = await QueryAsync<int>(compileQuery);
        return result.FirstOrDefault();
    }

    public virtual async Task<T> GetAsync(int parentId, int itemId)
    {
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }
        if (itemId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemId));
        }
        var item = (await SelectAsync<T>(TableName, new()
        {
            { ParentFieldName, parentId },
            { DbSchema.ObjectColumn.Id, itemId }
        })).FirstOrDefault();

        // notification
        if (item != null)
        {
            await OnRetrieved(parentId, item);
        }

        return item;
    }

    protected virtual Task OnRetrieved(int parentId, T item) =>
        Task.FromResult<object>(null);

    protected virtual async Task OnRetrieved(int parentId, IEnumerable<T> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                await OnRetrieved(parentId, item);
            }
        }
    }

    #endregion

    #region Create

    public virtual async Task<T> CreateAsync(int parentId, T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        var inserted = await InsertObject(parentId, obj);
        txScope.Complete();
        return inserted ? obj : default;
    }

    public virtual async Task<IEnumerable<T>> CreateAsync(int parentId, IEnumerable<T> items)
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
            if (await InsertObject(parentId, obj))
            {
                createdObjects.Add(obj);
            }
        }
        txScope.Complete();

        return createdObjects;
    }

    public virtual async Task CreateBulkAsync(int parentId, IEnumerable<T> items)
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
            query ??= DbQueryFactory.NewBulkInsertQuery(TableName, parameters.ParameterNames);
        }
        await ExecuteAsync(query, dataObjects);
    }

    private async Task<bool> InsertObject(int parentId, T item)
    {
        // create and update date
        item.InitCreatedDate(Date.Now);

        if (await OnCreatingAsync(parentId, item))
        {
            var parameters = new DbParameterCollection();
            GetCreateData(parentId, item, parameters);

            // build query statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbInsert(TableName, parameters.ParameterNames.ToList());
            queryBuilder.AppendIdentitySelect();
            var query = queryBuilder.ToString();

            // db insert
            try
            {
                item.Id = (int)await ExecuteScalarAsync(query, parameters);
            }
            catch (Exception exception)
            {
                var transformException = Context.TransformException(exception);
                if (transformException != null)
                {
                    throw transformException;
                }
                throw;
            }

            await OnCreatedAsync(parentId, item);
            return true;
        }

        return false;
    }

    protected virtual void GetCreateData(int parentId, T item, DbParameterCollection parameters)
    {
        GetObjectData(item, parameters);
        GetObjectCreateData(item, parameters);
        if (!parameters.ParameterNames.Any())
        {
            throw new PayrollException($"Missing object data for object {item}");
        }

        parameters.Add(DbSchema.ObjectColumn.Status, item.Status);
        parameters.Add(DbSchema.ObjectColumn.Created, item.Created);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(item));
        // parent
        parameters.Add(ParentFieldName, parentId);
    }

    protected virtual Task<bool> OnCreatingAsync(int parentId, T item) => Task.FromResult(true);
    protected virtual Task OnCreatedAsync(int parentId, T item) => Task.FromResult(0);

    #endregion

    #region Update

    public virtual async Task<T> UpdateAsync(int parentId, T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // update date
        item.Updated = Date.Now;
        if (await OnUpdatingAsync(parentId, item))
        {
            var parameters = new DbParameterCollection();
            GetUpdateData(item, parameters);

            // build update statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbUpdate(TableName, parameters.ParameterNames.ToList(), item.Id);
            var query = queryBuilder.ToString();

            // transaction
            using var txScope = TransactionFactory.NewTransactionScope();
            // db update
            await ExecuteAsync(query, parameters);
            // children
            await OnUpdatedAsync(parentId, item);
            txScope.Complete();
        }
        return item;
    }

    protected virtual Task<bool> OnUpdatingAsync(int parentId, T item) =>
        Task.FromResult(true);

    protected virtual Task OnUpdatedAsync(int parentId, T item) =>
        Task.FromResult<object>(null);

    protected virtual void GetUpdateData(T obj, DbParameterCollection parameters)
    {
        GetObjectData(obj, parameters);
        GetObjectUpdateData(obj, parameters);
        if (!parameters.ParameterNames.Any())
        {
            //   throw new PayrollException($"Missing object data for object {obj}");
        }

        parameters.Add(DbSchema.ObjectColumn.Status, obj.Status);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(obj));
    }

    #endregion

    #region Delete

    public virtual async Task<bool> DeleteAsync(int parentId, int itemId)
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
        if (await OnDeletingAsync(parentId, itemId))
        {
            // item
            var query = DbQueryFactory.NewDeleteQuery(TableName, itemId);
            var compileQuery = CompileQuery(query);

            // DELETE execution
            deleted = (await ExecuteAsync(compileQuery)) > 0;
            await OnDeletedAsync(parentId, itemId);
        }
        txScope.Complete();
        return deleted;
    }

    protected virtual Task<bool> OnDeletingAsync(int parentId, int itemId) =>
        Task.FromResult(true);

    protected virtual Task OnDeletedAsync(int parentId, int id) =>
        Task.FromResult<object>(null);

    #endregion

}
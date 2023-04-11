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

public abstract class RootDomainRepository<T> : DomainRepository<T>, IRootDomainRepository<T>
    where T : IDomainObject
{
    protected RootDomainRepository(string tableName, IDbContext context) :
        base(tableName, context)
    {
    }

    #region Query/Get

    public virtual async Task<IEnumerable<T>> QueryAsync(Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewQuery<T>(Context, TableName, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var items = (await QueryAsync<T>(compileQuery)).ToList();

        // notification
        await OnRetrieved(items);

        return items;
    }

    public virtual async Task<long> QueryCountAsync(Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(Context, TableName, query, DbQueryMode.ItemCount);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(compileQuery);
        return count;
    }

    public virtual async Task<T> GetAsync(int id)
    {
        var item = await SelectSingleByIdAsync<T>(TableName, id);
        // notification
        if (item != null)
        {
            await OnRetrieved(item);
        }
        return item;
    }

    protected virtual Task OnRetrieved(T item) =>
        Task.FromResult<object>(null);

    protected virtual async Task OnRetrieved(IEnumerable<T> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                await OnRetrieved(item);
            }
        }
    }
    #endregion

    #region Create

    public virtual async Task<T> CreateAsync(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // create and update date
        item.InitCreatedDate(Date.Now);

        // collect data
        var parameters = new DbParameterCollection();
        GetCreateData(item, parameters);

        // build db statement
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

        return item;
    }

    public virtual async Task<IEnumerable<T>> CreateAsync(IEnumerable<T> items)
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
            if (await InsertObject(obj))
            {
                createdObjects.Add(obj);
            }
        }
        txScope.Complete();

        return createdObjects;
    }

    protected virtual void GetCreateData(T obj, DbParameterCollection data)
    {
        GetObjectData(obj, data);
        GetObjectCreateData(obj, data);
        if (!data.ParameterNames.Any())
        {
            throw new PayrollException($"Missing object data for object {obj}");
        }

        data.Add(DbSchema.ObjectColumn.Status, obj.Status);
        data.Add(DbSchema.ObjectColumn.Created, obj.Created);
        data.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(obj));
    }

    private async Task<bool> InsertObject(T item)
    {
        // create and update date
        item.InitCreatedDate(Date.Now);

        if (await OnCreatingAsync(item))
        {
            var parameters = new DbParameterCollection();
            GetCreateData(item, parameters);

            // build db statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbInsert(TableName, parameters.ParameterNames.ToList());
            queryBuilder.AppendIdentitySelect();
            var query = queryBuilder.ToString();

            // insert
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

            await OnCreatedAsync(item);
            return true;
        }

        return false;
    }

    protected virtual Task<bool> OnCreatingAsync(T item) => Task.FromResult(true);
    protected virtual Task OnCreatedAsync(T item) => Task.FromResult(0);

    #endregion

    #region Update

    public virtual async Task<T> UpdateAsync(T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        // update date
        obj.Updated = Date.Now;
        var parameters = new DbParameterCollection();
        GetUpdateData(obj, parameters);

        // build db statement
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.ParameterNames.ToList(), obj.Id);
        var query = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        await ExecuteAsync(query, parameters);
        txScope.Complete();
        return obj;
    }

    protected virtual void GetUpdateData(T obj, DbParameterCollection parameters)
    {
        GetObjectData(obj, parameters);
        GetObjectUpdateData(obj, parameters);
        if (!parameters.ParameterNames.Any())
        {
            throw new PayrollException($"Missing object data for object {obj}");
        }

        parameters.Add(DbSchema.ObjectColumn.Status, obj.Status);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(obj));
    }

    #endregion

    #region Delete

    public virtual async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var query = DbQueryFactory.NewDeleteQuery(TableName, id);
        var compileQuery = CompileQuery(query);

        // DELETE execution
        return (await ExecuteAsync(compileQuery)) > 0;
    }

    #endregion

}
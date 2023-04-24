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
    protected RootDomainRepository(string tableName) :
        base(tableName)
    {
    }

    #region Query/Get

    public virtual async Task<IEnumerable<T>> QueryAsync(IDbContext context, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var items = (await QueryAsync<T>(context, compileQuery)).ToList();

        // notification
        await OnRetrieved(context, items);

        return items;
    }

    public virtual async Task<long> QueryCountAsync(IDbContext context, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, query, QueryMode.ItemCount);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    public virtual async Task<T> GetAsync(IDbContext context, int id)
    {
        var item = await SelectSingleByIdAsync<T>(context, TableName, id);
        // notification
        if (item != null)
        {
            await OnRetrieved(context, item);
        }
        return item;
    }

    protected virtual Task OnRetrieved(IDbContext context, T item) =>
        Task.FromResult<object>(null);

    protected virtual async Task OnRetrieved(IDbContext context, IEnumerable<T> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                await OnRetrieved(context, item);
            }
        }
    }
    #endregion

    #region Create

    public virtual async Task<T> CreateAsync(IDbContext context, T item)
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
        queryBuilder.AppendDbInsert(TableName, parameters.GetNames());
        queryBuilder.AppendIdentitySelect();
        var query = queryBuilder.ToString();

        // db insert
        try
        {
            //using var connection = context.NewConnection();
            //connection.Open();
            item.Id = (int)await ExecuteScalarAsync(context, query, parameters);
            //item.Id = (int)await ExecuteScalarAsync(connection, query, parameters);
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

        return item;
    }

    public virtual async Task<IEnumerable<T>> CreateAsync(IDbContext context, IEnumerable<T> items)
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
            if (await InsertObject(context, obj))
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
        if (!data.HasAny)
        {
            throw new PayrollException($"Missing object data for object {obj}");
        }

        data.Add(DbSchema.ObjectColumn.Status, obj.Status);
        data.Add(DbSchema.ObjectColumn.Created, obj.Created);
        data.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(obj));
    }

    private async Task<bool> InsertObject(IDbContext context, T item)
    {
        // create and update date
        item.InitCreatedDate(Date.Now);

        if (await OnCreatingAsync(context, item))
        {
            var parameters = new DbParameterCollection();
            GetCreateData(item, parameters);

            // build db statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbInsert(TableName, parameters.GetNames());
            queryBuilder.AppendIdentitySelect();
            var query = queryBuilder.ToString();

            // insert
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

            await OnCreatedAsync(context, item);
            return true;
        }

        return false;
    }

    protected virtual Task<bool> OnCreatingAsync(IDbContext context, T item) => Task.FromResult(true);
    protected virtual Task OnCreatedAsync(IDbContext context, T item) => Task.FromResult(0);

    #endregion

    #region Update

    public virtual async Task<T> UpdateAsync(IDbContext context, T obj)
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
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), obj.Id);
        var query = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        await ExecuteAsync(context, query, parameters);
        txScope.Complete();
        return obj;
    }

    protected virtual void GetUpdateData(T obj, DbParameterCollection parameters)
    {
        GetObjectData(obj, parameters);
        GetObjectUpdateData(obj, parameters);
        if (!parameters.HasAny)
        {
            throw new PayrollException($"Missing object data for object {obj}");
        }

        parameters.Add(DbSchema.ObjectColumn.Status, obj.Status);
        parameters.Add(DbSchema.ObjectColumn.Updated, GetValidUpdatedDate(obj));
    }

    #endregion

    #region Delete

    public virtual async Task<bool> DeleteAsync(IDbContext context, int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var query = DbQueryFactory.NewDeleteQuery(TableName, id);
        var compileQuery = CompileQuery(query);

        // DELETE execution
        return (await ExecuteAsync(context, compileQuery)) > 0;
    }

    #endregion

}
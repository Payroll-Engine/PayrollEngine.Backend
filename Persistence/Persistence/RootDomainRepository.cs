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

/// <summary>
/// Abstract base repository for top-level (root) domain objects that belong directly to a tenant.
/// Provides query, get, create, update, and delete operations without a parent entity context.
/// </summary>
/// <typeparam name="T">Root domain object type (e.g. Tenant, Calendar, User)</typeparam>
public abstract class RootDomainRepository<T>(string tableName) : DomainRepository<T>(tableName),
    IRootDomainRepository<T>
    where T : IDomainObject
{
    #region Query/Get

    public virtual async Task<IEnumerable<T>> QueryAsync(IDbContext context, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery, context);

        // SELECT execution
        var items = (await QueryAsync<T>(context, compileQuery)).ToList();

        return items;
    }

    public virtual async Task<long> QueryCountAsync(IDbContext context, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, query, QueryMode.ItemCount);

        // query compilation
        var compileQuery = CompileQuery(dbQuery, context);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    public virtual async Task<T> GetAsync(IDbContext context, int id)
    {
        var item = await SelectSingleByIdAsync<T>(context, TableName, id);
        return item;
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
        queryBuilder.AppendDbInsert(TableName, parameters.GetNames(), context);
        queryBuilder.AppendIdentitySelect(context);
        var query = queryBuilder.ToString();

        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        // db insert
        try
        {
            item.Id = Convert.ToInt32(await ExecuteScalarAsync(context, query, parameters));
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
        txGuard.Complete();

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

    private void GetCreateData(T obj, DbParameterCollection data)
    {
        GetObjectData(obj, data);
        GetObjectCreateData(obj, data);
        if (!data.HasAny)
        {
            throw new PayrollException($"Missing object data for object {obj}.");
        }

        data.AddStatus(obj.Status);
        data.AddCreated(obj.Created);
        data.AddUpdated(GetValidUpdatedDate(obj));
    }

    private async Task<bool> InsertObject(IDbContext context, T item)
    {
        // create and update date
        item.InitCreatedDate(Date.Now);

        if (await OnCreatingAsync())
        {
            var parameters = new DbParameterCollection();
            GetCreateData(item, parameters);

            // build db statement
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendDbInsert(TableName, parameters.GetNames(), context);
            queryBuilder.AppendIdentitySelect(context);
            var query = queryBuilder.ToString();

            // insert
            try
            {
                item.Id = Convert.ToInt32(await ExecuteScalarAsync(context, query, parameters));
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

            await OnCreatedAsync();
            return true;
        }

        return false;
    }

    private static Task<bool> OnCreatingAsync() => Task.FromResult(true);
    private static Task OnCreatedAsync() => Task.FromResult(0);

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
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), obj.Id, context);
        var query = queryBuilder.ToString();

        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        await ExecuteAsync(context, query, parameters);
        txGuard.Complete();
        return obj;
    }

    private void GetUpdateData(T obj, DbParameterCollection parameters)
    {
        GetObjectData(obj, parameters);
        GetObjectUpdateData();
        if (!parameters.HasAny)
        {
            throw new PayrollException($"Missing object data for object {obj}.");
        }

        parameters.AddStatus(obj.Status);
        parameters.AddUpdated(GetValidUpdatedDate(obj));
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
        var compileQuery = CompileQuery(query, context);

        // transaction guard: no-op if already inside an ambient scope
        using var txGuard = TransactionFactory.NewTransactionGuard();
        // DELETE execution
        var deleted = (await ExecuteAsync(context, compileQuery)) > 0;
        txGuard.Complete();
        return deleted;
    }

    #endregion

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Core;

public abstract class RepositoryRootObjectController<TService, TRepo, TDomain, TApi> : RepositoryObjectController<TService, TRepo, TDomain, TApi>
    where TService : class, IRootApplicationService<TRepo, TDomain>
    where TRepo : class, IRootDomainRepository<TDomain>
    where TDomain : class, IDomainObject, new()
    where TApi : ApiObjectBase, new()
{
    protected RepositoryRootObjectController(TService service, IControllerRuntime runtime,
        IApiMap<TDomain, TApi> map) :
        base(service, runtime, map)
    {
    }

    /// <summary>
    /// Query items
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>Items, count or both</returns>
    protected virtual async Task<ActionResult> QueryItemsAsync(Query query = null)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;

        try
        {
            switch (query.Result)
            {
                case QueryResultType.Items:
                    var items = await QueryAsync(query);
                    return items.IsValidResult() ? Ok(items.Value) : items.Result;
                case QueryResultType.Count:
                    var count = await QueryCountAsync(query);
                    return count.IsValidResult() ? Ok(count.Value) : count.Result;
                case QueryResultType.ItemsWithCount:
                    items = await QueryAsync(query);
                    count = await QueryCountAsync(query);
                    return items.IsValidResult() && count.IsValidResult() ?
                        Ok(new QueryResult<TApi>(items.Value, count.Value)) : items.Result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Query resources of <typeparamref name="TApi"/>
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>List of requested Api objects</returns>
    protected virtual async Task<ActionResult<TApi[]>> QueryAsync(Query query = null)
    {
        var apiObjects = new List<TApi>();
        var items = (await Service.QueryAsync(Runtime.DbContext, query)).ToList();
        foreach (var item in items)
        {
            apiObjects.Add(MapDomainToApi(item));
        }
        return apiObjects.ToArray();
    }

    /// <summary>
    /// Count count of resources query
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>Count of requested Api objects</returns>
    protected virtual async Task<ActionResult<long>> QueryCountAsync(Query query = null)
    {
        return await Service.QueryCountAsync(Runtime.DbContext, query);
    }

    protected virtual async Task<ActionResult<TApi>> GetAsync(int id)
    {
        try
        {
            // argument check
            if (id <= 0)
            {
                return UndefinedObjectIdRequest();
            }

            // get object
            var domainObject = await Service.GetAsync(Runtime.DbContext, id);
            if (domainObject == null)
            {
                return ObjectNotFoundRequest(id);
            }
            return MapDomainToApi(domainObject);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Create resource
    /// </summary>
    /// <param name="apiObject">The API object</param>
    /// <returns>New created object</returns>
    protected virtual async Task<ActionResult<TApi>> CreateAsync(TApi apiObject)
    {
        // argument check
        if (apiObject == null)
        {
            return UndefinedObjectRequest();
        }
        if (apiObject.Id > 0)
        {
            return CreateObjectWithIdRequest();
        }

        // map object
        TDomain domainObject;
        try
        {
            domainObject = MapApiToDomain(apiObject);
        }
        catch (PayrollMapException exception)
        {
            throw new QueryException(exception.GetBaseMessage(), exception);
        }

        // create object
        domainObject = await Service.CreateAsync(Runtime.DbContext, domainObject);
        if (domainObject.Id <= 0)
        {
            return CreateObjectFailedRequest();
        }

        // created resource
        return new CreatedObjectResult(Request.Path, MapDomainToApi(domainObject));
    }

    protected virtual async Task<ActionResult<TApi>> UpdateAsync(TApi apiObject)
    {
        try
        {
            // argument check
            if (apiObject == null)
            {
                return UndefinedObjectRequest();
            }
            if (apiObject.Id <= 0)
            {
                return UndefinedObjectIdRequest();
            }

            // check for existing object
            if (!await Service.ExistsAsync(Runtime.DbContext, apiObject.Id))
            {
                return ObjectNotFoundRequest(apiObject.Id);
            }

            // map object
            TDomain domainObject;
            try
            {
                domainObject = MapApiToDomain(apiObject);
            }
            catch (PayrollMapException exception)
            {
                var message = exception.GetBaseMessage();
                Log.Error(exception, message);
                return UnprocessableEntity(message);
            }

            // update object
            domainObject = await Service.UpdateAsync(Runtime.DbContext, domainObject);
            return MapDomainToApi(domainObject);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<IActionResult> DeleteAsync(int itemId)
    {
        try
        {
            // argument check
            if (itemId <= 0)
            {
                return UndefinedObjectIdRequest();
            }

            // check for existing object
            if (!await ExistsAsync(Runtime.DbContext, itemId))
            {
                return ObjectNotFoundRequest(itemId);
            }

            await Service.DeleteAsync(Runtime.DbContext, itemId);
            return Ok();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }
}
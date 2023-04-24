using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Core;

public abstract class RepositoryChildObjectController<TParentService, TService, TParentRepo, TRepo, TParent, TDomain, TApi> :
    RepositoryObjectController<TService, TRepo, TDomain, TApi>
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TService : class, IChildApplicationService<TRepo, TDomain>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IChildDomainRepository<TDomain>
    where TParent : class, IDomainObject, new()
    where TDomain : class, IDomainObject, new()
    where TApi : ApiObjectBase, new()
{
    protected TParentService ParentService { get; }
    protected string ParentObjectName => GetObjectName(typeof(TParent));
    protected TService ChildService => Service;

    protected RepositoryChildObjectController(TParentService parentService, TService service, IControllerRuntime runtime,
        IApiMap<TDomain, TApi> map) :
        base(service, runtime, map)
    {
        ParentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
    }

    protected virtual TDomain MapApiToDomain(int parentId, TApi apiObject) =>
        MapApiToDomain(apiObject);

    /// <summary>
    /// Query items
    /// </summary>
    /// <param name="parentId">The tenant id</param>
    /// <param name="query">The query</param>
    /// <returns>Items, count or both</returns>
    protected virtual async Task<ActionResult> QueryItemsAsync(int parentId, Query query = null)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;
        switch (query.Result)
        {
            case QueryResultType.Items:
                var items = await QueryAsync(parentId, query);
                return items.IsValidResult() ? Ok(items.Value) : items.Result;
            case QueryResultType.Count:
                var count = await QueryCountAsync(parentId, query);
                return count.IsValidResult() ? Ok(count.Value) : count.Result;
            case QueryResultType.ItemsWithCount:
                items = await QueryAsync(parentId, query);
                count = await QueryCountAsync(parentId, query);
                return items.IsValidResult() && count.IsValidResult() ?
                    Ok(new QueryResult<TApi>(items.Value, count.Value)) : items.Result;
            default:
                throw new ArgumentOutOfRangeException($"Unknown query result {query.Result}");
        }
    }

    /// <summary>
    /// Query resources of <typeparamref name="TApi"/>
    /// </summary>
    /// <param name="parentId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>List of requested Api objects</returns>
    protected virtual async Task<ActionResult<TApi[]>> QueryAsync(int parentId, Query query = null)
    {
        try
        {
            // parent check
            if (parentId <= 0)
            {
                return InvalidParentRequest(parentId);
            }
            // existing parent check
            if (!await ParentService.ExistsAsync(Runtime.DbContext, parentId))
            {
                return NotFound(typeof(TParent), parentId);
            }

            var apiObjects = new List<TApi>();
            var items = (await ChildService.QueryAsync(Runtime.DbContext, parentId, query)).ToList();
            foreach (var item in items)
            {
                apiObjects.Add(MapDomainToApi(item));
            }
            return apiObjects.ToArray();
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
    /// Count count of resources query
    /// </summary>
    /// <param name="parentId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>Count of requested Api objects</returns>
    protected virtual async Task<ActionResult<long>> QueryCountAsync(int parentId, Query query = null)
    {
        try
        {
            return await Service.QueryCountAsync(Runtime.DbContext, parentId, query);
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
    /// Get resource
    /// </summary>
    /// <param name="parentId">The parent id</param>
    /// <param name="id">The object id</param>
    /// <returns>The resource</returns>
    protected virtual async Task<ActionResult<TApi>> GetAsync(int parentId, int id)
    {
        try
        {
            // argument check
            if (parentId <= 0 || id <= 0)
            {
                return UndefinedObjectIdRequest();
            }

            // get object
            var domainObject = await Service.GetAsync(Runtime.DbContext, parentId, id);
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
    /// <param name="parentId">The parent id</param>
    /// <param name="apiObject">The API object</param>
    /// <returns>New created resource</returns>
    protected virtual async Task<ActionResult<TApi>> CreateAsync(int parentId, TApi apiObject)
    {
        try
        {
            // object
            if (apiObject == null)
            {
                return UndefinedObjectRequest();
            }
            // object id
            if (apiObject.Id > 0)
            {
                return CreateObjectWithIdRequest();
            }
            // parent
            if (parentId <= 0)
            {
                return InvalidParentRequest(parentId);
            }
            // existing parent
            if (!await ParentService.ExistsAsync(Runtime.DbContext, parentId))
            {
                return NotFound(typeof(TParent), parentId);
            }

            // map object
            TDomain domainObject;
            try
            {
                domainObject = MapApiToDomain(parentId, apiObject);
            }
            catch (PayrollMapException exception)
            {
                var message = exception.GetBaseMessage();
                Log.Error(exception, message);
                return UnprocessableEntity(message);
            }

            // create object
            try
            {
                domainObject = await ChildService.CreateAsync(Runtime.DbContext, parentId, domainObject);
                if (domainObject.Id <= 0)
                {
                    return CreateObjectFailedRequest();
                }
            }
            catch (PayrollScriptException scriptException)
            {
                var message = scriptException.GetBaseMessage();
                Log.Debug(scriptException, message);
                return UnprocessableEntity(message);
            }

            // created resource
            return new CreatedObjectResult(Request.Path, MapDomainToApi(domainObject));
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Create multiple resources
    /// </summary>
    /// <param name="parentId">The tenant id</param>
    /// <param name="apiObjects">The API objects</param>
    /// <returns>New created object</returns>
    protected virtual async Task<ActionResult<TApi[]>> CreateAsync(int parentId, IEnumerable<TApi> apiObjects)
    {
        try
        {
            // argument check
            if (apiObjects == null)
            {
                return UndefinedObjectRequest();
            }
            // parent
            if (parentId <= 0)
            {
                return InvalidParentRequest(parentId);
            }
            // existing parent
            if (!await ParentService.ExistsAsync(Runtime.DbContext, parentId))
            {
                return NotFound(typeof(TParent), parentId);
            }

            // map objects from api to domain
            var domainObjects = new List<TDomain>();
            foreach (var apiObject in apiObjects)
            {
                if (apiObject.Id > 0)
                {
                    return CreateObjectWithIdRequest();
                }
                try
                {
                    domainObjects.Add(MapApiToDomain(parentId, apiObject));
                }
                catch (PayrollMapException exception)
                {
                    var message = exception.GetBaseMessage();
                    Log.Error(exception, message);
                    return UnprocessableEntity(message);
                }
            }

            // create objects
            IEnumerable<TDomain> createdObjects;
            try
            {
                createdObjects = await ChildService.CreateAsync(Runtime.DbContext, parentId, domainObjects);
            }
            catch (PayrollScriptException scriptException)
            {
                var message = scriptException.GetBaseMessage();
                Log.Debug(scriptException, message);
                return UnprocessableEntity(message);
            }

            return MapDomainToApi(createdObjects);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Update resource
    /// </summary>
    /// <param name="parentId">The parent id</param>
    /// <param name="apiObject">The API object</param>
    /// <returns>The updated resource</returns>
    protected virtual async Task<ActionResult<TApi>> UpdateAsync(int parentId, TApi apiObject)
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
            // existing parent
            if (!await ParentService.ExistsAsync(Runtime.DbContext, parentId))
            {
                return NotFound(typeof(TParent), parentId);
            }
            // existing object
            if (!await Service.ExistsAsync(Runtime.DbContext, apiObject.Id))
            {
                return ObjectNotFoundRequest(apiObject.Id);
            }

            // map object
            TDomain domainObject;
            try
            {
                domainObject = MapApiToDomain(parentId, apiObject);
            }
            catch (PayrollMapException exception)
            {
                var message = exception.GetBaseMessage();
                Log.Error(exception, message);
                return UnprocessableEntity(message);
            }

            // update object
            try
            {
                domainObject = await Service.UpdateAsync(Runtime.DbContext, parentId, domainObject);
            }
            catch (PayrollScriptException scriptException)
            {
                var message = scriptException.GetBaseMessage();
                Log.Debug(scriptException, message);
                return UnprocessableEntity(message);
            }

            return MapDomainToApi(domainObject);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<IActionResult> DeleteAsync(int parentId, int itemId)
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

            await Service.DeleteAsync(Runtime.DbContext, parentId, itemId);
            return Ok();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Invalid parent request
    /// </summary>
    /// <param name="parentId">The parent id</param>
    /// <returns>Http bad request</returns>
    protected BadRequestObjectResult InvalidParentRequest(int parentId) =>
        BadRequest($"Invalid {ParentObjectName} id {parentId}");

    /// <summary>
    /// Parent not available request
    /// </summary>
    /// <param name="parentId">The parent id</param>
    /// <returns>Http bad request</returns>
    protected BadRequestObjectResult ParentNotFoundRequest(int parentId) =>
        BadRequest($"Not found {ParentObjectName} with id {parentId}");
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the case value
/// </summary>
public abstract class CaseChangeController<TParentService, TParentRepo, TRepo, TParent, TDomain, TApi> :
    RepositoryChildObjectController<TParentService, ICaseChangeService<TRepo, TDomain>, TParentRepo, TRepo, TParent, TDomain, TApi>
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IChildDomainRepository<TDomain>
    where TParent : class, IDomainObject, new()
    where TDomain : CaseChange, new()
    where TApi : ApiObject.CaseChange, new()
{
    protected CaseChangeController(TParentService parentService, ICaseChangeService<TRepo, TDomain> caseChangeService,
        IControllerRuntime runtime) :
        base(parentService, caseChangeService, runtime, new CaseChangeMap<TDomain, TApi>())
    {
    }

    /// <summary>
    /// Query case change values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">The query</param>
    /// <returns>Items, count or both</returns>
    protected async Task<ActionResult> QueryAsync(int tenantId, int parentId, Query query = null)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;
        switch (query.Result)
        {
            case QueryResultType.Items:
                var items = await QueryChangesAsync(tenantId, parentId, query);
                return items.IsValidResult() ? Ok(items.Value) : items.Result;
            case QueryResultType.Count:
                var count = await QueryChangesCountAsync(tenantId, parentId, query);
                return count.IsValidResult() ? Ok(count.Value) : count.Result;
            case QueryResultType.ItemsWithCount:
                items = await QueryChangesAsync(tenantId, parentId, query);
                count = await QueryChangesCountAsync(tenantId, parentId, query);
                return items.IsValidResult() && count.IsValidResult() ?
                    Ok(new QueryResult<ApiObject.CaseChange>(items.Value, count.Value)) : items.Result;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Query resources of <typeparamref name="TApi"/>
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>List of requested Api objects</returns>
    private async Task<ActionResult<ApiObject.CaseChange[]>> QueryChangesAsync(int tenantId, int parentId, Query query = null)
    {
        try
        {
            // authorization
            var authResult = await AuthorizeAsync(tenantId);
            if (authResult != null)
            {
                return authResult;
            }
            // parent check
            if (tenantId <= 0)
            {
                return InvalidParentRequest(tenantId);
            }

            var apiObjects = new List<ApiObject.CaseChange>();
            var items = (await Service.QueryAsync(Runtime.DbContext, tenantId, parentId, query)).ToList();
            foreach (var item in items)
            {
                apiObjects.Add(Map.ToApi(item));
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
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>Count of requested Api objects</returns>
    private async Task<ActionResult<long>> QueryChangesCountAsync(int tenantId,
        int parentId, Query query = null)
    {
        try
        {
            // authorization
            var authResult = await AuthorizeAsync(tenantId);
            if (authResult != null)
            {
                return authResult;
            }
            // parent check
            if (tenantId <= 0)
            {
                return InvalidParentRequest(tenantId);
            }

            return await Service.QueryValuesCountAsync(Runtime.DbContext, tenantId, parentId, query);
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
    /// Query case change values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">The query</param>
    /// <returns>Items, count or both</returns>
    protected async Task<ActionResult> QueryValuesAsync(int tenantId, int parentId, Query query = null)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;
        switch (query.Result)
        {
            case QueryResultType.Items:
                var items = await QueryChangesValuesAsync(tenantId, parentId, query);
                return items.IsValidResult() ? Ok(items.Value) : items.Result;
            case QueryResultType.Count:
                var count = await QueryChangesValuesCountAsync(tenantId, parentId, query);
                return count.IsValidResult() ? Ok(count.Value) : count.Result;
            case QueryResultType.ItemsWithCount:
                items = await QueryChangesValuesAsync(tenantId, parentId, query);
                count = await QueryChangesValuesCountAsync(tenantId, parentId, query);
                return items.IsValidResult() && count.IsValidResult() ?
                    Ok(new QueryResult<ApiObject.CaseChangeCaseValue>(items.Value, count.Value)) : items.Result;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Query resources of <typeparamref name="TApi"/>
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>List of requested Api objects</returns>
    private async Task<ActionResult<ApiObject.CaseChangeCaseValue[]>> QueryChangesValuesAsync(int tenantId, int parentId, Query query = null)
    {
        try
        {
            // authorization
            var authResult = await AuthorizeAsync(tenantId);
            if (authResult != null)
            {
                return authResult;
            }
            // parent check
            if (tenantId <= 0)
            {
                return InvalidParentRequest(tenantId);
            }

            var apiObjects = new List<ApiObject.CaseChangeCaseValue>();
            var items = (await Service.QueryValuesAsync(Runtime.DbContext, tenantId, parentId, query)).ToList();
            var map = new CaseChangeCaseValueMap();
            foreach (var item in items)
            {
                apiObjects.Add(map.ToApi(item));
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
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>Count of requested Api objects</returns>
    private async Task<ActionResult<long>> QueryChangesValuesCountAsync(int tenantId, int parentId, Query query = null)
    {
        try
        {
            // authorization
            var authResult = await AuthorizeAsync(tenantId);
            if (authResult != null)
            {
                return authResult;
            }
            // parent check
            if (tenantId <= 0)
            {
                return InvalidParentRequest(tenantId);
            }

            return await Service.QueryValuesCountAsync(Runtime.DbContext, tenantId, parentId, query);
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
    /// Get case change including case values
    /// </summary>
    /// <param name="parentId">The case change parent id</param>
    /// <param name="caseChangeId">The case change id</param>
    /// <remarks>Do not call the base class method</remarks>
    /// <returns>A case change</returns>
    protected override async Task<ActionResult<TApi>> GetAsync(int parentId, int caseChangeId) =>
        Map.ToApi(await Service.Repository.GetAsync(Runtime.DbContext, parentId, caseChangeId));
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation lookups
/// </summary>
[ApiControllerName("Lookups")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Lookup)]
public abstract class LookupController : RepositoryChildObjectController<IRegulationService, ILookupService,
    IRegulationRepository, ILookupRepository,
    DomainObject.Regulation, DomainObject.Lookup, ApiObject.Lookup>
{
    protected IApiMap<DomainObject.Lookup, ApiObject.Lookup> LookupMap { get; } = new LookupMap();
    protected IApiMap<DomainObject.LookupSet, ApiObject.LookupSet> LookupSetMap { get; } = new LookupSetMap();
    protected ILookupService LookupService => Service;
    protected ILookupSetService LookupSetService { get; }

    protected LookupController(IRegulationService regulationService, ILookupService lookupService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(regulationService, lookupService, runtime, new LookupMap())
    {
        LookupSetService = lookupSetService ?? throw new ArgumentNullException(nameof(lookupSetService));
    }

    protected override async Task<ActionResult<ApiObject.Lookup>> CreateAsync(int regulationId, ApiObject.Lookup lookup)
    {
        if (string.IsNullOrWhiteSpace(lookup.Name))
        {
            return BadRequest($"Lookup {lookup.Id} without name");
        }
        // unique lookup name per tenant
        if (await ChildService.ExistsAnyAsync(regulationId, new[] { lookup.Name }))
        {
            return BadRequest($"Lookup with name {lookup.Name} already exists");
        }

        return await base.CreateAsync(regulationId, lookup);
    }

    protected virtual async Task<ActionResult<ApiObject.Lookup[]>> CreateAsync(int regulationId, ApiObject.Lookup[] apiObjects)
    {
        var names = apiObjects.Select(x => x.Name).ToList();
        if (names.Count == 0)
        {
            return BadRequest("Missing lookup names");
        }
        // unique lookup name per tenant
        if (await ChildService.ExistsAnyAsync(regulationId, names))
        {
            foreach (var name in names)
            {
                // find the conflicting name
                if (await ChildService.ExistsAnyAsync(regulationId, new[] { name }))
                {
                    return BadRequest($"Lookup with name {name} already exists");
                }
            }
            return BadRequest("Lookup with existing name");
        }

        return LookupMap.ToApi(await ChildService.CreateAsync(regulationId, LookupMap.ToDomain(apiObjects)));
    }

    #region Sets

    /// <summary>
    /// Query items
    /// </summary>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="query">The query</param>
    /// <returns>Items, count or both</returns>
    protected virtual async Task<ActionResult> QueryLookupSetsAsync(int regulationId, Query query = null)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;
        switch (query.Result)
        {
            case QueryResultType.Items:
                var items = await QuerySetsAsync(regulationId, query);
                return items.IsValidResult() ? Ok(items.Value) : items.Result;
            case QueryResultType.Count:
                var count = await QueryCountAsync(regulationId, query);
                return count.IsValidResult() ? Ok(count.Value) : count.Result;
            case QueryResultType.ItemsWithCount:
                items = await QuerySetsAsync(regulationId, query);
                count = await QueryCountAsync(regulationId, query);
                return items.IsValidResult() && count.IsValidResult() ?
                    Ok(new QueryResult<ApiObject.LookupSet>(items.Value, count.Value)) : items.Result;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected async Task<ActionResult<ApiObject.LookupSet[]>> QuerySetsAsync(int regulationId, Query query = null)
    {
        try
        {
            // parent check
            if (regulationId <= 0)
            {
                return InvalidParentRequest(regulationId);
            }
            // existing parent check
            if (!await ParentService.ExistsAsync(regulationId))
            {
                return NotFound(typeof(IRegulationService), regulationId);
            }

            var apiObjects = new List<ApiObject.LookupSet>();
            var items = (await LookupSetService.QueryAsync(regulationId, query)).ToList();
            foreach (var item in items)
            {
                apiObjects.Add(LookupSetMap.ToApi(item));
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

    protected virtual async Task<ActionResult<ApiObject.LookupSet>> GetSetAsync(int tenantId, int regulationId, int lookupId)
    {
        try
        {
            // argument check
            if (lookupId <= 0)
            {
                return UndefinedObjectIdRequest();
            }

            // get object
            var lookupSet = await LookupSetService.GetSetAsync(tenantId, regulationId, lookupId);
            if (lookupSet == null)
            {
                return ObjectNotFoundRequest(lookupId);
            }
            return LookupSetMap.ToApi(lookupSet);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult> CreateSetsAsync(int regulationId, IEnumerable<ApiObject.LookupSet> lookupSets)
    {
        try
        {
            await LookupSetService.CreateAsync(regulationId, LookupSetMap.ToDomain(lookupSets));
            return Ok();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult> DeleteSetAsync(int regulationId, int lookupId)
    {
        try
        {
            await LookupSetService.DeleteAsync(regulationId, lookupId);
            return Ok();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

}
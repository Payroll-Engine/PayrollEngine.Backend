﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Lookup values")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups/{lookupId}/values")]
public class LookupValueController : Api.Controller.LookupValueController
{
    /// <inheritdoc/>
    public LookupValueController(ILookupService lookupService, ILookupValueService lookupValueService,
        IControllerRuntime runtime) :
        base(lookupService, lookupValueService, runtime)
    {
    }

    /// <summary>
    /// Query regulation lookup values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation lookup values</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLookupValues")]
    public async Task<ActionResult> QueryLookupValuesAsync(int tenantId,
        int regulationId, int lookupId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(lookupId, query);
    }

    /// <summary>
    /// Get lookup values data
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="culture">The culture</param>
    /// <returns>The lookup value data</returns>
    [HttpGet("data")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookupValuesData")]
    public override async Task<ActionResult<ApiObject.LookupValueData[]>> GetLookupValuesDataAsync(
        int tenantId, int regulationId, int lookupId, [FromQuery] string culture)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetLookupValuesDataAsync(tenantId, regulationId, lookupId, culture);
    }

    /// <summary>
    /// Get a regulation lookup value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="lookupValueId">The id of the lookup value</param>
    [HttpGet("{lookupValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookupValue")]
    public async Task<ActionResult<ApiObject.LookupValue>> GetLookupValueAsync(
        int tenantId, int regulationId, int lookupId, int lookupValueId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(lookupId, lookupValueId);
    }

    /// <summary>
    /// Add a new regulation lookup value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="lookup">The lookup value to add</param>
    /// <returns>The newly created lookup value</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateLookupValue")]
    public async Task<ActionResult<ApiObject.LookupValue>> CreateLookupValueAsync(
        int tenantId, int regulationId, int lookupId, ApiObject.LookupValue lookup)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(lookupId, lookup);
    }

    /// <summary>
    /// Update a regulation lookup value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="lookup">The lookup value with updated values</param>
    /// <returns>The modified lookup value</returns>
    [HttpPut("{lookupValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateLookupValue")]
    public async Task<ActionResult<ApiObject.LookupValue>> UpdateLookupValueAsync(
        int tenantId, int regulationId, int lookupId, ApiObject.LookupValue lookup)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(lookupId, lookup);
    }

    /// <summary>
    /// Delete a regulation lookup value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="lookupValueId">The id of the lookup value</param>
    [HttpDelete("{lookupValueId}")]
    [ApiOperationId("DeleteLookupValue")]
    public async Task<IActionResult> DeleteLookupValueAsync(int tenantId,
        int regulationId, int lookupId, int lookupValueId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(lookupId, lookupValueId);
    }
}
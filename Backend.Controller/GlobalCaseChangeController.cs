﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using DomainObject =  PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Global case changes")]
[Route("api/tenants/{tenantId}/globalcases/changes")]
public class GlobalCaseChangeController : Api.Controller.GlobalCaseChangeController
{
    /// <inheritdoc/>
    public GlobalCaseChangeController(ITenantService tenantService, IGlobalCaseChangeService caseChangeService,
        IControllerRuntime runtime) :
        base(tenantService, caseChangeService, runtime)
    {
    }

    /// <summary>
    /// Query global case changes
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case value changes array</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryGlobalCaseChanges")]
    public async Task<ActionResult> QueryGlobalCaseChangesAsync(int tenantId, 
        [FromQuery] DomainObject.CaseChangeQuery query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryAsync(tenantId, tenantId, query);
    }

    /// <summary>
    /// Get a global case change
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseChangeId">The case value change id</param>
    /// <returns>The case value change</returns>
    [HttpGet("{caseChangeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetGlobalCaseChange")]
    public async Task<ActionResult<ApiObject.CaseChange>> GetGlobalCaseChangeAsync(
        int tenantId, int caseChangeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, caseChangeId);
    }

    /// <summary>
    /// Query global case changes values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case change values array</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryGlobalCaseChangesValues")]
    public async Task<ActionResult> QueryGlobalCaseChangesValuesAsync(int tenantId, 
        [FromQuery] DomainObject.CaseChangeQuery query) =>
        await QueryValuesAsync(tenantId, tenantId, query);

    /// <summary>
    /// Delete a global case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    [HttpDelete("{caseValueId}")]
    [ApiOperationId("DeleteGlobalCaseChange")]
    public async Task<IActionResult> DeleteGlobalCaseChangeAsync(int tenantId, int caseValueId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, caseValueId);
    }
}
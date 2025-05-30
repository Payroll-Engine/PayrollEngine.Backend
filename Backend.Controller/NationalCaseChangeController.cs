﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using DomainObject =  PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("National case changes")]
[Route("api/tenants/{tenantId}/nationalcases/changes")]
public class NationalCaseChangeController : Api.Controller.NationalCaseChangeController
{
    /// <inheritdoc/>
    public NationalCaseChangeController(ITenantService tenantService, INationalCaseChangeService caseChangeService,
         IControllerRuntime runtime) :
        base(tenantService, caseChangeService, runtime)
    {
    }

    
    /// <summary>
    /// Query national case changes
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case value changes array</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryNationalCaseChanges")]
    public async Task<ActionResult> QueryNationalCaseChangesAsync(int tenantId,
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
    /// Get a national case change
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseChangeId">The case value change id</param>
    /// <returns>The case value change</returns>
    [HttpGet("{caseChangeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetNationalCaseChange")]
    public async Task<ActionResult<ApiObject.CaseChange>> GetNationalCaseChangeAsync(
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
    /// Query national case changes values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case change values array</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryNationalCaseChangesValues")]
    public async Task<ActionResult> QueryNationalCaseChangesValuesAsync(
        int tenantId, [FromQuery] DomainObject.CaseChangeQuery query) =>
        await QueryValuesAsync(tenantId, tenantId, query);

    /// <summary>
    /// Delete a national case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    [HttpDelete("{caseValueId}")]
    [ApiOperationId("DeleteNationalCaseChange")]
    public async Task<IActionResult> DeleteNationalCaseChangeAsync(int tenantId, int caseValueId)
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Company case changes")]
[Route("api/tenants/{tenantId}/companycases/changes")]
public class CompanyCaseChangeController : Api.Controller.CompanyCaseChangeController
{
    /// <inheritdoc/>
    public CompanyCaseChangeController(ITenantService tenantService, ICompanyCaseChangeService caseChangeService,
        IControllerRuntime runtime) :
        base(tenantService, caseChangeService, runtime)
    {
    }

    /// <summary>
    /// Query company case changes
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case value changes array</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCompanyCaseChanges")]
    public async Task<ActionResult> QueryCompanyCaseChangesAsync(int tenantId, [FromQuery] DomainObject.CaseChangeQuery query)
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
    /// Get a company case change
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseChangeId">The case value change id</param>
    /// <returns>The case value change</returns>
    [HttpGet("{caseChangeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCompanyCaseChange")]
    public async Task<ActionResult<ApiObject.CaseChange>> GetCompanyCaseChangeAsync(int tenantId, int caseChangeId)
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
    /// Query company case changes values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case change values array</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCompanyCaseChangesValues")]
    public async Task<ActionResult> QueryCompanyCaseChangesValuesAsync(int tenantId, [FromQuery] DomainObject.CaseChangeQuery query) =>
        await QueryValuesAsync(tenantId, tenantId, query);

    /// <summary>
    /// Delete a company case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <returns></returns>
    [HttpDelete("{caseValueId}")]
    [ApiOperationId("DeleteCompanyCaseChange")]
    public async Task<IActionResult> DeleteCompanyCaseChangeAsync(int tenantId, int caseValueId)
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
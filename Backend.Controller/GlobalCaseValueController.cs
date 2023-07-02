using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Global case values")]
[Route("api/tenants/{tenantId}/globalcases")]
public class GlobalCaseValueController : Api.Controller.GlobalCaseValueController
{
    /// <inheritdoc/>
    public GlobalCaseValueController(ITenantService tenantService, IPayrollService payrollService,
        IRegulationService regulationService, IGlobalCaseValueService caseValueService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(tenantService, caseValueService, payrollService, regulationService, lookupSetService, runtime)
    {
    }

    /// <summary>
    /// Query global case values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The global case values</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryGlobalCaseValues")]
    public async Task<ActionResult> QueryGlobalCaseValuesAsync(int tenantId, 
        [FromQuery] CaseValueQuery query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        if (!await ParentService.ExistsAsync(Runtime.DbContext, tenantId))
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get global case value slots
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value slots</returns>
    [HttpGet("slots")]
    [OkResponse]
    [ApiOperationId("GetGlobalCaseValueSlots")]
    public async Task<ActionResult<IEnumerable<string>>> GetGlobalCaseValueSlotsAsync(
        int tenantId, [Required] string caseFieldName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return Ok(await GetCaseValueSlotsAsync(tenantId, caseFieldName));
    }

    /// <summary>
    /// Get a global case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The global case value id</param>
    /// <returns>The case value</returns>
    [HttpGet("{caseValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetGlobalCaseValue")]
    public async Task<ActionResult<ApiObject.CaseValue>> GetGlobalCaseValueAsync(
        int tenantId, int caseValueId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, caseValueId);
    }
}
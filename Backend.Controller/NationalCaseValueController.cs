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
[ApiControllerName("National case values")]
[Route("api/tenants/{tenantId}/nationalcases")]
public class NationalCaseValueController : Api.Controller.NationalCaseValueController
{
    /// <inheritdoc/>
    public NationalCaseValueController(ITenantService tenantService, IPayrollService payrollService,
        IRegulationService regulationService, INationalCaseValueService caseValueService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(tenantService, caseValueService, payrollService, regulationService, lookupSetService, runtime)
    {
    }
        /// <summary>
    /// Query national case values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The national case values</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryNationalCaseValues")]
    public async Task<ActionResult> QueryNationalCaseValuesAsync(int tenantId,
            [FromQuery] CaseValueQuery query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        if (!await ParentService.ExistsAsync(Runtime.DbContext, tenantId))
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get national case value slots
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value slots</returns>
    [HttpGet("slots")]
    [OkResponse]
    [ApiOperationId("GetNationalCaseValueSlots")]
    public async Task<ActionResult<IEnumerable<string>>> GetNationalCaseValueSlotsAsync(
        int tenantId, [Required] string caseFieldName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return Ok(await GetCaseValueSlotsAsync(tenantId, caseFieldName));
    }

    /// <summary>
    /// Get a national case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The national case value id</param>
    /// <returns>The case value</returns>
    [HttpGet("{caseValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetNationalCaseValue")]
    public async Task<ActionResult<ApiObject.CaseValue>> GetNationalCaseValueAsync(
        int tenantId, int caseValueId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, caseValueId);
    }

}
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class CompanyCaseValueController : Api.Controller.CompanyCaseValueController
{
    /// <inheritdoc/>
    public CompanyCaseValueController(ITenantService tenantService, ICompanyCaseValueService caseValueService,
        IPayrollService payrollsService, IRegulationService regulationService, ILookupSetService lookupSetService,
        IControllerRuntime runtime) :
        base(tenantService, caseValueService, payrollsService, regulationService, lookupSetService, runtime)
    {
    }

    /// <summary>
    /// Query company case values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case values</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCompanyCaseValues")]
    public async Task<ActionResult> QueryCompanyCaseValuesAsync(int tenantId, [FromQuery] CaseValueQuery query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get company case value slots
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value slots</returns>
    [HttpGet("slots")]
    [OkResponse]
    [ApiOperationId("GetCompanyCaseValueSlots")]
    public async Task<ActionResult<IEnumerable<string>>> GetCompanyCaseValueSlotsAsync(int tenantId, [Required] string caseFieldName)
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
        return Ok(await GetCaseValueSlotsAsync(tenantId, caseFieldName));
    }

    /// <summary>
    /// Get a company case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The company case value id</param>
    /// <returns>The case value</returns>
    [HttpGet("{caseValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCompanyCaseValue")]
    public async Task<ActionResult<ApiObject.CaseValue>> GetCompanyCaseValue(int tenantId, int caseValueId)
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
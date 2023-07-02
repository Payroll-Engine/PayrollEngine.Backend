using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Cases")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases")]
public class CaseController : Api.Controller.CaseController
{
    /// <inheritdoc/>
    public CaseController(IRegulationService regulationService, ICaseService caseService,
        IControllerRuntime runtime) :
        base(regulationService, caseService, runtime)
    {
    }

    /// <summary>
    /// Query all regulation cases
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation cases</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCases")]
    public async Task<ActionResult> QueryCasesAsync(int tenantId, int regulationId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a regulation case
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseId">The case id</param>
    /// <returns>The regulation case</returns>
    [HttpGet("{caseId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCase")]
    public async Task<ActionResult<ApiObject.Case>> GetCaseAsync(int tenantId, int regulationId, int caseId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(regulationId, caseId);
    }

    /// <summary>
    /// Add a new regulation case
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="case">The case to add</param>
    /// <returns>The newly created regulation case</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateCase")]
    public async Task<ActionResult<ApiObject.Case>> CreateCaseAsync(int tenantId, int regulationId, ApiObject.Case @case)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(regulationId, @case);
    }

    /// <summary>
    /// Update a regulation case
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="case">The case with updated values</param>
    /// <returns>The modified regulation case</returns>
    [HttpPut("{caseId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateCase")]
    public async Task<ActionResult<ApiObject.Case>> UpdateCaseAsync(int tenantId, int regulationId, ApiObject.Case @case)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(regulationId, @case);
    }

    /// <summary>
    /// Rebuild regulation case
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseId">The case id</param>
    [HttpPut("{caseId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildCase")]
    public async Task<ActionResult> RebuildCaseAsync(int tenantId, int regulationId, int caseId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await RebuildAsync(regulationId, caseId);
    }

    /// <summary>
    /// Delete a regulation case
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseId">The case id</param>
    [HttpDelete("{caseId}")]
    [ApiOperationId("DeleteCase")]
    public async Task<IActionResult> DeleteCaseAsync(int tenantId, int regulationId, int caseId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(regulationId, caseId);
    }
}
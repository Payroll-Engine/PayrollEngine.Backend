using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Regulations")]
[Route("api/tenants/{tenantId}/regulations")]
public class RegulationController : Api.Controller.RegulationController
{
    /// <inheritdoc/>
    public RegulationController(ITenantService tenantService, IRegulationService regulationService,
        ICaseService caseService, ICaseFieldService caseFieldService, IControllerRuntime runtime) :
        base(tenantService, regulationService, caseService, caseFieldService, runtime)
    {
    }

    /// <summary>
    /// Query regulations
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant regulations</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryRegulations")]
    public async Task<ActionResult> QueryRegulationsAsync(int tenantId, [FromQuery] Query query)
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
    /// Get a regulation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <returns></returns>
    [HttpGet("{regulationId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetRegulation")]
    public async Task<ActionResult<ApiObject.Regulation>> GetRegulationAsync(
        int tenantId, int regulationId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, regulationId);
    }

    /// <summary>
    /// Get case name by case field
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case name</returns>
    [HttpGet("cases/{caseFieldName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCaseOfCaseField")]
    public override async Task<ActionResult<string>> GetCaseOfCaseFieldAsync(
        int tenantId, [Required] string caseFieldName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetCaseOfCaseFieldAsync(tenantId, caseFieldName);
    }

    /// <summary>
    /// Add a new regulation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulation">The regulation to add</param>
    /// <returns>The newly created regulation regulation</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateRegulation")]
    public async Task<ActionResult<ApiObject.Regulation>> CreateRegulationAsync(
        int tenantId, ApiObject.Regulation regulation)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(tenantId, regulation);
    }

    /// <summary>
    /// Update a regulation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulation">The regulation id</param>
    /// <returns>The modified regulation regulation</returns>
    [HttpPut("{regulationId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateRegulation")]
    public async Task<ActionResult<ApiObject.Regulation>> UpdateRegulationAsync(
        int tenantId, ApiObject.Regulation regulation)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(tenantId, regulation);
    }

    /// <summary>
    /// Delete a regulation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <returns></returns>
    [HttpDelete("{regulationId}")]
    [ApiOperationId("DeleteRegulation")]
    public async Task<IActionResult> DeleteRegulationAsync(int tenantId, int regulationId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, regulationId);
    }

    #region Attributes

    /// <summary>
    /// Get a regulation attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The id of the regulation</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{regulationId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetRegulationAttribute")]
    public virtual async Task<ActionResult<string>> GetRegulationAttributeAsync(
        int tenantId, int regulationId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAttributeAsync(regulationId, attributeName);
    }

    /// <summary>
    /// Set a regulation attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The id of the regulation</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{regulationId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetRegulationAttribute")]
    public virtual async Task<ActionResult<string>> SetRegulationAttributeAsync(
        int tenantId, int regulationId, string attributeName,
        [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await SetAttributeAsync(regulationId, attributeName, value);
    }

    /// <summary>
    /// Delete a regulation attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The id of the regulation</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{regulationId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteRegulationAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteRegulationAttributeAsync(
        int tenantId, int regulationId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAttributeAsync(regulationId, attributeName);
    }

    #endregion

}
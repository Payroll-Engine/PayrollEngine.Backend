using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Divisions")]
[Route("api/tenants/{tenantId}/divisions")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Division)]
public class DivisionController : Api.Controller.DivisionController
{
    /// <inheritdoc/>
    public DivisionController(ITenantService tenantService, IDivisionService divisionService, IControllerRuntime runtime) :
        base(tenantService, divisionService, runtime)
    {
    }

    /// <summary>
    /// Query divisions
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant divisions</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryDivisions")]
    public async Task<ActionResult> QueryDivisionsAsync(int tenantId, [FromQuery] Query query)
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
    /// Get a division
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The id of the division</param>
    /// <returns></returns>
    [HttpGet("{divisionId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetDivision")]
    public async Task<ActionResult<ApiObject.Division>> GetDivisionAsync(int tenantId, int divisionId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, divisionId);
    }

    /// <summary>
    /// Add a new division
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="division">The division to add</param>
    /// <returns>The newly created division</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateDivision")]
    public async Task<ActionResult<ApiObject.Division>> CreateDivisionAsync(int tenantId, ApiObject.Division division)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(tenantId, division);
    }

    /// <summary>
    /// Update a division
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="division">The division with updated values</param>
    /// <returns>The modified division</returns>
    [HttpPut("{divisionId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateDivision")]
    public async Task<ActionResult<ApiObject.Division>> UpdateDivisionAsync(int tenantId, ApiObject.Division division)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(tenantId, division);
    }

    /// <summary>
    /// Delete a division
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The id of the division</param>
    /// <returns></returns>
    [HttpDelete("{divisionId}")]
    [ApiOperationId("DeleteDivision")]
    public async Task<IActionResult> DeleteDivisionAsync(int tenantId, int divisionId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, divisionId);
    }

    #region Attributes

    /// <summary>
    /// Get a division attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The id of the division</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{divisionId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetDivisionAttribute")]
    public virtual async Task<ActionResult<string>> GetDivisionAttributeAsync(int tenantId, int divisionId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAttributeAsync(divisionId, attributeName);
    }

    /// <summary>
    /// Set a division attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The id of the division</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{divisionId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetDivisionAttribute")]
    public virtual async Task<ActionResult<string>> SetDivisionAttributeAsync(int tenantId, int divisionId, string attributeName,
        [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.SetAttributeAsync(divisionId, attributeName, value);
    }

    /// <summary>
    /// Delete a division attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The id of the division</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{divisionId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteDivisionAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteDivisionAttributeAsync(int tenantId, int divisionId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.DeleteAttributeAsync(divisionId, attributeName);
    }

    #endregion

}
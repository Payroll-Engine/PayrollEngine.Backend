using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payroll layers")]
[Route("api/tenants/{tenantId}/payrolls/{payrollId}/layers")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.PayrollLayer)]
public class PayrollLayerController : Api.Controller.PayrollLayerController
{
    /// <inheritdoc/>
    public PayrollLayerController(IPayrollService payrollService, IPayrollLayerService layerService,
        IControllerRuntime runtime) :
        base(payrollService, layerService, runtime)
    {
    }

    /// <summary>
    /// Query payroll layers
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll layers</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayrollLayers")]
    public async Task<ActionResult> QueryPayrollLayersAsync(int tenantId, int payrollId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(payrollId, query);
    }

    /// <summary>
    /// Get a payroll layer
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layerId">The payroll layer id</param>
    /// <returns>The payroll layer</returns>
    [HttpGet("{layerId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLayer")]
    public async Task<ActionResult<ApiObject.PayrollLayer>> GetPayrollLayerAsync(
        int tenantId, int payrollId, int layerId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(payrollId, layerId);
    }

    /// <summary>
    /// Add a new payroll layer
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layer">The payroll layer to add</param>
    /// <returns>The newly created payroll layer</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreatePayrollLayer")]
    public async Task<ActionResult<ApiObject.PayrollLayer>> CreatePayrollLayerAsync(
        int tenantId, int payrollId, ApiObject.PayrollLayer layer)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(payrollId, layer);
    }

    /// <summary>
    /// Update a payroll layer
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layer">The payroll layer with updated values</param>
    /// <returns>The modified payroll layer</returns>
    [HttpPut("{layerId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdatePayrollLayer")]
    public async Task<ActionResult<ApiObject.PayrollLayer>> UpdatePayrollLayerAsync(
        int tenantId, int payrollId, ApiObject.PayrollLayer layer)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(payrollId, layer);
    }

    /// <summary>
    /// Delete a payroll layer
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layerId">The id of the payroll layer</param>
    [HttpDelete("{layerId}")]
    [ApiOperationId("DeletePayrollLayer")]
    public async Task<IActionResult> DeletePayrollLayerAsync(int tenantId, int payrollId, int layerId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(payrollId, layerId);
    }

    #region Attributes

    /// <summary>
    /// Get a payroll layer attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layerId">The id of the payroll layer</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{layerId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLayerAttribute")]
    public virtual async Task<ActionResult<string>> GetPayrollLayerAttributeAsync(
        int tenantId, int payrollId, int layerId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAttributeAsync(layerId, attributeName);
    }

    /// <summary>
    /// Set a payroll layer attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layerId">The id of the payroll layer</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{layerId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetPayrollLayerAttribute")]
    public virtual async Task<ActionResult<string>> SetPayrollLayerAttributeAsync(
        int tenantId, int payrollId, int layerId, string attributeName, [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await SetAttributeAsync(layerId, attributeName, value);
    }

    /// <summary>
    /// Delete a payroll layer attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="layerId">The id of the payroll layer</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{layerId}/attributes/{attributeName}")]
    [ApiOperationId("DeletePayrollLayerAttribute")]
    public virtual async Task<ActionResult<bool>> DeletePayrollLayerAttributeAsync(
        int tenantId, int payrollId, int layerId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAttributeAsync(layerId, attributeName);
    }

    #endregion

}
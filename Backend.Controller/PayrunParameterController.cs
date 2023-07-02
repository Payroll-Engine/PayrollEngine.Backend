using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payrun parameters")]
[Route("api/tenants/{tenantId}/payruns/{payrunId}/parameters")]
public class PayrunParameterController : Api.Controller.PayrunParameterController
{
    /// <inheritdoc/>
    public PayrunParameterController(IPayrunService payrunService, IPayrunParameterService payrunParameterService,
        IControllerRuntime runtime) :
        base(payrunService, payrunParameterService, runtime)
    {
    }

    /// <summary>
    /// Query payrun parameters
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payrun parameters</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayrunParameters")]
    public async Task<ActionResult> QueryPayrunParametersAsync(int tenantId, int payrunId, 
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(payrunId, query);
    }

    /// <summary>
    /// Get a payrun parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    /// <param name="parameterId">The id of the parameter</param>
    /// <returns>The payrun parameter</returns>
    [HttpGet("{parameterId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrunParameter")]
    public async Task<ActionResult<ApiObject.PayrunParameter>> GetPayrunParameterAsync(
        int tenantId, int payrunId, int parameterId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(payrunId, parameterId);
    }

    /// <summary>
    /// Add a new payrun parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    /// <param name="parameter">The payrun parameter to add</param>
    /// <returns>The newly created payrun parameter</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreatePayrunParameter")]
    public async Task<ActionResult<ApiObject.PayrunParameter>> CreatePayrunParameterAsync(
        int tenantId, int payrunId, ApiObject.PayrunParameter parameter)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(payrunId, parameter);
    }

    /// <summary>
    /// Update a payrun parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    /// <param name="parameter">The payrun parameter to modify</param>
    /// <returns>The modified parameter</returns>
    [HttpPut("{parameterId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdatePayrun")]
    public async Task<ActionResult<ApiObject.PayrunParameter>> UpdatePayrunAsync(
        int tenantId, int payrunId, ApiObject.PayrunParameter parameter)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(payrunId, parameter);
    }

    /// <summary>
    /// Delete a payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    /// <param name="parameterId">The id of the payrun parameter</param>
    /// <returns></returns>
    [HttpDelete("{parameterId}")]
    [ApiOperationId("DeletePayrun")]
    public async Task<IActionResult> DeletePayrunAsync(int tenantId, int payrunId, int parameterId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(payrunId, parameterId);
    }
}
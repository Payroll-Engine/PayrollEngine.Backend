using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payruns")]
[Route("api/tenants/{tenantId}/payruns")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Payrun)]
public class PayrunController : Api.Controller.PayrunController
{
    /// <inheritdoc/>
    public PayrunController(ITenantService tenantService, IPayrunService payrunService,
        IPayrollService payrollService, IControllerRuntime runtime) :
        base(tenantService, payrunService, payrollService, runtime)
    {
    }

    /// <summary>
    /// Query payruns
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant payruns</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayruns")]
    public async Task<ActionResult> QueryPayrunsAsync(int tenantId, [FromQuery] Query query)
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
    /// Get a payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The id of the payrun</param>
    /// <returns></returns>
    [HttpGet("{payrunId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrun")]
    public async Task<ActionResult<ApiObject.Payrun>> GetPayrunAsync(int tenantId, int payrunId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, payrunId);
    }

    /// <summary>
    /// Add a new payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrun">The payrun to add</param>
    /// <returns>The newly created payrun</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreatePayrun")]
    public async Task<ActionResult<ApiObject.Payrun>> CreatePayrunAsync(int tenantId, ApiObject.Payrun payrun)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(tenantId, payrun);
    }

    /// <summary>
    /// Update a payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrun">The payrun with updated values</param>
    /// <returns>The modified payrun</returns>
    [HttpPut("{payrunId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdatePayrun")]
    public async Task<ActionResult<ApiObject.Payrun>> UpdatePayrunAsync(int tenantId, ApiObject.Payrun payrun)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(tenantId, payrun);
    }

    /// <summary>
    /// Rebuild payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The id of the payrun</param>
    [HttpPut("{payrunId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildPayrun")]
    public async Task<ActionResult> RebuildPayrunAsync(int tenantId, int payrunId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await RebuildAsync(tenantId, payrunId);
    }

    /// <summary>
    /// Delete a payrun
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The id of the payrun</param>
    /// <returns></returns>
    [HttpDelete("{payrunId}")]
    [ApiOperationId("DeletePayrun")]
    public async Task<IActionResult> DeletePayrunAsync(int tenantId, int payrunId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, payrunId);
    }
}
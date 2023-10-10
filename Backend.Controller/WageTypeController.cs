using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Wage types")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/wagetypes")]
public class WageTypeController : Api.Controller.WageTypeController
{
    /// <inheritdoc/>
    public WageTypeController(IRegulationService regulationService, IWageTypeService wageTypeService,
        IControllerRuntime runtime) :
        base(regulationService, wageTypeService, runtime)
    {
    }

    /// <summary>
    /// Query regulation wage types
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation wage types</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryWageTypes")]
    public async Task<ActionResult> QueryWageTypesAsync(int tenantId, int regulationId,
        [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a regulation wageType
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageTypeId">The wage type id</param>
    /// <returns>The regulation wageType</returns>
    [HttpGet("{wageTypeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetWageType")]
    public async Task<ActionResult<ApiObject.WageType>> GetWageTypeAsync(
        int tenantId, int regulationId, int wageTypeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(regulationId, wageTypeId);
    }

    /// <summary>
    /// Add a new regulation wage type
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageType">The wage type to add</param>
    /// <returns>The newly created regulation wage type</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateWageType")]
    public async Task<ActionResult<ApiObject.WageType>> CreateWageTypeAsync(
        int tenantId, int regulationId, ApiObject.WageType wageType)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(regulationId, wageType);
    }

    /// <summary>
    /// Update a regulation wage type
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageType">The wage type with updated values</param>
    /// <returns>The modified regulation wage type</returns>
    [HttpPut("{wageTypeId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateWageType")]
    public async Task<ActionResult<ApiObject.WageType>> UpdateWageTypeAsync(
        int tenantId, int regulationId, ApiObject.WageType wageType)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(regulationId, wageType);
    }

    /// <summary>
    /// Rebuild regulation wage type
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageTypeId">The id of the wage type</param>
    [HttpPut("{wageTypeId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildWageType")]
    public async Task<ActionResult> RebuildWageTypeAsync(int tenantId,
        int regulationId, int wageTypeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await RebuildAsync(regulationId, wageTypeId);
    }

    /// <summary>
    /// Delete a regulation wage type
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageTypeId">The id of the wage type</param>
    [HttpDelete("{wageTypeId}")]
    [ApiOperationId("DeleteWageType")]
    public async Task<IActionResult> DeleteWageTypeAsync(int tenantId,
        int regulationId, int wageTypeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(regulationId, wageTypeId);
    }
}
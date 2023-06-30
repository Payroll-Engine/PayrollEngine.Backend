using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Lookups")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Lookup)]
public class LookupController : Api.Controller.LookupController
{
    /// <inheritdoc/>
    public LookupController(IRegulationService regulationService, ILookupService lookupService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(regulationService, lookupService, lookupSetService, runtime)
    {
    }

    /// <summary>
    /// Query regulation lookups
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation lookups</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLookups")]
    public async Task<ActionResult> QueryLookupsAsync(int tenantId, 
        int regulationId, [FromQuery] Query query)
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
    /// Get a regulation lookup
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <returns></returns>
    [HttpGet("{lookupId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookup")]
    public async Task<ActionResult<ApiObject.Lookup>> GetLookupAsync(int tenantId,
        int regulationId, int lookupId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(regulationId, lookupId);
    }

    /// <summary>
    /// Add a new regulation lookup
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookup">The lookup to add</param>
    /// <returns>The newly created lookup</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateLookup")]
    public async Task<ActionResult<ApiObject.Lookup>> CreateLookupAsync(int tenantId,
        int regulationId, ApiObject.Lookup lookup)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(regulationId, lookup);
    }

    /// <summary>
    /// Update a regulation lookup
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookup">The lookup with updated values</param>
    /// <returns>The modified lookup</returns>
    [HttpPut("{lookupId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateLookup")]
    public async Task<ActionResult<ApiObject.Lookup>> UpdateLookupAsync(int tenantId,
        int regulationId, ApiObject.Lookup lookup)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(regulationId, lookup);
    }

    /// <summary>
    /// Delete a regulation lookup
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <returns></returns>
    [HttpDelete("{lookupId}")]
    [ApiOperationId("DeleteLookup")]
    public async Task<IActionResult> DeleteLookupAsync(int tenantId, int regulationId, int lookupId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(regulationId, lookupId);
    }

    #region Sets

    /// <summary>
    /// Query regulation lookup sets
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation lookups</returns>
    [HttpGet("sets")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLookupSets")]
    public override async Task<ActionResult> QueryLookupSetsAsync(int tenantId,
        int regulationId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.QueryLookupSetsAsync(tenantId, regulationId, query);
    }

    /// <summary>
    /// Get a regulation lookup set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <returns></returns>
    [HttpGet("sets/{lookupId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookupSet")]
    public async Task<ActionResult<ApiObject.LookupSet>> GetLookupSetAsync(int tenantId,
        int regulationId, int lookupId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetSetAsync(tenantId, regulationId, lookupId);
    }

    /// <summary>
    /// Add lookup set including the lookup values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupSets">The lookups to add</param>
    [HttpPost("sets")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateLookupSets")]
    public async Task<ActionResult> CreateLookupSetsAsync(int tenantId,
        int regulationId, ApiObject.LookupSet[] lookupSets)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateSetsAsync(regulationId, lookupSets);
    }

    /// <summary>
    /// Delete lookup set including the lookup values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    [HttpDelete("sets/{lookupId}")]
    [ApiOperationId("DeleteLookupSet")]
    public async Task<ActionResult> DeleteLookupSetAsync(int tenantId, int regulationId, int lookupId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteSetAsync(regulationId, lookupId);
    }

    #endregion

}
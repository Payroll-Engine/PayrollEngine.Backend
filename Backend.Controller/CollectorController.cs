using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Collectors")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/collectors")]
public class CollectorController : Api.Controller.CollectorController
{
    /// <inheritdoc/>
    public CollectorController(IRegulationService regulationService, ICollectorService collectorService, 
        IControllerRuntime runtime) :
        base(regulationService, collectorService, runtime)
    {
    }

    /// <summary>
    /// Get regulation collectors
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation collectors</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCollectors")]
    public async Task<ActionResult> QueryCollectorsAsync(int tenantId, int regulationId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a regulation collector
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collectorId">The collector id</param>
    /// <returns>The regulation collector</returns>
    [HttpGet("{collectorId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCollector")]
    public async Task<ActionResult<ApiObject.Collector>> GetCollectorAsync(int tenantId, int regulationId, int collectorId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(regulationId, collectorId);
    }

    /// <summary>
    /// Add a new regulation collector
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collector">The collector to add</param>
    /// <returns>The newly created regulation collector</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateCollector")]
    public async Task<ActionResult<ApiObject.Collector>> CreateCollectorAsync(int tenantId,
        int regulationId, ApiObject.Collector collector)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(regulationId, collector);
    }

    /// <summary>
    /// Update a regulation collector
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collector">The collector with updated values</param>
    /// <returns>The modified regulation collector</returns>
    [HttpPut("{collectorId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateCollector")]
    public async Task<ActionResult<ApiObject.Collector>> UpdateCollectorAsync(int tenantId,
        int regulationId, ApiObject.Collector collector)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(regulationId, collector);
    }

    /// <summary>
    /// Rebuild regulation collector
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collectorId">The id of the collector</param>
    [HttpPut("{collectorId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildCollector")]
    public async Task<ActionResult> RebuildCollectorAsync(int tenantId, int regulationId, int collectorId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await RebuildAsync(regulationId, collectorId);
    }

    /// <summary>
    /// Delete a regulation collector
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collectorId">The id of the collector</param>
    [HttpDelete("{collectorId}")]
    [ApiOperationId("DeleteCollector")]
    public async Task<IActionResult> DeleteCollectorAsync(int tenantId, int regulationId, int collectorId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(regulationId, collectorId);
    }
}
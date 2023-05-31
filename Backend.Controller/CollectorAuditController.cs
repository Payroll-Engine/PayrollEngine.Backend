using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Collector audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/collectors/{collectorId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CollectorAudit)]
public class CollectorAuditController : Api.Controller.CollectorAuditController
{
    /// <inheritdoc/>
    public CollectorAuditController(ICollectorService collectorService, ICollectorAuditService collectorsAuditService,
        IControllerRuntime runtime) :
        base(collectorService, collectorsAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation collector audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="collectorId">The id of the collector</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCollectorAudits")]
    public async Task<ActionResult> QueryCollectorAuditsAsync(int tenantId, int collectorId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(collectorId, query);
    }

    /// <summary>
    /// Get a regulation collector audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="collectorId">The collector id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCollectorAudit")]
    public async Task<ActionResult<ApiObject.CollectorAudit>> GetCollectorAuditAsync(int tenantId, int collectorId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(collectorId, auditId);
    }
}
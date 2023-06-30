using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Lookup audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups/{lookupId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.LookupAudit)]
public class LookupAuditController : Api.Controller.LookupAuditController
{
    /// <inheritdoc/>
    public LookupAuditController(ILookupService lookupService,
        ILookupAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(lookupService, caseFieldAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation lookup audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLookupAudits")]
    public async Task<ActionResult> QueryLookupAuditsAsync(int tenantId, int regulationId,
        int lookupId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(lookupId, query);
    }

    /// <summary>
    /// Get a regulation lookup audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookupAudit")]
    public async Task<ActionResult<ApiObject.LookupAudit>> GetLookupAuditAsync(int tenantId,
        int regulationId, int lookupId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(lookupId, auditId);
    }
}
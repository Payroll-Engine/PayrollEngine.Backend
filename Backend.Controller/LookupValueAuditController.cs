using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Lookup value audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups/{lookupId}/values/{lookupValueId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.LookupValueAudit)]
public class LookupValueAuditController : Api.Controller.LookupValueAuditController
{
    /// <inheritdoc/>
    public LookupValueAuditController(ILookupValueService lookupValueService, 
        ILookupValueAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(lookupValueService, caseFieldAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation lookup value audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupValueId">The lookup value id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLookupValueAudits")]
    public async Task<ActionResult> QueryLookupValueAuditsAsync(int tenantId, int lookupValueId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(lookupValueId, query);
    }

    /// <summary>
    /// Get a regulation lookup audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupValueId">The lookup value id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLookupValueAudit")]
    public async Task<ActionResult<ApiObject.LookupValueAudit>> GetLookupValueAuditAsync(int tenantId, int lookupValueId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(lookupValueId, auditId);
    }
}
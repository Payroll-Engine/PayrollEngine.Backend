using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Case relation audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/caserelations({relationId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseRelationAudit)]
public class CaseRelationAuditController : Api.Controller.CaseRelationAuditController
{
    /// <inheritdoc/>
    public CaseRelationAuditController(ICaseRelationService caseRelationService, 
        ICaseRelationAuditService caseRelationAuditService, IControllerRuntime runtime) :
        base(caseRelationService, caseRelationAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case relation audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="relationId">The case relation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCaseRelationAudits")]
    public async Task<ActionResult> QueryCaseRelationAuditsAsync(int tenantId, int regulationId, 
        int relationId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(relationId, query);
    }

    /// <summary>
    /// Get a regulation case relation audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="relationId">The relation id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCaseRelationAudit")]
    public async Task<ActionResult<ApiObject.CaseRelationAudit>> GetCaseRelationAuditAsync(int tenantId, 
        int regulationId, int relationId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(relationId, auditId);
    }
}
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Case field audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases/{caseId}/fields/{fieldId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseFieldAudit)]
public class CaseFieldAuditController : Api.Controller.CaseFieldAuditController
{
    /// <inheritdoc/>
    public CaseFieldAuditController(ICaseFieldService caseFieldService, 
        ICaseFieldAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(caseFieldService, caseFieldAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case field audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="fieldId">The case field id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCaseFieldAudits")]
    public async Task<ActionResult> QueryCaseFieldAuditsAsync(int tenantId, int fieldId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(fieldId, query);
    }

    /// <summary>
    /// Get a regulation case field audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="fieldId">The case field id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCaseFieldAudit")]
    public async Task<ActionResult<ApiObject.CaseFieldAudit>> GetCaseFieldAuditAsync(int tenantId, int fieldId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(fieldId, auditId);
    }
}
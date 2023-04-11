using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class CaseAuditController : Api.Controller.CaseAuditController
{
    /// <inheritdoc/>
    public CaseAuditController(ICaseService caseService, 
        ICaseAuditService caseAuditService, IControllerRuntime runtime) :
        base(caseService, caseAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id </param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCaseAudits")]
    public async Task<ActionResult> QueryCaseAuditsAsync(int tenantId, int caseId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(caseId, query);
    }

    /// <summary>
    /// Get a regulation case audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCaseAudit")]
    public async Task<ActionResult<ApiObject.CaseAudit>> GetCaseAuditAsync(int tenantId, int caseId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(caseId, auditId);
    }
}
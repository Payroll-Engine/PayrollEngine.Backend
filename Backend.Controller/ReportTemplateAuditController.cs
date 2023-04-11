using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class ReportTemplateAuditController : Api.Controller.ReportTemplateAuditController
{
    /// <inheritdoc/>
    public ReportTemplateAuditController(IReportTemplateService reportTemplateService,
        IReportTemplateAuditService auditService, IControllerRuntime runtime) :
        base(reportTemplateService, auditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation report template audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportTemplateId">The id of the report template</param>
    /// <param name="query">Query templates</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportTemplateAudits")]
    public async Task<ActionResult> QueryReportTemplateAuditsAsync(int tenantId, int reportTemplateId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(reportTemplateId, query);
    }

    /// <summary>
    /// Get a regulation report template audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportTemplateId">The report template id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportTemplateAudit")]
    public async Task<ActionResult<ApiObject.ReportTemplateAudit>> GetReportTemplateAuditAsync(int tenantId, int reportTemplateId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(reportTemplateId, auditId);
    }
}
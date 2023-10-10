using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Report template audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/templates/{templateId}/audits")]
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
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="templateId">The id of the report template</param>
    /// <param name="query">Query templates</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportTemplateAudits")]
    public async Task<ActionResult> QueryReportTemplateAuditsAsync(int tenantId, 
        int regulationId, int reportId, int templateId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(templateId, query);
    }

    /// <summary>
    /// Get a regulation report template audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="templateId">The report template id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportTemplateAudit")]
    public async Task<ActionResult<ApiObject.ReportTemplateAudit>> GetReportTemplateAuditAsync(
        int tenantId, int regulationId, int reportId, int templateId, int auditId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(templateId, auditId);
    }
}
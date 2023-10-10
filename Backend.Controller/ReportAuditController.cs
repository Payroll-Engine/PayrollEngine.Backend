using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
// ReSharper disable StringLiteralTypo
[ApiControllerName("Report audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/audits")]
// ReSharper restore StringLiteralTypo
public class ReportAuditController : Api.Controller.ReportAuditController
{
    /// <inheritdoc/>
    public ReportAuditController(IReportService reportService,
        IReportAuditService auditService, IControllerRuntime runtime) :
        base(reportService, auditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation report audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportAudits")]
    public async Task<ActionResult> QueryReportAuditsAsync(int tenantId, int regulationId,
        int reportId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(reportId, query);
    }

    /// <summary>
    /// Get a regulation report audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportAudit")]
    public async Task<ActionResult<ApiObject.ReportAudit>> GetReportAuditAsync(int tenantId,
        int regulationId, int reportId, int auditId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(reportId, auditId);
    }
}
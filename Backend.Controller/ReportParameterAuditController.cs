using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
// ReSharper disable StringLiteralTypo
[ApiControllerName("Report parameter audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/parameters/{parameterId}/audits")]
// ReSharper restore StringLiteralTypo
public class ReportParameterAuditController : Api.Controller.ReportParameterAuditController
{
    /// <inheritdoc/>
    public ReportParameterAuditController(IReportParameterService reportParameterService,
        IReportParameterAuditService auditService, IControllerRuntime runtime) :
        base(reportParameterService, auditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation report parameter audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="parameterId">The id of the report parameter</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportParameterAudits")]
    public async Task<ActionResult> QueryReportParameterAuditsAsync(int tenantId,
        int regulationId, int reportId, int parameterId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(parameterId, query);
    }

    /// <summary>
    /// Get a regulation report parameter audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="parameterId">The report parameter id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportParameterAudit")]
    public async Task<ActionResult<ApiObject.ReportParameterAudit>> GetReportParameterAuditAsync(
        int tenantId, int regulationId, int reportId, int parameterId, int auditId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(parameterId, auditId);
    }
}
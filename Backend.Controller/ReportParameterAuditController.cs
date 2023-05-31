using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
// ReSharper disable StringLiteralTypo
[ApiControllerName("Report parameter audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/parameters/{parameterId}/audits")]
// ReSharper restore StringLiteralTypo
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ReportParameterAudit)]
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
    /// <param name="reportParameterId">The id of the report parameter</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportParameterAudits")]
    public async Task<ActionResult> QueryReportParameterAuditsAsync(int tenantId, int reportParameterId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(reportParameterId, query);
    }

    /// <summary>
    /// Get a regulation report parameter audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportParameterId">The report parameter id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportParameterAudit")]
    public async Task<ActionResult<ApiObject.ReportParameterAudit>> GetReportParameterAuditAsync(int tenantId, int reportParameterId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(reportParameterId, auditId);
    }
}
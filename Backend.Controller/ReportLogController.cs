using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Report logs")]
[Route("api/tenants/{tenantId}/reportlogs")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ReportLog)]
public class ReportLogController : Api.Controller.ReportLogController
{
    /// <inheritdoc/>
    public ReportLogController(ITenantService tenantService, IReportLogService reportLogService,
        IControllerRuntime runtime) :
        base(tenantService, reportLogService, runtime)
    {
    }

    /// <summary>
    /// Query report logs
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query logs</param>
    /// <returns>The report logs</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportLogs")]
    public async Task<ActionResult> QueryReportLogsAsync(int tenantId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a report log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="logId">The id of the log</param>
    /// <returns>The report log</returns>
    [HttpGet("{logId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportLog")]
    public async Task<ActionResult<ApiObject.ReportLog>> GetReportLogAsync(int tenantId, int logId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, logId);
    }

    /// <summary>
    /// Add a new report log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="log">The report log to add</param>
    /// <returns>The newly created report log</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateReportLog")]
    public async Task<ActionResult<ApiObject.ReportLog>> CreateReportLogAsync(int tenantId,
        ApiObject.ReportLog log)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(tenantId, log);
    }

    /// <summary>
    /// Delete a report log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="logId">The id of the report log</param>
    /// <returns></returns>
    [HttpDelete("{logId}")]
    [ApiOperationId("DeleteReportLog")]
    public async Task<IActionResult> DeleteReportLogAsync(int tenantId, int logId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, logId);
    }
}
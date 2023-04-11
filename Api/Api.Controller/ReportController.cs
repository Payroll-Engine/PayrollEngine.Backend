using System;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the reports
/// </summary>
[ApiControllerName("Reports")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Report)]
public abstract class ReportController : ScriptTrackChildObjectController<IRegulationService, IReportService,
    IRegulationRepository, IReportRepository,
    DomainObject.Tenant, DomainObject.Report, DomainObject.ReportAudit, ApiObject.Report>
{
    private readonly ReportSetMap reportSetMap = new();
    private readonly ReportRequestMap reportRequestMap = new();
    private readonly ReportResponseMap reportResponseMap = new();

    protected ITenantService TenantService { get; }
    protected IReportSetService ReportSetService { get; }

    protected ReportController(ITenantService tenantService, IRegulationService regulationService, IReportService reportService,
        IReportSetService reportSetService, IControllerRuntime runtime) :
        base(regulationService, reportService, runtime, new ReportMap())
    {
        TenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        ReportSetService = reportSetService ?? throw new ArgumentNullException(nameof(reportSetService));
    }

    public virtual async Task<ActionResult<ApiObject.ReportSet>> GetReportSetAsync(int tenantId, int regulationId,
        int reportId, ApiObject.ReportRequest reportRequest)
    {
        try
        {
            // tenant
            var tenant = await TenantService.GetAsync(tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // report
            var report = await ReportSetService.GetAsync(regulationId, reportId);
            if (report == null)
            {
                return BadRequest($"Unknown report with id {reportId}");
            }

            // query
            var domainResponse = await ReportSetService.GetReportAsync(tenant, report,
                new ApiControllerContext(ControllerContext),
                reportRequestMap.ToDomain(reportRequest));
            var apiResponse = reportSetMap.ToApi(domainResponse);
            return apiResponse;
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    public virtual async Task<ActionResult<ApiObject.ReportResponse>> ExecuteReportAsync(int tenantId, int regulationId, int reportId,
        ApiObject.ReportRequest request)
    {
        try
        {
            // tenant
            var tenant = await TenantService.GetAsync(tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // report
            var report = await ReportSetService.GetAsync(regulationId, reportId);
            if (report == null)
            {
                return BadRequest($"Unknown report with id {reportId}");
            }

            // query
            var domainResponse = await ReportSetService.ExecuteReportAsync(tenant, report,
                new ApiControllerContext(ControllerContext), reportRequestMap.ToDomain(request));
            var apiResponse = reportResponseMap.ToApi(domainResponse);
            return apiResponse;
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.ReportSet>> CreateReportSetAsync(int regulationId, ApiObject.ReportSet report)
    {
        var domainReport = reportSetMap.ToDomain(report);
        var result = await ReportSetService.CreateAsync(regulationId, domainReport);
        return reportSetMap.ToApi(result);
    }

    protected virtual async Task<IActionResult> DeleteReportSetAsync(int regulationId, int reportId)
    {
        await ReportSetService.DeleteAsync(regulationId, reportId);
        return Ok();
    }

}
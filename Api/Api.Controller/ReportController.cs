using System;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the reports
/// </summary>
public abstract class ReportController(ITenantService tenantService, IRegulationService regulationService,
        IReportService reportService,
        IReportSetService reportSetService, IControllerRuntime runtime)
    : ScriptTrackChildObjectController<IRegulationService, IReportService,
    IRegulationRepository, IReportRepository,
    Tenant, Report, ReportAudit, ApiObject.Report>(regulationService, reportService, runtime, new ReportMap())
{
    private readonly ReportSetMap reportSetMap = new();
    private readonly ReportRequestMap reportRequestMap = new();
    private readonly ReportResponseMap reportResponseMap = new();

    private ITenantService TenantService { get; } = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private IReportSetService ReportSetService { get; } = reportSetService ?? throw new ArgumentNullException(nameof(reportSetService));

    public virtual async Task<ActionResult<ApiObject.ReportSet>> GetReportSetAsync(
        int tenantId, int regulationId, int reportId, ApiObject.ReportRequest reportRequest = null)
    {
        try
        {
            // tenant
            var tenant = await TenantService.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // report
            var report = await ReportSetService.GetAsync(Runtime.DbContext, regulationId, reportId);
            if (report == null)
            {
                return BadRequest($"Unknown report with id {reportId}");
            }

            // query (request is optional)
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

    public virtual async Task<ActionResult<ApiObject.ReportResponse>> ExecuteReportAsync(
        int tenantId, int regulationId, int reportId, ApiObject.ReportRequest request)
    {
        try
        {
            // tenant
            var tenant = await TenantService.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // report
            var report = await ReportSetService.GetAsync(Runtime.DbContext, regulationId, reportId);
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

    protected async Task<ActionResult<ApiObject.ReportSet>> CreateReportSetAsync(
        int regulationId, ApiObject.ReportSet report)
    {
        var domainReport = reportSetMap.ToDomain(report);
        var result = await ReportSetService.CreateAsync(Runtime.DbContext, regulationId, domainReport);
        return reportSetMap.ToApi(result);
    }

    protected async Task<IActionResult> DeleteReportSetAsync(int regulationId, int reportId)
    {
        await ReportSetService.DeleteAsync(Runtime.DbContext, regulationId, reportId);
        return Ok();
    }

}
using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public class ReportSetService(IReportSetRepository repository, IQueryService queryService, ReportToolSettings settings)
    : ChildApplicationService<IReportSetRepository, ReportSet>(repository), IReportSetService
{
    private IQueryService QueryService { get; } = queryService ?? throw new ArgumentNullException(nameof(queryService));
    private ReportToolSettings Settings { get; } = settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task<ReportSet> GetReportAsync(Tenant tenant, ReportSet report, IApiControllerContext controllerContext,
        ReportRequest reportRequest = null)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }
        if (controllerContext == null)
        {
            throw new ArgumentNullException(nameof(controllerContext));
        }
        if (reportRequest == null)
        {
            throw new ArgumentNullException(nameof(reportRequest));
        }

        // build report
        var builder = new ReportBuilder(tenant, QueryService, Settings);
        var buildReport = await builder.BuildAsync(report, controllerContext, reportRequest);
        return buildReport;
    }

    public async Task<ReportResponse> ExecuteReportAsync(Tenant tenant, ReportSet report,
        IApiControllerContext controllerContext, ReportRequest reportRequest)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }
        if (report == null)
        {
            throw new ArgumentNullException(nameof(report));
        }
        if (controllerContext == null)
        {
            throw new ArgumentNullException(nameof(controllerContext));
        }
        if (reportRequest == null)
        {
            throw new ArgumentNullException(nameof(reportRequest));
        }

        // execute report
        var executor = new ReportProcessor(tenant, QueryService, Settings);
        var response = await executor.ExecuteAsync(report, controllerContext, reportRequest);
        return response;
    }

}
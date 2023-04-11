using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Domain.Application;

public class ReportBuilder : ReportTool
{
    // query
    public IQueryService QueryService { get; }

    public ReportBuilder(Tenant tenant, IQueryService queryService, ReportToolSettings settings) :
        base(tenant, settings)
    {
        QueryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    public virtual async Task<ReportSet> BuildAsync(ReportSet report, IApiControllerContext controllerContext, ReportRequest reportRequest)
    {
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

        if (reportRequest.UserId > 0)
        {
            // user
            var user = await GetUserAsync(reportRequest.UserId);

            // setup report, validate request
            await SetupReport(report, reportRequest);

            // report build script
            if (!string.IsNullOrWhiteSpace(report.BuildExpression))
            {
                if (ReportBuildScript(user, report, reportRequest, controllerContext) == false)
                {
                    return null;
                }
            }
        }

        // report response
        return report;
    }

    private bool? ReportBuildScript(User user, ReportSet report, ReportRequest request, IApiControllerContext controllerContext)
    {
        return new ReportScriptController<ReportSet>().Build(new()
        {
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = user,
            Report = report,
            ReportRequest = request,
            QueryService = QueryService,
            GlobalCaseValueRepository = Settings.GlobalCaseValueRepository,
            NationalCaseValueRepository = Settings.NationalCaseValueRepository,
            CompanyCaseValueRepository = Settings.CompanyCaseValueRepository,
            EmployeCaseValueRepository = Settings.EmployeCaseValueRepository,
            LookupRepository = Settings.LookupRepository,
            LookupValueRepository = Settings.LookupValueRepository,
            CollectorRepository = Settings.CollectorRepository,
            WageTypeRepository = Settings.WageTypeRepository,
            ReportLogRepository = Settings.ReportLogRepository,
            PayrollResultRepository = Settings.PayrollResultRepository,
            WageTypeResultRepository = Settings.WageTypeResultRepository,
            WageTypeCustomResultRepository = Settings.WageTypeCustomResultRepository,
            CollectorResultRepository = Settings.CollectorResultRepository,
            CollectorCustomResultRepository = Settings.CollectorCustomResultRepository,
            PayrunResultRepository = Settings.PayrunResultRepository,
            WebhookDispatchService = WebhookDispatchService,
            ControllerContext = controllerContext
        });
    }
}
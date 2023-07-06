using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Process a report
/// </summary>
public class ReportProcessor : ReportTool
{
    // query
    private IQueryService QueryService { get; }

    public ReportProcessor(Tenant tenant, IQueryService queryService, ReportToolSettings settings) :
        base(tenant, settings)
    {
        QueryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    public async Task<ReportResponse> ExecuteAsync(ReportSet report, IApiControllerContext controllerContext, ReportRequest reportRequest)
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

        // user
        var user = await GetUserAsync(Settings.DbContext, reportRequest.UserId);

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

        // report start script
        if (!string.IsNullOrWhiteSpace(report.StartExpression))
        {
            ReportStartScript(user, report, reportRequest, controllerContext);
        }

        // execute queries to data set
        var dataSet = await ExecuteQueries(report, reportRequest, controllerContext);

        // attributes
        if (report.AttributeMode == ReportAttributeMode.Table)
        {
            SetupAttributes(dataSet);
        }

        // relations
        SetupRelations(report, dataSet);

        // report end script
        if (!string.IsNullOrWhiteSpace(report.EndExpression))
        {
            ReportEndScript(user, report, reportRequest, controllerContext, dataSet);
        }

        // convert system data set to payroll data set
        var payrollDataSet = Data.DataSetExtensions.ToPayrollDataSet(dataSet);

        // report response
        var response = new ReportResponse
        {
            Culture = reportRequest.Culture,
            User = user.Identifier,
            Queries = report.Queries,
            Relations = report.Relations,
            Parameters = reportRequest.Parameters,
            ReportName = report.Name,
            Result = payrollDataSet
        };
        return response;
    }

    private static void SetupRelations(ReportSet report, DataSet dataSet)
    {
        if (dataSet.Tables.Count > 0 && report.Relations != null && report.Relations.Any())
        {
            foreach (var relation in report.Relations)
            {
                var parentTable = dataSet.Tables[relation.ParentTable];
                if (parentTable == null)
                {
                    throw new QueryException($"Missing relation parent table {relation.ParentTable}");
                }

                var parentColumn = parentTable.Columns[relation.ParentColumn];
                if (parentColumn == null)
                {
                    throw new QueryException(
                        $"Missing relation parent column {relation.ParentTable}.{relation.ParentColumn}");
                }

                var childTable = dataSet.Tables[relation.ChildTable];
                if (childTable == null)
                {
                    throw new QueryException($"Missing relation child table {relation.ChildTable}");
                }

                var childColumn = childTable.Columns[relation.ChildColumn];
                if (childColumn == null)
                {
                    throw new QueryException(
                        $"Missing relation child column {relation.ChildTable}.{relation.ChildColumn}");
                }

                dataSet.Relations.Add(relation.Name, parentColumn, childColumn);
            }
        }
    }

    private async Task<DataSet> ExecuteQueries(ReportSet report, ReportRequest request, IApiControllerContext controllerContext)
    {
        var dataSet = new DataSet(report.Name)
        {
            Locale = CultureInfo.InvariantCulture
        };

        // execute queries
        if (report.Queries != null && report.Queries.Any())
        {
            foreach (var query in report.Queries)
            {
                if (!QueryService.ExistsQuery(query.Value))
                {
                    throw new QueryException($"Unknown report query {query.Value}");
                }

                var reportParameters = report.Parameters?.ToDictionary(x => x.Name, y => y.Value);
                var resultTable = await QueryService.ExecuteQueryAsync(Tenant.Id, query.Key, query.Value, request.Culture,
                    reportParameters, request.Parameters, controllerContext);
                // store result table to data set
                if (resultTable != null)
                {
                    dataSet.Tables.Add(resultTable);
                }
            }
        }

        return dataSet;
    }

    private bool? ReportBuildScript(User user, ReportSet report, ReportRequest request, IApiControllerContext controllerContext)
    {
        return new ReportScriptController<ReportSet>().Build(GetRuntimeSettings(user, report, request, controllerContext));
    }

    private void ReportStartScript(User user, ReportSet report, ReportRequest request, IApiControllerContext controllerContext)
    {
        new ReportScriptController<ReportSet>().Start(GetRuntimeSettings(user, report, request, controllerContext));
    }

    private void ReportEndScript(User user, ReportSet report, ReportRequest request,
        IApiControllerContext controllerContext, DataSet dataSet)
    {
        new ReportScriptController<ReportSet>().End(
            GetRuntimeSettings(user, report, request, controllerContext), dataSet);
    }

    private ReportRuntimeSettings GetRuntimeSettings(User user, ReportSet report, ReportRequest request,
        IApiControllerContext controllerContext)
    {
        // [culture by priority]: report-request > user> system</remarks>
        var culture =
            // priority 1: report request culture
            request.Culture ??
            // priority 2: user culture
            user.Culture ??
            // priority 3: system culture
            CultureInfo.CurrentCulture.Name;

        return new()
        {
            DbContext = Settings.DbContext,
            UserCulture = culture,
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
            WageTypeRepository = Settings.WageTypeRepository,
            ReportLogRepository = Settings.ReportLogRepository,
            PayrollResultRepository = Settings.PayrollResultRepository,
            WageTypeResultRepository = Settings.WageTypeResultRepository,
            WageTypeCustomResultRepository = Settings.WageTypeCustomResultRepository,
            CollectorResultRepository = Settings.CollectorResultRepository,
            CollectorCustomResultRepository = Settings.CollectorCustomResultRepository,
            PayrunResultRepository = Settings.PayrunResultRepository,
            WebhookDispatchService = Settings.WebhookDispatchService,
            ControllerContext = controllerContext
        };
    }
}
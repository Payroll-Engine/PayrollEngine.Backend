using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Data;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public class ReportService(IReportRepository repository, IQueryService queryService) :
    ScriptTrackChildApplicationService<IReportRepository, Report, ReportAudit>(repository), IReportService
{
    private IQueryService QueryService { get; } = queryService ?? throw new ArgumentNullException(nameof(queryService));

    public async Task<DataTable> ExecuteQueryAsync(Tenant tenant, string methodName, string culture,
        Dictionary<string, string> parameters, IApiControllerContext controllerContext)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        // report query
        var systemTable = await QueryService.ExecuteQueryAsync(tenant.Id, methodName, culture, parameters, controllerContext);

        // result table
        var payrollTable = systemTable?.ToPayrollDataTable();
        return payrollTable;
    }

}
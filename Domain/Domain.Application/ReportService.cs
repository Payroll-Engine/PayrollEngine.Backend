using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Data;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public class ReportService : ScriptTrackChildApplicationService<IReportRepository, Report, ReportAudit>, IReportService
{
    public IQueryService QueryService { get; }

    public ReportService(IReportRepository repository, IQueryService queryService) :
        base(repository)
    {
        QueryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    public virtual async Task<DataTable> ExecuteQueryAsync(Tenant tenant, string methodName, string culture,
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
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IReportService : IScriptTrackChildApplicationService<IReportRepository, Report, ReportAudit>
{
    Task<DataTable> ExecuteQueryAsync(Tenant tenant, string methodName, string culture,
        Dictionary<string, string> parameters, IApiControllerContext controllerContext);
}
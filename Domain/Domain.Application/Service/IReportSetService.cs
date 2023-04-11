using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IReportSetService : IChildApplicationService<IReportSetRepository, ReportSet>
{
    Task<ReportSet> GetReportAsync(Tenant tenant, ReportSet report, IApiControllerContext controllerContext,
        ReportRequest reportRequest = null);

    Task<ReportResponse> ExecuteReportAsync(Tenant tenant, ReportSet report, IApiControllerContext controllerContext,
        ReportRequest reportRequest);
}
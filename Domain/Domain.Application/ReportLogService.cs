using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportLogService : ChildApplicationService<IReportLogRepository, ReportLog>, IReportLogService
{
    public ReportLogService(IReportLogRepository repository) :
        base(repository)
    {
    }
}
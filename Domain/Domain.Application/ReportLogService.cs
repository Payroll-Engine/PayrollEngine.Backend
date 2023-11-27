using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportLogService
    (IReportLogRepository repository) : ChildApplicationService<IReportLogRepository, ReportLog>(repository),
        IReportLogService;
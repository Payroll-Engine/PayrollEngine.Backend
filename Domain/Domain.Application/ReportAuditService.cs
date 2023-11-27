using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportAuditService
    (IReportAuditRepository repository) : ChildApplicationService<IReportAuditRepository, ReportAudit>(repository),
        IReportAuditService;
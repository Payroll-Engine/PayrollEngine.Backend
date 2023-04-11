using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportAuditService : ChildApplicationService<IReportAuditRepository, ReportAudit>, IReportAuditService
{
    public ReportAuditService(IReportAuditRepository repository) :
        base(repository)
    {
    }
}
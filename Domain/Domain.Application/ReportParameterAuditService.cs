using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportParameterAuditService : ChildApplicationService<IReportParameterAuditRepository, ReportParameterAudit>, IReportParameterAuditService
{
    public ReportParameterAuditService(IReportParameterAuditRepository repository) :
        base(repository)
    {
    }
}
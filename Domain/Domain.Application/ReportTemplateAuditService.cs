using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportTemplateAuditService : ChildApplicationService<IReportTemplateAuditRepository, ReportTemplateAudit>, IReportTemplateAuditService
{
    public ReportTemplateAuditService(IReportTemplateAuditRepository repository) :
        base(repository)
    {
    }
}
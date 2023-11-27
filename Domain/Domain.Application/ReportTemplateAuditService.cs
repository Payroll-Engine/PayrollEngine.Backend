using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportTemplateAuditService(IReportTemplateAuditRepository repository) :
    ChildApplicationService<IReportTemplateAuditRepository, ReportTemplateAudit>(repository),
    IReportTemplateAuditService;
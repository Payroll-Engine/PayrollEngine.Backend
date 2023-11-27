using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportParameterAuditService(IReportParameterAuditRepository repository) :
    ChildApplicationService<IReportParameterAuditRepository, ReportParameterAudit>(repository),
    IReportParameterAuditService;
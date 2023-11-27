using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseAuditService
    (ICaseAuditRepository repository) : ChildApplicationService<ICaseAuditRepository, CaseAudit>(repository),
        ICaseAuditService;
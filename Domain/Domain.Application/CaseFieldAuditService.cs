using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseFieldAuditService(ICaseFieldAuditRepository repository) :
    ChildApplicationService<ICaseFieldAuditRepository, CaseFieldAudit>(repository), ICaseFieldAuditService;
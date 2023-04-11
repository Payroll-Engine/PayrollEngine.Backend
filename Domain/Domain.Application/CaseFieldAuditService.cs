using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseFieldAuditService : ChildApplicationService<ICaseFieldAuditRepository, CaseFieldAudit>, ICaseFieldAuditService
{
    public CaseFieldAuditService(ICaseFieldAuditRepository repository) :
        base(repository)
    {
    }
}
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseAuditService : ChildApplicationService<ICaseAuditRepository, CaseAudit>, ICaseAuditService
{
    public CaseAuditService(ICaseAuditRepository repository) :
        base(repository)
    {
    }
}
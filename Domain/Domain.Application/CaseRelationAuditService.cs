using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseRelationAuditService : ChildApplicationService<ICaseRelationAuditRepository, CaseRelationAudit>, ICaseRelationAuditService
{
    public CaseRelationAuditService(ICaseRelationAuditRepository repository) :
        base(repository)
    {
    }
}
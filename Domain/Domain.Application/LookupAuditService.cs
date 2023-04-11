using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupAuditService : ChildApplicationService<ILookupAuditRepository, LookupAudit>, ILookupAuditService
{
    public LookupAuditService(ILookupAuditRepository repository) :
        base(repository)
    {
    }
}
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupValueAuditService : ChildApplicationService<ILookupValueAuditRepository, LookupValueAudit>, ILookupValueAuditService
{
    public LookupValueAuditService(ILookupValueAuditRepository repository) :
        base(repository)
    {
    }
}
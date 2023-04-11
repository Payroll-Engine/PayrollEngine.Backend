using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CollectorAuditService : ChildApplicationService<ICollectorAuditRepository, CollectorAudit>, ICollectorAuditService
{
    public CollectorAuditService(ICollectorAuditRepository repository) :
        base(repository)
    {
    }
}
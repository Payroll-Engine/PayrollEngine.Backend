using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WageTypeAuditService : ChildApplicationService<IWageTypeAuditRepository, WageTypeAudit>, IWageTypeAuditService
{
    public WageTypeAuditService(IWageTypeAuditRepository repository) :
        base(repository)
    {
    }
}
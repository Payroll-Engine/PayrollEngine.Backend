using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupAuditService
    (ILookupAuditRepository repository) : ChildApplicationService<ILookupAuditRepository, LookupAudit>(repository),
        ILookupAuditService;
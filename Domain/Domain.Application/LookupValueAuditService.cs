using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupValueAuditService(ILookupValueAuditRepository repository) :
    ChildApplicationService<ILookupValueAuditRepository, LookupValueAudit>(repository), ILookupValueAuditService;
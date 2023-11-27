using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CollectorAuditService(ICollectorAuditRepository repository) :
    ChildApplicationService<ICollectorAuditRepository, CollectorAudit>(repository), ICollectorAuditService;
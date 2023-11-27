using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WageTypeAuditService(IWageTypeAuditRepository repository) :
    ChildApplicationService<IWageTypeAuditRepository, WageTypeAudit>(repository), IWageTypeAuditService;
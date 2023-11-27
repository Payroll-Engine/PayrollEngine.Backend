using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ScriptAuditService
    (IScriptAuditRepository repository) : ChildApplicationService<IScriptAuditRepository, ScriptAudit>(repository),
        IScriptAuditService;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ScriptAuditService : ChildApplicationService<IScriptAuditRepository, ScriptAudit>, IScriptAuditService
{
    public ScriptAuditService(IScriptAuditRepository repository) :
        base(repository)
    {
    }
}
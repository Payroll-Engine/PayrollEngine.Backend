using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation script audits
/// </summary>
[ApiControllerName("Script audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/scripts/{scriptId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ScriptAudit)]
public abstract class ScriptAuditController : RepositoryChildObjectController<IScriptService, IScriptAuditService, 
    IScriptRepository, IScriptAuditRepository,
    DomainObject.Script, DomainObject.ScriptAudit, ApiObject.ScriptAudit>
{
    protected ScriptAuditController(IScriptService scriptService, IScriptAuditService scriptAuditService, IControllerRuntime runtime) :
        base(scriptService, scriptAuditService, runtime, new ScriptAuditMap())
    {
    }
}
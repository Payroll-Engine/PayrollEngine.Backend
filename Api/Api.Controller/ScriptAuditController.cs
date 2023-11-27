using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation script audits
/// </summary>
public abstract class ScriptAuditController(IScriptService scriptService, IScriptAuditService scriptAuditService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<IScriptService, IScriptAuditService, 
    IScriptRepository, IScriptAuditRepository,
    DomainObject.Script, DomainObject.ScriptAudit, ApiObject.ScriptAudit>(scriptService, scriptAuditService, runtime, new ScriptAuditMap());
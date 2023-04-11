using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for logs
/// </summary>
[ApiControllerName("Logs")]
[Route("api/tenants/{tenantId}/logs")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Log)]
public abstract class LogController : RepositoryChildObjectController<ITenantService, ILogService,
    ITenantRepository, ILogRepository,
    DomainObject.Tenant, DomainObject.Log, ApiObject.Log>
{
    protected LogController(ITenantService tenantService, ILogService logService, IControllerRuntime runtime) :
        base(tenantService, logService, runtime, new LogMap())
    {
    }
}
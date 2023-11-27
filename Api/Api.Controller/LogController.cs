using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for logs
/// </summary>
public abstract class LogController(ITenantService tenantService, ILogService logService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, ILogService,
    ITenantRepository, ILogRepository,
    DomainObject.Tenant, DomainObject.Log, ApiObject.Log>(tenantService, logService, runtime, new LogMap());
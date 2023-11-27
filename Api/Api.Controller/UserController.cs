using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll users
/// </summary>
public abstract class UserController(ITenantService tenantService, IUserService userService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IUserService,
    ITenantRepository, IUserRepository,
    DomainObject.Tenant, DomainObject.User, ApiObject.User>(tenantService, userService, runtime, new UserMap());
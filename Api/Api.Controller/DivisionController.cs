using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for divisions
/// </summary>
[ApiControllerName("Divisions")]
[Route("api/tenants/{tenantId}/divisions")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Division)]
public abstract class DivisionController : RepositoryChildObjectController<ITenantService, IDivisionService,
    ITenantRepository, IDivisionRepository,
    DomainObject.Tenant, DomainObject.Division, ApiObject.Division>
{
    protected DivisionController(ITenantService tenantService, IDivisionService divisionService, IControllerRuntime runtime) :
        base(tenantService, divisionService, runtime, new DivisionMap())
    {
    }
}
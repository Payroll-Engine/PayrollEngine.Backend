using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll company case changes
/// </summary>
[ApiControllerName("Company case changes")]
[Route("api/tenants/{tenantId}/companycases/changes")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CompanyCaseChange)]
public abstract class CompanyCaseChangeController : CaseChangeController<ITenantService,
    ITenantRepository, ICompanyCaseChangeRepository,
    DomainObject.Tenant, DomainObject.CaseChange, ApiObject.CaseChange>
{
    protected CompanyCaseChangeController(ITenantService tenantService, ICompanyCaseChangeService caseChangeService,
        ICaseFieldService caseFieldService, IDivisionService divisionService, IUserService userService, IControllerRuntime runtime) :
        base(tenantService, caseChangeService, caseFieldService, divisionService, userService, runtime)
    {
    }
}
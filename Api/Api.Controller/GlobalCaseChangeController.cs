﻿using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll global case changes
/// </summary>
[ApiControllerName("Global case changes")]
[Route("api/tenants/{tenantId}/globalcases/changes")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.GlobalCaseChange)]
public abstract class GlobalCaseChangeController : CaseChangeController<ITenantService,
    ITenantRepository, IGlobalCaseChangeRepository,
    DomainObject.Tenant, DomainObject.CaseChange, ApiObject.CaseChange>
{
    protected GlobalCaseChangeController(ITenantService tenantService, IGlobalCaseChangeService caseChangeService,
        ICaseFieldService caseFieldService, IDivisionService divisionService, IUserService userService, IControllerRuntime runtime) :
        base(tenantService, caseChangeService, caseFieldService, divisionService, userService, runtime)
    {
    }
}
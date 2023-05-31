﻿using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll company cases
/// </summary>
public abstract class CompanyCaseValueController : CaseValueController<ITenantService, ITenantRepository, 
    ICompanyCaseValueRepository, DomainObject.Tenant>
{
    protected CompanyCaseValueController(ITenantService tenantService, ICaseValueService<ICompanyCaseValueRepository, 
            DomainObject.CaseValue> caseValueService, IPayrollService payrollsService, IRegulationService regulationService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(tenantService, caseValueService, payrollsService, regulationService, lookupSetService, runtime)
    {
    }
}
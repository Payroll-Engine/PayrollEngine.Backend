using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll company case changes
/// </summary>
public abstract class CompanyCaseChangeController(ITenantService tenantService,
        ICompanyCaseChangeService caseChangeService,
        IControllerRuntime runtime)
    : CaseChangeController<ITenantService,
    ITenantRepository, ICompanyCaseChangeRepository,
    DomainObject.Tenant, DomainObject.CaseChange, ApiObject.CaseChange>(tenantService, caseChangeService, runtime);
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll national cases
/// </summary>
public abstract class NationalCaseValueController(ITenantService tenantService,
        ICaseValueService<INationalCaseValueRepository,
            DomainObject.CaseValue> caseValueService, IPayrollService payrollsService,
        IRegulationService regulationService,
        ILookupSetService lookupSetService, IControllerRuntime runtime)
    : CaseValueController<ITenantService, ITenantRepository, INationalCaseValueRepository, DomainObject.Tenant>(tenantService, caseValueService, payrollsService, regulationService, lookupSetService, runtime);
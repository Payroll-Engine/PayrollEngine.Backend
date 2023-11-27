using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll global cases
/// </summary>
public abstract class GlobalCaseValueController(ITenantService tenantService,
        ICaseValueService<IGlobalCaseValueRepository,
            DomainObject.CaseValue> caseValueService, IPayrollService payrollsService,
        IRegulationService regulationService,
        ILookupSetService lookupSetService, IControllerRuntime runtime)
    : CaseValueController<ITenantService, ITenantRepository, IGlobalCaseValueRepository, DomainObject.Tenant>(tenantService, caseValueService, payrollsService, regulationService, lookupSetService, runtime);
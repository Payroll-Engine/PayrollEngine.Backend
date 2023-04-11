using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll global cases
/// </summary>
[ApiControllerName("Global case values")]
[Route("api/tenants/{tenantId}/globalcases")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.GlobalCaseValue)]
public abstract class GlobalCaseValueController : CaseValueController<ITenantService, ITenantRepository, IGlobalCaseValueRepository, DomainObject.Tenant>
{
    protected GlobalCaseValueController(ITenantService tenantService, ICaseValueService<IGlobalCaseValueRepository, 
            DomainObject.CaseValue> caseValueService, IPayrollService payrollsService, IRegulationService regulationService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(tenantService, caseValueService, payrollsService, regulationService, lookupSetService, runtime)
    {
    }
}
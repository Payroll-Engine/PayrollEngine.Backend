using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll consolidated results
/// </summary>
public abstract class PayrollConsolidatedResultController : RepositoryChildObjectController<ITenantService, IPayrollConsolidatedResultService,
    ITenantRepository, IPayrollConsolidatedResultRepository,
    DomainObject.Tenant, DomainObject.PayrollResult, ApiObject.PayrollResult>
{
    protected PayrollConsolidatedResultController(ITenantService tenantService, IPayrollConsolidatedResultService payrollResultService,
        IControllerRuntime runtime) :
        base(tenantService, payrollResultService, runtime, new PayrollResultMap())
    {
    }
}
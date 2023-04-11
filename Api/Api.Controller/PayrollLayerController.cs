using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll layers
/// </summary>
[ApiControllerName("Payroll layers")]
[Route("api/tenants/{tenantId}/payrolls/{payrollId}/layers")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.PayrollLayer)]
public abstract class PayrollLayerController : RepositoryChildObjectController<IPayrollService, IPayrollLayerService,
    IPayrollRepository, IPayrollLayerRepository,
    DomainObject.Payroll, DomainObject.PayrollLayer, ApiObject.PayrollLayer>
{
    protected PayrollLayerController(IPayrollService payrollService, IPayrollLayerService payrollLayerService, IControllerRuntime runtime) :
        base(payrollService, payrollLayerService, runtime, new PayrollLayerMap())
    {
    }

    protected override async Task<ActionResult<ApiObject.PayrollLayer>> CreateAsync(int regulationId, ApiObject.PayrollLayer payrollLayer)
    {
        // unique payroll layer level and priority per payroll
        if (await ChildService.ExistsAsync(regulationId, payrollLayer.Level, payrollLayer.Priority))
        {
            return BadRequest($"Payroll layer with level {payrollLayer.Level} and priority {payrollLayer.Priority} already exists");
        }

        return await base.CreateAsync(regulationId, payrollLayer);
    }
}
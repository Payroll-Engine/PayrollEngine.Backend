using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll layers
/// </summary>
public abstract class PayrollLayerController : RepositoryChildObjectController<IPayrollService, IPayrollLayerService,
    IPayrollRepository, IPayrollLayerRepository,
    Payroll, PayrollLayer, ApiObject.PayrollLayer>
{
    protected PayrollLayerController(IPayrollService payrollService, IPayrollLayerService payrollLayerService, IControllerRuntime runtime) :
        base(payrollService, payrollLayerService, runtime, new PayrollLayerMap())
    {
    }

    protected override async Task<ActionResult<ApiObject.PayrollLayer>> CreateAsync(int regulationId, ApiObject.PayrollLayer payrollLayer)
    {
        // unique payroll layer level and priority per payroll
        if (await ChildService.ExistsAsync(Runtime.DbContext, regulationId, payrollLayer.Level, payrollLayer.Priority))
        {
            return BadRequest($"Payroll layer with level {payrollLayer.Level} and priority {payrollLayer.Priority} already exists");
        }

        return await base.CreateAsync(regulationId, payrollLayer);
    }
}
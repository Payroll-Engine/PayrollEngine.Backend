using System;
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
/// API controller for the payruns
/// </summary>
public abstract class PayrunController : RepositoryChildObjectController<ITenantService, IPayrunService,
    ITenantRepository, IPayrunRepository,
    DomainObject.Tenant, DomainObject.Payrun, ApiObject.Payrun>
{
    private IPayrollService PayrollService { get; }

    protected PayrunController(ITenantService tenantService, IPayrunService payrunService,
        IPayrollService payrollService, IControllerRuntime runtime) :
        base(tenantService, payrunService, runtime, new PayrunMap())
    {
        PayrollService = payrollService ?? throw new ArgumentNullException(nameof(payrollService));
    }

    protected override async Task<ActionResult<ApiObject.Payrun>> CreateAsync(int regulationId, ApiObject.Payrun payrun)
    {
        // payroll
        if (payrun.PayrollId <= 0 || !await PayrollService.ExistsAsync(Runtime.DbContext, payrun.PayrollId))
        {
            return BadRequest($"Unknown payroll with id {payrun.PayrollId}");
        }

        return await base.CreateAsync(regulationId, payrun);
    }

    // duplicated in ScriptTrackChildObjectController!
    protected async Task<ActionResult> RebuildAsync(int tenantId, int payrunId)
    {
        if (tenantId <= 0)
        {
            return InvalidParentRequest(tenantId);
        }

        // test tenant
        if (!await ParentService.ExistsAsync(Runtime.DbContext, tenantId))
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // test payrun
        if (!await ChildService.ExistsAsync(Runtime.DbContext, payrunId))
        {
            return BadRequest($"Unknown payrun with id {payrunId}");
        }

        await Service.RebuildAsync(Runtime.DbContext, tenantId, payrunId);
        return Ok();
    }

}
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
[ApiControllerName("Payruns")]
[Route("api/tenants/{tenantId}/payruns")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Payrun)]
public abstract class PayrunController : RepositoryChildObjectController<ITenantService, IPayrunService,
    ITenantRepository, IPayrunRepository,
    DomainObject.Tenant, DomainObject.Payrun, ApiObject.Payrun>
{
    protected IPayrollService PayrollService { get; }

    protected PayrunController(ITenantService tenantService, IPayrunService payrunService,
        IPayrollService payrollService, IControllerRuntime runtime) :
        base(tenantService, payrunService, runtime, new PayrunMap())
    {
        PayrollService = payrollService ?? throw new ArgumentNullException(nameof(payrollService));
    }

    protected override async Task<ActionResult<ApiObject.Payrun>> CreateAsync(int regulationId, ApiObject.Payrun payrun)
    {
        // payroll
        if (payrun.PayrollId <= 0 || !await PayrollService.ExistsAsync(payrun.PayrollId))
        {
            return BadRequest($"Unknown payroll with id {payrun.PayrollId}");
        }

        return await base.CreateAsync(regulationId, payrun);
    }

    // duplicated in ScriptTrackChildObjectController!
    protected virtual async Task<ActionResult> RebuildAsync(int tenantId, int payrunId)
    {
        if (tenantId <= 0)
        {
            return InvalidParentRequest(tenantId);
        }

        // test item
        if (!await ChildService.ExistsAsync(payrunId))
        {
            return BadRequest($"Unknown payrun with id {payrunId}");
        }

        await Service.RebuildAsync(tenantId, payrunId);
        return Ok();
    }

}
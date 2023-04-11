using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class WageTypeAuditController : Api.Controller.WageTypeAuditController
{
    /// <inheritdoc/>
    public WageTypeAuditController(IWageTypeService wageTypeService,
        IWageTypeAuditService auditService, IControllerRuntime runtime) :
        base(wageTypeService, auditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation wage type audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="wageTypeId">The id of the wage type</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryWageTypeAudits")]
    public async Task<ActionResult> QueryWageTypeAuditsAsync(int tenantId, int wageTypeId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(wageTypeId, query);
    }

    /// <summary>
    /// Get a regulation wage type audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="wageTypeId">The wage type id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetWageTypeAudit")]
    public async Task<ActionResult<ApiObject.WageTypeAudit>> GetWageTypeAuditAsync(int tenantId, int wageTypeId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(wageTypeId, auditId);
    }
}
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Script audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/scripts/{scriptId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ScriptAudit)]
public class ScriptAuditController : Api.Controller.ScriptAuditController
{
    /// <inheritdoc/>
    public ScriptAuditController(IScriptService scriptService, IScriptAuditService auditService,
        IControllerRuntime runtime) :
        base(scriptService, auditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation script audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="scriptId">The script id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryScriptAudits")]
    public async Task<ActionResult> QueryScriptAuditsAsync(int tenantId,
        int regulationId, int scriptId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(scriptId, query);
    }

    /// <summary>
    /// Get a regulation script audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="scriptId">The script id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetScriptAudit")]
    public async Task<ActionResult<ApiObject.ScriptAudit>> GetScriptAuditAsync(
        int tenantId, int regulationId, int scriptId, int auditId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(scriptId, auditId);
    }
}
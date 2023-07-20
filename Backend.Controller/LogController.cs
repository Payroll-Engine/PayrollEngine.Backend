using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Logs")]
[Route("api/tenants/{tenantId}/logs")]
public class LogController : Api.Controller.LogController
{
    /// <inheritdoc/>
    public LogController(ITenantService tenantService, ILogService logService, IControllerRuntime runtime) :
        base(tenantService, logService, runtime)
    {
    }

    /// <summary>
    /// Query logs
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant logs</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryLogs")]
    public async Task<ActionResult> QueryLogsAsync(int tenantId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="logId">The id of the log</param>
    /// <returns>The tenant log</returns>
    [HttpGet("{logId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetLog")]
    public async Task<ActionResult<ApiObject.Log>> GetLogAsync(int tenantId, int logId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, logId);
    }

    /// <summary>
    /// Add a new log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="log">The log to add</param>
    /// <returns>The newly created log</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateLog")]
    public async Task<ActionResult<ApiObject.Log>> CreateLogAsync(int tenantId, ApiObject.Log log)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(tenantId, log);
    }

    /// <summary>
    /// Delete a log
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="logId">The id of the log</param>
    /// <returns></returns>
    [HttpDelete("{logId}")]
    [ApiOperationId("DeleteLog")]
    public async Task<IActionResult> DeleteLogAsync(int tenantId, int logId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, logId);
    }
}
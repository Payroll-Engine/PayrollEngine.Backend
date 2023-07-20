using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Scripts")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/scripts")]
public class ScriptController : Api.Controller.ScriptController
{
    /// <inheritdoc/>
    public ScriptController(IRegulationService regulationService, IScriptService scriptService, 
        IControllerRuntime runtime) :
        base(regulationService, scriptService, runtime)
    {
    }

    /// <summary>
    /// Query regulation scripts
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation scripts</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryScripts")]
    public async Task<ActionResult> QueryScriptsAsync(int tenantId, int regulationId, 
        [FromQuery] Query query)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a regulation script
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="scriptId">The script id</param>
    /// <returns>The regulation script</returns>
    [HttpGet("{scriptId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetScript")]
    public async Task<ActionResult<ApiObject.Script>> GetScriptAsync(int tenantId,
        int regulationId, int scriptId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(regulationId, scriptId);
    }

    /// <summary>
    /// Add a new regulation script
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="script">The script to add</param>
    /// <returns>The newly created regulation script</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateScript")]
    public async Task<ActionResult<ApiObject.Script>> CreateScriptAsync(int tenantId,
        int regulationId, ApiObject.Script script)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(regulationId, script);
    }

    /// <summary>
    /// Update a regulation script
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="script">The script with updated values</param>
    /// <returns>The modified regulation script</returns>
    [HttpPut("{scriptId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateScript")]
    public async Task<ActionResult<ApiObject.Script>> UpdateScriptAsync(int tenantId,
        int regulationId, ApiObject.Script script)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(regulationId, script);
    }

    /// <summary>
    /// Delete a regulation script
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="scriptId">The script id</param>
    [HttpDelete("{scriptId}")]
    [ApiOperationId("DeleteScript")]
    public async Task<IActionResult> DeleteScriptAsync(int tenantId, 
        int regulationId, int scriptId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(regulationId, scriptId);
    }
}
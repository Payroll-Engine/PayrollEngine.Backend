using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Data;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Tenants")]
[Route("api/tenants")]
public class TenantController : Api.Controller.TenantController
{
    /// <inheritdoc/>
    public TenantController(ITenantService tenantService,
        IRegulationService regulationService, IRegulationShareService regulationShareService, IReportService reportService,
        IControllerRuntime runtime) :
        base(tenantService, regulationService, regulationShareService, reportService, runtime)
    {
    }

    /// <summary>
    /// Query tenants
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenants matching the query</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryTenants")]
    public async Task<ActionResult> QueryTenantsAsync([FromQuery] Query query) =>
        await QueryItemsAsync(query);

    /// <summary>
    /// Add a new tenant
    /// </summary>
    /// <remarks>No authorization check</remarks>
    /// <param name="tenant">The tenant to add</param>
    /// <returns>The newly created tenant</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateTenant")]
    public async Task<ActionResult<ApiObject.Tenant>> CreateTenantAsync(ApiObject.Tenant tenant)
    {
        // unique tenant by identifier
        if (await Service.ExistsAsync(Runtime.DbContext, tenant.Identifier))
        {
            return BadRequest($"Tenant with identifier {tenant.Identifier} already exists");
        }
        return await CreateAsync(tenant);
    }

    /// <summary>
    /// Update a tenant
    /// </summary>
    /// <remarks>No authorization check</remarks>
    /// <param name="tenant">The tenant with updated values</param>
    /// <returns>The modified tenant</returns>
    [HttpPut("{tenantId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateTenant")]
    public async Task<ActionResult<ApiObject.Tenant>> UpdateTenantAsync(ApiObject.Tenant tenant) =>
        await UpdateAsync(tenant);

    /// <summary>
    /// Delete a tenant including all tenant data
    /// </summary>
    /// <remarks>No authorization check</remarks>
    /// <param name="tenantId">The tenant id</param>
    [HttpDelete("{tenantId}")]
    [ApiOperationId("DeleteTenant")]
    public async Task<IActionResult> DeleteTenantAsync(int tenantId) =>
        await DeleteAsync(tenantId);

    /// <summary>
    /// Get a tenant
    /// </summary>
    /// <remarks>No authorization check</remarks>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("{tenantId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetTenant")]
    public async Task<ActionResult<ApiObject.Tenant>> GetTenantAsync(int tenantId) =>
        await GetAsync(tenantId);

    /// <summary>
    /// Get tenant shared regulations
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionId">The division id</param>
    /// <returns>The tenant shared regulations</returns>
    [HttpGet("{tenantId}/shared/regulations")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetTenantSharedRegulations")]
    public override async Task<ActionResult<IEnumerable<ApiObject.Regulation>>> GetSharedRegulationsAsync(
        int tenantId, [FromQuery] int? divisionId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetSharedRegulationsAsync(tenantId, divisionId);
    }

    /// <summary>
    /// Get the system script actions
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="functionType">The function type</param>
    /// <returns>List of system actions</returns>
    [HttpGet("{tenantId}/system/actions")]
    [OkResponse]
    [ApiOperationId("GetSystemScriptActions")]
    [QueryIgnore]
    public override async Task<ActionResult<IEnumerable<ApiObject.ActionInfo>>> GetSystemScriptActionsAsync(
        int tenantId, FunctionType functionType = FunctionType.All)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetSystemScriptActionsAsync(tenantId, functionType);
    }

    /// <summary>
    /// Get the system script action properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="functionType">The function type</param>
    /// <param name="readOnly">Read-only properties (default: true)</param>
    /// <returns>List of system actions</returns>
    [HttpGet("{tenantId}/system/properties")]
    [OkResponse]
    [ApiOperationId("GetSystemScriptActionProperties")]
    [QueryIgnore]
    public override async Task<ActionResult<IEnumerable<ApiObject.ActionInfo>>> GetSystemScriptActionPropertiesAsync(
        int tenantId, FunctionType functionType = FunctionType.All, bool readOnly = true)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetSystemScriptActionPropertiesAsync(tenantId, functionType, readOnly);
    }

    /// <summary>
    /// Execute a report query
    /// </summary>
    /// <remarks>
    /// Request body contains array of case values (optional)
    /// Without the request body, this would be a GET method
    /// </remarks>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="methodName">The query method</param>
    /// <param name="culture">The data culture</param>
    /// <param name="parameters">The query parameters</param>
    /// <returns>The resulting data table</returns>
    [HttpPost("{tenantId}/queries")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("ExecuteReportQuery")]
    [QueryIgnore]
    public override async Task<ActionResult<DataTable>> ExecuteReportQueryAsync(int tenantId,
        [FromQuery] string methodName, [FromQuery] string culture,
        [FromBody] Dictionary<string, string> parameters = null)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.ExecuteReportQueryAsync(tenantId, methodName, culture, parameters);
    }

    #region Attributes

    /// <summary>
    /// Get a tenant attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{tenantId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetTenantAttribute")]
    public virtual async Task<ActionResult<string>> GetTenantAttributeAsync(
        int tenantId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAttributeAsync(tenantId, attributeName);
    }

    /// <summary>
    /// Set a tenant attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{tenantId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetTenantAttribute")]
    public virtual async Task<ActionResult<string>> SetTenantAttributeAsync(
        int tenantId, string attributeName, [FromBody] string value)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await SetAttributeAsync(tenantId, attributeName, value);
    }

    /// <summary>
    /// Delete a tenant attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{tenantId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteTenantAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteTenantAttributeAsync(
        int tenantId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAttributeAsync(tenantId, attributeName);
    }

    #endregion

}
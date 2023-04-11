using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class SharedRegulationController : Api.Controller.SharedRegulationController
{
    /// <summary>
    /// The regulation service
    /// </summary>
    protected IRegulationService RegulationService { get; }
    /// <summary>
    /// The division service
    /// </summary>
    protected IDivisionService DivisionService { get; }

    /// <inheritdoc/>
    public SharedRegulationController(IRegulationPermissionService permissionService,
        IRegulationService regulationService, IDivisionService divisionService, IControllerRuntime runtime) :
        base(permissionService, runtime)
    {
        RegulationService = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
        DivisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
    }

    
    /// <summary>
    /// Get a regulation permission
    /// </summary>
    /// <param name="permissionId">The regulation permission id</param>
    /// <returns></returns>
    [HttpGet("{permissionId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetRegulationPermission")]
    public async Task<ActionResult<ApiObject.RegulationPermission>> GetRegulationPermissionAsync(int permissionId)
    {
        return await GetAsync(permissionId);
    }

    #region Attributes

    /// <summary>
    /// Get a regulation permission attribute
    /// </summary>
    /// <param name="permissionId">The regulation permission id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{permissionId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetRegulationPermissionAttribute")]
    public virtual async Task<ActionResult<string>> GetRegulationPermissionAttributeAsync(int permissionId, string attributeName)
    {
        return await GetAttributeAsync(permissionId, attributeName);
    }

    /// <summary>
    /// Set a regulation permission attribute
    /// </summary>
    /// <param name="permissionId">The regulation permission id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{permissionId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetRegulationPermissionAttribute")]
    public virtual async Task<ActionResult<string>> SetRegulationPermissionAttributeAsync(int permissionId, string attributeName,
        [FromBody] string value)
    {
        return await base.SetAttributeAsync(permissionId, attributeName, value);
    }

    /// <summary>
    /// Delete a regulation permission attribute
    /// </summary>
    /// <param name="permissionId">The regulation permission id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{permissionId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteRegulationPermissionAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteRegulationPermissionAttributeAsync(int permissionId, string attributeName)
    {
        return await base.DeleteAttributeAsync(permissionId, attributeName);
    }

    #endregion

    /// <summary>
    /// Query regulation permissions
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation permissions matching the query</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryRegulationPermissions")]
    public async Task<ActionResult> QueryRegulationPermissionsAsync([FromQuery] Query query) =>
        await QueryItemsAsync(query);

    /// <summary>
    /// Add a new regulation permission
    /// </summary>
    /// <param name="permission">The regulation permission to add</param>
    /// <returns>The newly created regulation permission</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateRegulationPermission")]
    public async Task<ActionResult<ApiObject.RegulationPermission>> CreateRegulationPermissionAsync(ApiObject.RegulationPermission permission)
    {
        // validate tenant
        var tenantId = await RegulationService.GetParentIdAsync(permission.RegulationId);
        if (!tenantId.HasValue || tenantId.Value != permission.TenantId)
        {
            return BadRequest($"Invalid permission regulation id {permission.RegulationId}");
        }
        // validate regulation
        var regulation = await RegulationService.GetAsync(tenantId.Value, permission.RegulationId);
        if (regulation == null)
        {
            return BadRequest($"Unknown regulation id {permission.RegulationId} on tenant id {permission.TenantId}");
        }
        if (!regulation.SharedRegulation)
        {
            return BadRequest($"Regulation {regulation.Name} is not shared");
        }
        // validate permission tenant
        if (permission.TenantId == permission.PermissionTenantId)
        {
            return BadRequest($"Invalid self referencing permission on tenant id {permission.TenantId}");
        }
        // validate permission division
        if (permission.PermissionDivisionId.HasValue)
        {
            tenantId = await DivisionService.GetParentIdAsync(permission.PermissionDivisionId.Value);
            if (!tenantId.HasValue || tenantId.Value != permission.PermissionTenantId)
            {
                return BadRequest($"Invalid permission division id {permission.PermissionDivisionId}");
            }
        }
        return await CreateAsync(permission);
    }

    /// <summary>
    /// Delete a regulation permission
    /// </summary>
    /// <param name="permissionId">The regulation permission id</param>
    /// <returns></returns>
    [HttpDelete("{permissionId}")]
    [ApiOperationId("DeleteRegulationPermission")]
    public async Task<IActionResult> DeleteRegulationPermissionAsync(int permissionId) =>
        await DeleteAsync(permissionId);
}
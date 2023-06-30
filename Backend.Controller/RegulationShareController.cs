using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Regulation shares")]
[Route("api/shares/regulations")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.RegulationShare)]
public class RegulationShareController : Api.Controller.RegulationShareController
{
    /// <summary>
    /// The regulation service
    /// </summary>
    private IRegulationService RegulationService { get; }
    /// <summary>
    /// The division service
    /// </summary>
    private IDivisionService DivisionService { get; }

    /// <inheritdoc/>
    public RegulationShareController(IRegulationShareService shareService,
        IRegulationService regulationService, IDivisionService divisionService, IControllerRuntime runtime) :
        base(shareService, runtime)
    {
        RegulationService = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
        DivisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
    }

    /// <summary>
    /// Get a regulation share
    /// </summary>
    /// <param name="shareId">The regulation share id</param>
    /// <returns></returns>
    [HttpGet("{shareId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetRegulationShare")]
    public async Task<ActionResult<ApiObject.RegulationShare>> GetRegulationShareAsync(int shareId) =>
        await GetAsync(shareId);

    /// <summary>
    /// Query regulation shares
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation shares matching the query</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryRegulationShares")]
    public async Task<ActionResult> QueryRegulationSharesAsync([FromQuery] Query query) =>
        await QueryItemsAsync(query);

    /// <summary>
    /// Add a new regulation share
    /// </summary>
    /// <param name="share">The regulation share to add</param>
    /// <returns>The newly created regulation share</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateRegulationShare")]
    public async Task<ActionResult<ApiObject.RegulationShare>> CreateRegulationShareAsync(
        ApiObject.RegulationShare share)
    {
        // validate tenant
        var tenantId = await RegulationService.GetParentIdAsync(Runtime.DbContext, share.ProviderRegulationId);
        if (!tenantId.HasValue || tenantId.Value != share.ProviderTenantId)
        {
            return BadRequest($"Regulation share: invalid regulation {share.ProviderRegulationId} for provider {share.ProviderTenantId}");
        }

        // validate regulation
        var regulation = await RegulationService.GetAsync(Runtime.DbContext, tenantId.Value, share.ProviderRegulationId);
        if (regulation == null)
        {
            return BadRequest($"Regulation share: unknown provider regulation {share.ProviderRegulationId} on tenant {share.ProviderTenantId}");
        }
        if (!regulation.SharedRegulation)
        {
            return BadRequest($"Regulation share: regulation {regulation.Name} is not shared");
        }

        // validate tenants
        if (share.ProviderTenantId == share.ConsumerTenantId)
        {
            return BadRequest($"Regulation share: invalid self referencing tenant {share.ProviderTenantId}");
        }

        // validate consumer division
        if (share.ConsumerDivisionId.HasValue)
        {
            tenantId = await DivisionService.GetParentIdAsync(Runtime.DbContext, share.ConsumerDivisionId.Value);
            if (!tenantId.HasValue || tenantId.Value != share.ConsumerTenantId)
            {
                return BadRequest($"Regulation share: invalid consumer division {share.ConsumerDivisionId}");
            }
        }
        return await CreateAsync(share);
    }

    /// <summary>
    /// Update a regulation share
    /// </summary>
    /// <param name="share">The regulation share with updated values</param>
    /// <returns>The modified regulation share</returns>
    [HttpPut("{shareId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateRegulationShare")]
    public async Task<ActionResult<ApiObject.RegulationShare>> UpdateRegulationShareAsync(
        ApiObject.RegulationShare share) =>
        await UpdateAsync(share);

    /// <summary>
    /// Delete a regulation share
    /// </summary>
    /// <param name="shareId">The regulation share id</param>
    /// <returns></returns>
    [HttpDelete("{shareId}")]
    [ApiOperationId("DeleteRegulationShare")]
    public async Task<IActionResult> DeleteRegulationShareAsync(int shareId) =>
        await DeleteAsync(shareId);


    #region Attributes

    /// <summary>
    /// Get a regulation share attribute
    /// </summary>
    /// <param name="shareId">The regulation share id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{shareId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetRegulationShareAttribute")]
    public virtual async Task<ActionResult<string>> GetRegulationShareAttributeAsync(
        int shareId, string attributeName) =>
        await GetAttributeAsync(shareId, attributeName);

    /// <summary>
    /// Set a regulation share attribute
    /// </summary>
    /// <param name="shareId">The regulation share id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{shareId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetRegulationShareAttribute")]
    public virtual async Task<ActionResult<string>> SetRegulationShareAttributeAsync(
        int shareId, string attributeName, [FromBody] string value)
    {
        return await SetAttributeAsync(shareId, attributeName, value);
    }

    /// <summary>
    /// Delete a regulation share attribute
    /// </summary>
    /// <param name="shareId">The regulation share id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{shareId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteRegulationShareAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteRegulationShareAttributeAsync(
        int shareId, string attributeName)
    {
        return await DeleteAttributeAsync(shareId, attributeName);
    }

    #endregion

}
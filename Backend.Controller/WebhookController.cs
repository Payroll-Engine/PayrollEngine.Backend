using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Webhooks")]
[Route("api/tenants/{tenantId}/webhooks")]
public class WebhookController : Api.Controller.WebhookController
{
    /// <inheritdoc/>
    public WebhookController(ITenantService tenantService, IWebhookService webhookService,
        IControllerRuntime runtime) :
        base(tenantService, webhookService, runtime)
    {
    }

    /// <summary>
    /// Query webhooks
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant webhooks</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryWebhooks")]
    public async Task<ActionResult> QueryWebhooksAsync(int tenantId,
        [FromQuery] Query query)
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
    /// Get a webhook
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the webhook</param>
    /// <returns></returns>
    [HttpGet("{webhookId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetWebhook")]
    public async Task<ActionResult<ApiObject.Webhook>> GetWebhookAsync(
        int tenantId, int webhookId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, webhookId);
    }

    /// <summary>
    /// Add a new webhook
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhook">The webhook to add</param>
    /// <returns>The newly created webhook</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateWebhook")]
    public async Task<ActionResult<ApiObject.Webhook>> CreateWebhookAsync(
        int tenantId, ApiObject.Webhook webhook)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(tenantId, webhook);
    }

    /// <summary>
    /// Update a webhook
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhook">The webhook with updated values</param>
    /// <returns>The modified webhook</returns>
    [HttpPut("{webhookId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateWebhook")]
    public async Task<ActionResult<ApiObject.Webhook>> UpdateWebhookAsync(
        int tenantId, ApiObject.Webhook webhook)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(tenantId, webhook);
    }

    /// <summary>
    /// Delete a webhook
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the webhook</param>
    /// <returns></returns>
    [HttpDelete("{webhookId}")]
    [ApiOperationId("DeleteWebhook")]
    public async Task<IActionResult> DeleteWebhookAsync(int tenantId, int webhookId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, webhookId);
    }

    #region Attributes

    /// <summary>
    /// Get a webhook attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the webhook</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{webhookId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetWebhookAttribute")]
    public virtual async Task<ActionResult<string>> GetWebhookAttributeAsync(
        int tenantId, int webhookId, string attributeName)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAttributeAsync(webhookId, attributeName);
    }

    /// <summary>
    /// Set a webhook attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the webhook</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{webhookId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetWebhookAttribute")]
    public virtual async Task<ActionResult<string>> SetWebhookAttributeAsync(
        int tenantId, int webhookId, string attributeName, [FromBody] string value)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await SetAttributeAsync(webhookId, attributeName, value);
    }

    /// <summary>
    /// Delete a webhook attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the webhook</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{webhookId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteWebhookAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteWebhookAttributeAsync(
        int tenantId, int webhookId, string attributeName)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAttributeAsync(webhookId, attributeName);
    }

    #endregion

}
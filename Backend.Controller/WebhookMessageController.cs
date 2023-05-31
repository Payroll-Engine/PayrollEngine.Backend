﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Webhook messages")]
[Route("api/tenants/{tenantId}/webhooks/{webhookId}/messages")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.WebhookMessage)]
public class WebhookMessageController : Api.Controller.WebhookMessageController
{
    /// <inheritdoc/>
    public WebhookMessageController(IWebhookService webhookService, IWebhookMessageService webhookMessageService,
        IControllerRuntime runtime) :
        base(webhookService, webhookMessageService, runtime)
    {
    }

    /// <summary>
    /// Query webhook messages
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The web hook id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The webhook messages</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryWebhookMessages")]
    public async Task<ActionResult> QueryWebhookMessagesAsync(int tenantId, int webhookId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(webhookId, query);
    }

    /// <summary>
    /// Get a webhook message
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The webhook id</param>
    /// <param name="webhookMessageId">The id of the webhook message</param>
    /// <returns></returns>
    [HttpGet("{webhookMessageId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetWebhookMessage")]
    public async Task<ActionResult<ApiObject.WebhookMessage>> GetWebhookMessageAsync(int tenantId, int webhookId, int webhookMessageId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(webhookId, webhookMessageId);
    }

    /// <summary>
    /// Add a new webhook messages
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The id of the web hook</param>
    /// <param name="webhookMessage">The webhook messages to add</param>
    /// <returns>The newly created webhook messages</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateWebhookMessage")]
    public async Task<ActionResult<ApiObject.WebhookMessage>> CreateWebhookMessageAsync(int tenantId,
        int webhookId, ApiObject.WebhookMessage webhookMessage)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(webhookId, webhookMessage);
    }

    /// <summary>
    /// Update a webhook messages
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The webhook id</param>
    /// <param name="webhookMessage">The webhook messages with updated values</param>
    /// <returns>The modified webhook messages</returns>
    [HttpPut("{webhookMessageId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateWebhookMessage")]
    public async Task<ActionResult<ApiObject.WebhookMessage>> UpdateWebhookMessageAsync(int tenantId, int webhookId,
        ApiObject.WebhookMessage webhookMessage)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(webhookId, webhookMessage);
    }

    /// <summary>
    /// Delete a webhook messages
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="webhookId">The webhook id</param>
    /// <param name="webhookMessageId">The id of the webhook messages</param>
    /// <returns></returns>
    [HttpDelete("{webhookMessageId}")]
    [ApiOperationId("DeleteWebhookMessage")]
    public async Task<IActionResult> DeleteWebhookMessageAsync(int tenantId, int webhookId, int webhookMessageId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(webhookId, webhookMessageId);
    }
}
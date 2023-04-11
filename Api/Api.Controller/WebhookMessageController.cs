using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the web hook messages
/// </summary>
[ApiControllerName("Webhook messages")]
[Route("api/tenants/{tenantId}/webhooks/{webhookId}/messages")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.WebhookMessage)]
public abstract class WebhookMessageController : RepositoryChildObjectController<IWebhookService, IWebhookMessageService,
    IWebhookRepository, IWebhookMessageRepository,
    DomainObject.Webhook, DomainObject.WebhookMessage, ApiObject.WebhookMessage>
{
    protected WebhookMessageController(IWebhookService webhookService, IWebhookMessageService webhookMessageService, IControllerRuntime runtime) :
        base(webhookService, webhookMessageService, runtime, new WebhookMessageMap())
    {
    }
}
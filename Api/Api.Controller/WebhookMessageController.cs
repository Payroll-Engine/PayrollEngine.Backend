using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the web hook messages
/// </summary>
public abstract class WebhookMessageController(IWebhookService webhookService,
        IWebhookMessageService webhookMessageService, IControllerRuntime runtime)
    : RepositoryChildObjectController<IWebhookService, IWebhookMessageService,
    IWebhookRepository, IWebhookMessageRepository,
    DomainObject.Webhook, DomainObject.WebhookMessage, ApiObject.WebhookMessage>(webhookService, webhookMessageService, runtime, new WebhookMessageMap());
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the web hooks
/// </summary>
[ApiControllerName("Webhooks")]
[Route("api/tenants/{tenantId}/webhooks")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Webhook)]
public abstract class WebhookController : RepositoryChildObjectController<ITenantService, IWebhookService,
    ITenantRepository, IWebhookRepository,
    DomainObject.Tenant, DomainObject.Webhook, ApiObject.Webhook>
{
    protected WebhookController(ITenantService tenantService, IWebhookService webhookService, IControllerRuntime runtime) :
        base(tenantService, webhookService, runtime, new WebhookMap())
    {
    }
}
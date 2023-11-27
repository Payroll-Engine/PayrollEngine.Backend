using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the web hooks
/// </summary>
public abstract class WebhookController(ITenantService tenantService, IWebhookService webhookService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IWebhookService,
    ITenantRepository, IWebhookRepository,
    DomainObject.Tenant, DomainObject.Webhook, ApiObject.Webhook>(tenantService, webhookService, runtime, new WebhookMap());
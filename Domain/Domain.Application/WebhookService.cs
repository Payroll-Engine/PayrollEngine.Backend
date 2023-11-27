using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WebhookService
    (IWebhookRepository repository) : ChildApplicationService<IWebhookRepository, Webhook>(repository), IWebhookService;
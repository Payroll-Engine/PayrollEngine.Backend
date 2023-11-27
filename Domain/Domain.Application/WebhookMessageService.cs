using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WebhookMessageService(IWebhookMessageRepository repository) :
    ChildApplicationService<IWebhookMessageRepository, WebhookMessage>(repository), IWebhookMessageService;
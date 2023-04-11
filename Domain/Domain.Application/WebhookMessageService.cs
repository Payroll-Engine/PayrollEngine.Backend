using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WebhookMessageService : ChildApplicationService<IWebhookMessageRepository, WebhookMessage>, IWebhookMessageService
{
    public WebhookMessageService(IWebhookMessageRepository repository) :
        base(repository)
    {
    }
}
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WebhookService : ChildApplicationService<IWebhookRepository, Webhook>, IWebhookService
{
    public WebhookService(IWebhookRepository repository) :
        base(repository)
    {
    }
}
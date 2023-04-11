using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseChangeService : CaseChangeService<ICompanyCaseChangeRepository, CaseChange>, ICompanyCaseChangeService
{
    public CompanyCaseChangeService(IWebhookDispatchService webhookDispatcher, ICompanyCaseChangeRepository companyCaseChangeRepository) :
        base(webhookDispatcher, companyCaseChangeRepository)
    {
    }
}
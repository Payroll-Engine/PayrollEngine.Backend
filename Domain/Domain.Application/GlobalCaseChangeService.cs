using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseChangeService : CaseChangeService<IGlobalCaseChangeRepository, CaseChange>, IGlobalCaseChangeService
{
    public GlobalCaseChangeService(IWebhookDispatchService webhookDispatcher, IGlobalCaseChangeRepository globalCaseChangeRepository) :
        base(webhookDispatcher, globalCaseChangeRepository)
    {
    }
}
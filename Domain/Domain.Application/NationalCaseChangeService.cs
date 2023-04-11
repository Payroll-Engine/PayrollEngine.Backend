using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseChangeService : CaseChangeService<INationalCaseChangeRepository, CaseChange>, INationalCaseChangeService
{
    public NationalCaseChangeService(IWebhookDispatchService webhookDispatcher, INationalCaseChangeRepository nationalCaseChangeRepository) :
        base(webhookDispatcher, nationalCaseChangeRepository)
    {
    }
}
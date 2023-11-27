using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseChangeService(IWebhookDispatchService webhookDispatcher,
        INationalCaseChangeRepository nationalCaseChangeRepository)
    : CaseChangeService<INationalCaseChangeRepository, CaseChange>(webhookDispatcher, nationalCaseChangeRepository),
        INationalCaseChangeService;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseChangeService(IWebhookDispatchService webhookDispatcher,
        IGlobalCaseChangeRepository globalCaseChangeRepository)
    : CaseChangeService<IGlobalCaseChangeRepository, CaseChange>(webhookDispatcher, globalCaseChangeRepository),
        IGlobalCaseChangeService;
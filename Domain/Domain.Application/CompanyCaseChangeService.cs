using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseChangeService(IWebhookDispatchService webhookDispatcher,
        ICompanyCaseChangeRepository companyCaseChangeRepository)
    : CaseChangeService<ICompanyCaseChangeRepository, CaseChange>(webhookDispatcher, companyCaseChangeRepository),
        ICompanyCaseChangeService;
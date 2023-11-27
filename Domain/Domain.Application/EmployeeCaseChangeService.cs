using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseChangeService(IWebhookDispatchService webhookDispatcher,
        IEmployeeCaseChangeRepository employeeCaseChangeRepository)
    : CaseChangeService<IEmployeeCaseChangeRepository, CaseChange>(webhookDispatcher, employeeCaseChangeRepository),
        IEmployeeCaseChangeService;
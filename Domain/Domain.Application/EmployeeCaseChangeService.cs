using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseChangeService : CaseChangeService<IEmployeeCaseChangeRepository, CaseChange>, IEmployeeCaseChangeService
{
    public EmployeeCaseChangeService(IWebhookDispatchService webhookDispatcher, IEmployeeCaseChangeRepository employeeCaseChangeRepository) :
        base(webhookDispatcher, employeeCaseChangeRepository)
    {
    }
}
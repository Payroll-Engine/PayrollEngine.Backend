using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseValueService : CaseValueService<IEmployeeCaseValueRepository>, IEmployeeCaseValueService
{
    public EmployeeCaseValueService(IWebhookDispatchService webhookDispatcher, ILookupSetRepository lookupSetRepository,
        IPayrollRepository payrollRepository,
        ICaseRepository caseRepository, IEmployeeCaseValueRepository employeeCaseValueRepository) :
        base(webhookDispatcher, lookupSetRepository, payrollRepository, caseRepository, employeeCaseValueRepository)
    {
    }
}
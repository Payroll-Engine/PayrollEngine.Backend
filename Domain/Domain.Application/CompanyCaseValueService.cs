using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseValueService : CaseValueService<ICompanyCaseValueRepository>, ICompanyCaseValueService
{
    public CompanyCaseValueService(IWebhookDispatchService webhookDispatcher, ILookupSetRepository lookupSetRepository,
        IPayrollRepository payrollRepository,
        ICaseRepository caseRepository, ICompanyCaseValueRepository companyCaseValueRepository) :
        base(webhookDispatcher, lookupSetRepository, payrollRepository, caseRepository, companyCaseValueRepository)
    {
    }
}
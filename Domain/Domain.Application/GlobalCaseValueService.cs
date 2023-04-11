using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseValueService : CaseValueService<IGlobalCaseValueRepository>, IGlobalCaseValueService
{
    public GlobalCaseValueService(IWebhookDispatchService webhookDispatcher, ILookupSetRepository lookupSetRepository,
        IPayrollRepository payrollRepository,
        ICaseRepository caseRepository, IGlobalCaseValueRepository globalCaseValueRepository) :
        base(webhookDispatcher, lookupSetRepository, payrollRepository, caseRepository, globalCaseValueRepository)
    {
    }
}
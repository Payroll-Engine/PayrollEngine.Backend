using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseValueService : CaseValueService<INationalCaseValueRepository>, INationalCaseValueService
{
    public NationalCaseValueService(IWebhookDispatchService webhookDispatcher, ILookupSetRepository lookupSetRepository,
        IPayrollRepository payrollRepository,
        ICaseRepository caseRepository, INationalCaseValueRepository nationalCaseValueRepository) :
        base(webhookDispatcher, lookupSetRepository, payrollRepository, caseRepository, nationalCaseValueRepository)
    {
    }
}
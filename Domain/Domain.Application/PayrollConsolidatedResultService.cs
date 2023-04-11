using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollConsolidatedResultService : ChildApplicationService<IPayrollConsolidatedResultRepository, PayrollResult>, IPayrollConsolidatedResultService
{
    public PayrollConsolidatedResultService(IPayrollResultContextService context) :
        base(context.ConsolidatedResultRepository)
    {
    }

    public virtual async Task<ConsolidatedPayrollResult> GetPayrollResultAsync(PayrollResultQuery query) =>
        await Repository.GetPayrollResultAsync(query);

    public virtual async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await Repository.GetCollectorResultsAsync(query);

    public virtual async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await Repository.GetCollectorCustomResultsAsync(query);

    public virtual async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await Repository.GetWageTypeResultsAsync(query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await Repository.GetWageTypeCustomResultsAsync(query);

    public virtual async Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(ConsolidatedPayrunResultQuery query) =>
        await Repository.GetPayrunResultsAsync(query);
}
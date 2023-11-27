using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollConsolidatedResultService(IPayrollResultContextService context) :
    ChildApplicationService<IPayrollConsolidatedResultRepository, PayrollResult>(context.ConsolidatedResultRepository),
    IPayrollConsolidatedResultService
{
    public async Task<ConsolidatedPayrollResult> GetPayrollResultAsync(IDbContext context, PayrollResultQuery query) =>
        await Repository.GetPayrollResultAsync(context, query);

    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await Repository.GetCollectorResultsAsync(context, query);

    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await Repository.GetCollectorCustomResultsAsync(context, query);

    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await Repository.GetWageTypeResultsAsync(context, query);

    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await Repository.GetWageTypeCustomResultsAsync(context, query);

    public async Task<IEnumerable<PayrunResult>> GetPayrunResultsAsync(IDbContext context, ConsolidatedPayrunResultQuery query) =>
        await Repository.GetPayrunResultsAsync(context, query);
}
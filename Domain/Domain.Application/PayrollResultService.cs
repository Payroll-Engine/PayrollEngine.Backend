using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollResultService : ChildApplicationService<IPayrollResultRepository, PayrollResult>, IPayrollResultService
{
    public ICollectorResultRepository CollectorResultRepository { get; }
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; }
    public IWageTypeResultRepository WageTypeResultRepository { get; }
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; }
    public IPayrunResultRepository PayrunResultRepository { get; }
    public IPayrollResultSetRepository ResultSetRepository { get; }
    public IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; }

    public PayrollResultService(IPayrollResultContextService context) :
        base(context.ResultRepository)
    {
        CollectorResultRepository = context.CollectorResultRepository ?? throw new ArgumentNullException(nameof(context.CollectorResultRepository));
        CollectorCustomResultRepository = context.CollectorCustomResultRepository ?? throw new ArgumentNullException(nameof(context.CollectorCustomResultRepository));
        WageTypeResultRepository = context.WageTypeResultRepository ?? throw new ArgumentNullException(nameof(context.WageTypeResultRepository));
        WageTypeCustomResultRepository = context.WageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(context.WageTypeCustomResultRepository));
        PayrunResultRepository = context.PayrunResultRepository ?? throw new ArgumentNullException(nameof(context.PayrunResultRepository));
        ResultSetRepository = context.ResultSetRepository ?? throw new ArgumentNullException(nameof(context.ResultSetRepository));
        ConsolidatedResultRepository = context.ConsolidatedResultRepository ?? throw new ArgumentNullException(nameof(context.ConsolidatedResultRepository));
    }

    public virtual async Task<PayrollResultSet> GetResultSetAsync(int tenantId, int resultId) =>
        await ResultSetRepository.GetAsync(tenantId, resultId);

    public virtual async Task<IEnumerable<CollectorResult>> QueryCollectorResultsAsync(int resultId, Query query = null) =>
        await CollectorResultRepository.QueryAsync(resultId, query);

    public virtual async Task<IEnumerable<CollectorCustomResult>> QueryCollectorCustomResultsAsync(int resultId, Query query = null) =>
        await CollectorCustomResultRepository.QueryAsync(resultId, query);

    public virtual async Task<IEnumerable<WageTypeResult>> QueryWageTypeResultsAsync(int resultId, Query query = null) =>
        await WageTypeResultRepository.QueryAsync(resultId, query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> QueryWageTypeCustomResultsAsync(int resultId, Query query = null) =>
        await WageTypeCustomResultRepository.QueryAsync(resultId, query);

    public virtual async Task<IEnumerable<PayrunResult>> QueryPayrunResultsAsync(int resultId, Query query = null) =>
        await PayrunResultRepository.QueryAsync(resultId, query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetWageTypeCustomResultsAsync(query, payrunJobId, parentPayrunJobId);

    public virtual async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetCollectorCustomResultsAsync(query, payrunJobId, parentPayrunJobId);

    public virtual async Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(int tenantId, int employeeId, Query query = null) =>
        await Repository.QueryResultValuesAsync(tenantId, employeeId, query);

    public virtual async Task<long> QueryResultValueCountAsync(int tenantId, int employeeId, Query query = null) =>
        await Repository.QueryResultValueCountAsync(tenantId, employeeId, query);

    public virtual async Task<IEnumerable<PayrollResultSet>> QueryResultSetsAsync(int tenantId, Query query = null) =>
        await ResultSetRepository.QueryAsync(tenantId, query);

    public virtual async Task<ConsolidatedPayrollResult> GetConsolidatedPayrollResultAsync(PayrollResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrollResultAsync(query);

    public virtual async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeResultsAsync(query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeCustomResultsAsync(query);

    public virtual async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorResultsAsync(query);

    public virtual async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorCustomResultsAsync(query);

    public virtual async Task<IEnumerable<PayrunResult>> GetConsolidatedPayrunResultsAsync(ConsolidatedPayrunResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrunResultsAsync(query);
}
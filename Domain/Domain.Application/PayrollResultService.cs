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

    public virtual async Task<PayrollResultSet> GetResultSetAsync(IDbContext context,int tenantId, int resultId) =>
        await ResultSetRepository.GetAsync(context, tenantId, resultId);

    public virtual async Task<IEnumerable<CollectorResult>> QueryCollectorResultsAsync(IDbContext context,int resultId, Query query = null) =>
        await CollectorResultRepository.QueryAsync(context, resultId, query);

    public virtual async Task<IEnumerable<CollectorCustomResult>> QueryCollectorCustomResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await CollectorCustomResultRepository.QueryAsync(context, resultId, query);

    public virtual async Task<IEnumerable<WageTypeResult>> QueryWageTypeResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await WageTypeResultRepository.QueryAsync(context, resultId, query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> QueryWageTypeCustomResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await WageTypeCustomResultRepository.QueryAsync(context, resultId, query);

    public virtual async Task<IEnumerable<PayrunResult>> QueryPayrunResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await PayrunResultRepository.QueryAsync(context, resultId, query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context,WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetWageTypeCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    public virtual async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context,CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetCollectorCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    public virtual async Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(IDbContext context,int tenantId, int employeeId, Query query = null) =>
        await Repository.QueryResultValuesAsync(context, tenantId, employeeId, query);

    public virtual async Task<long> QueryResultValueCountAsync(IDbContext context, int tenantId, int employeeId, Query query = null) =>
        await Repository.QueryResultValueCountAsync(context, tenantId, employeeId, query);

    public virtual async Task<IEnumerable<PayrollResultSet>> QueryResultSetsAsync(IDbContext context, int tenantId, Query query = null) =>
        await ResultSetRepository.QueryAsync(context, tenantId, query);

    public virtual async Task<ConsolidatedPayrollResult> GetConsolidatedPayrollResultAsync(IDbContext context,PayrollResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrollResultAsync(context, query);

    public virtual async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeResultsAsync(context, query);

    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeCustomResultsAsync(context, query);

    public virtual async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorResultsAsync(context, query);

    public virtual async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorCustomResultsAsync(context, query);

    public virtual async Task<IEnumerable<PayrunResult>> GetConsolidatedPayrunResultsAsync(IDbContext context, ConsolidatedPayrunResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrunResultsAsync(context, query);
}
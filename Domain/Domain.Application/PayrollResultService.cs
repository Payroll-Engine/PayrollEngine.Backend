using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollResultService(IPayrollResultContextService context) :
    ChildApplicationService<IPayrollResultRepository, PayrollResult>(context.ResultRepository), IPayrollResultService
{
    private ICollectorResultRepository CollectorResultRepository { get; } = context.CollectorResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.CollectorResultRepository));
    private ICollectorCustomResultRepository CollectorCustomResultRepository { get; } = context.CollectorCustomResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.CollectorCustomResultRepository));
    private IWageTypeResultRepository WageTypeResultRepository { get; } = context.WageTypeResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.WageTypeResultRepository));
    private IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; } = context.WageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.WageTypeCustomResultRepository));
    private IPayrunResultRepository PayrunResultRepository { get; } = context.PayrunResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.PayrunResultRepository));
    private IPayrollResultSetRepository ResultSetRepository { get; } = context.ResultSetRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.ResultSetRepository));
    private IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; } = context.ConsolidatedResultRepository ?? throw new ArgumentNullException(nameof(IPayrollResultContextService.ConsolidatedResultRepository));

    public async Task<PayrollResultSet> GetResultSetAsync(IDbContext context,int tenantId, int resultId) =>
        await ResultSetRepository.GetAsync(context, tenantId, resultId);

    public async Task<IEnumerable<CollectorResult>> QueryCollectorResultsAsync(IDbContext context,int resultId, Query query = null) =>
        await CollectorResultRepository.QueryAsync(context, resultId, query);

    public async Task<IEnumerable<CollectorCustomResult>> QueryCollectorCustomResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await CollectorCustomResultRepository.QueryAsync(context, resultId, query);

    public async Task<IEnumerable<WageTypeResult>> QueryWageTypeResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await WageTypeResultRepository.QueryAsync(context, resultId, query);

    public async Task<IEnumerable<WageTypeCustomResult>> QueryWageTypeCustomResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await WageTypeCustomResultRepository.QueryAsync(context, resultId, query);

    public async Task<IEnumerable<PayrunResult>> QueryPayrunResultsAsync(IDbContext context, int resultId, Query query = null) =>
        await PayrunResultRepository.QueryAsync(context, resultId, query);

    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context,WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetWageTypeCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context,CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await Repository.GetCollectorCustomResultsAsync(context, query, payrunJobId, parentPayrunJobId);

    public async Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(IDbContext context,int tenantId, int? employeeId = null, Query query = null) =>
        await Repository.QueryResultValuesAsync(context, tenantId, employeeId, query);

    public async Task<long> QueryResultValueCountAsync(IDbContext context, int tenantId, int? employeeId = null, Query query = null) =>
        await Repository.QueryResultValueCountAsync(context, tenantId, employeeId, query);

    public async Task<IEnumerable<PayrollResultSet>> QueryResultSetsAsync(IDbContext context, int tenantId, Query query = null) =>
        await ResultSetRepository.QueryAsync(context, tenantId, query);

    public async Task<ConsolidatedPayrollResult> GetConsolidatedPayrollResultAsync(IDbContext context,PayrollResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrollResultAsync(context, query);

    public async Task<IEnumerable<WageTypeResult>> GetConsolidatedWageTypeResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeResultsAsync(context, query);

    public async Task<IEnumerable<WageTypeCustomResult>> GetConsolidatedWageTypeCustomResultsAsync(IDbContext context, ConsolidatedWageTypeResultQuery query) =>
        await ConsolidatedResultRepository.GetWageTypeCustomResultsAsync(context, query);

    public async Task<IEnumerable<CollectorResult>> GetConsolidatedCollectorResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorResultsAsync(context, query);

    public async Task<IEnumerable<CollectorCustomResult>> GetConsolidatedCollectorCustomResultsAsync(IDbContext context, ConsolidatedCollectorResultQuery query) =>
        await ConsolidatedResultRepository.GetCollectorCustomResultsAsync(context, query);

    public async Task<IEnumerable<PayrunResult>> GetConsolidatedPayrunResultsAsync(IDbContext context, ConsolidatedPayrunResultQuery query) =>
        await ConsolidatedResultRepository.GetPayrunResultsAsync(context, query);
}
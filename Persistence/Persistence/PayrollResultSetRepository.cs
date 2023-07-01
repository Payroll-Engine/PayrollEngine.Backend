using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class PayrollResultSetRepository : ChildDomainRepository<PayrollResultSet>, IPayrollResultSetRepository
{
    private IWageTypeResultSetRepository WageTypeResultSetRepository { get; }
    private ICollectorResultSetRepository CollectorResultSetRepository { get; }
    private IPayrunResultRepository PayrunResultRepository { get; }
    private bool BulkInsert { get; }

    public PayrollResultSetRepository(IWageTypeResultSetRepository wageTypeResultSetRepository,
        ICollectorResultSetRepository collectorResultSetRepository,
        IPayrunResultRepository payrunResultRepository, bool bulkInsert) :
        base(DbSchema.Tables.PayrollResult, DbSchema.PayrollResultColumn.TenantId)
    {
        WageTypeResultSetRepository = wageTypeResultSetRepository ?? throw new ArgumentNullException(nameof(wageTypeResultSetRepository));
        CollectorResultSetRepository = collectorResultSetRepository ?? throw new ArgumentNullException(nameof(collectorResultSetRepository));
        PayrunResultRepository = payrunResultRepository ?? throw new ArgumentNullException(nameof(payrunResultRepository));
        BulkInsert = bulkInsert;
    }

    protected override void GetObjectCreateData(PayrollResultSet resultSet, DbParameterCollection parameters)
    {
        parameters.Add(nameof(resultSet.PayrollId), resultSet.PayrollId);
        parameters.Add(nameof(resultSet.PayrunId), resultSet.PayrunId);
        parameters.Add(nameof(resultSet.PayrunJobId), resultSet.PayrunJobId);
        parameters.Add(nameof(resultSet.EmployeeId), resultSet.EmployeeId);
        parameters.Add(nameof(resultSet.DivisionId), resultSet.DivisionId);
        parameters.Add(nameof(resultSet.CycleName), resultSet.CycleName);
        parameters.Add(nameof(resultSet.CycleStart), resultSet.CycleStart);
        parameters.Add(nameof(resultSet.CycleEnd), resultSet.CycleEnd);
        parameters.Add(nameof(resultSet.PeriodName), resultSet.PeriodName);
        parameters.Add(nameof(resultSet.PeriodStart), resultSet.PeriodStart);
        parameters.Add(nameof(resultSet.PeriodEnd), resultSet.PeriodEnd);
        base.GetObjectCreateData(resultSet, parameters);
    }

    protected override async Task OnRetrieved(IDbContext context, int tenantId, PayrollResultSet resultSet)
    {
        // wage type results
        resultSet.WageTypeResults = (await WageTypeResultSetRepository.QueryAsync(context, resultSet.Id)).ToList();

        // collector results
        resultSet.CollectorResults = (await CollectorResultSetRepository.QueryAsync(context, resultSet.Id)).ToList();

        // payrun results
        resultSet.PayrunResults = (await PayrunResultRepository.QueryAsync(context, resultSet.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(IDbContext context, int tenantId, PayrollResultSet resultSet)
    {
        // wage type results
        if (resultSet.WageTypeResults != null && resultSet.WageTypeResults.Any())
        {
            if (resultSet.WageTypeResults.Any(x => x.CustomResults.Count > 0))
            {
                // no bulk insert supported, new wage type result id is delegated to the child results
                await WageTypeResultSetRepository.CreateAsync(context, resultSet.Id, resultSet.WageTypeResults);
            }
            else
            {
                await WageTypeResultSetRepository.CreateBulkAsync(context, resultSet.Id, resultSet.WageTypeResults);
            }
        }

        // collector results
        if (resultSet.CollectorResults != null && resultSet.CollectorResults.Any())
        {
            if (resultSet.CollectorResults.Any(x => x.CustomResults.Count > 0))
            {
                // no bulk insert supported, new collector result id is delegated to the child results
                await CollectorResultSetRepository.CreateAsync(context, resultSet.Id, resultSet.CollectorResults);
            }
            else
            {
                await CollectorResultSetRepository.CreateBulkAsync(context, resultSet.Id, resultSet.CollectorResults);
            }
        }

        // payrun results
        if (resultSet.PayrunResults != null && resultSet.PayrunResults.Any())
        {
            if (BulkInsert)
            {
                await PayrunResultRepository.CreateBulkAsync(context, resultSet.Id, resultSet.PayrunResults);
            }
            else
            {
                await PayrunResultRepository.CreateAsync(context, resultSet.Id, resultSet.PayrunResults);
            }
        }

        await base.OnCreatedAsync(context, tenantId, resultSet);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int tenantId, PayrollResultSet payrollResultSet)
    {
        throw new NotSupportedException("Update of payroll results not supported");
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int payrollResultId)
    {
        // wage type results
        var wageTypeResults = (await WageTypeResultSetRepository.QueryAsync(context, payrollResultId)).ToList();
        foreach (var wageTypeResult in wageTypeResults)
        {
            await WageTypeResultSetRepository.DeleteAsync(context, payrollResultId, wageTypeResult.Id);
        }

        // collector results
        var collectorResults = (await CollectorResultSetRepository.QueryAsync(context, payrollResultId)).ToList();
        foreach (var collectorResult in collectorResults)
        {
            await CollectorResultSetRepository.DeleteAsync(context, payrollResultId, collectorResult.Id);
        }

        // payrun results
        var payrunResults = (await PayrunResultRepository.QueryAsync(context, payrollResultId)).ToList();
        foreach (var payrunResult in payrunResults)
        {
            await PayrunResultRepository.DeleteAsync(context, payrollResultId, payrunResult.Id);
        }

        return await base.OnDeletingAsync(context, payrollResultId);
    }
}
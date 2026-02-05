using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrollResultSetRepository(IWageTypeResultSetRepository wageTypeResultSetRepository,
        ICollectorResultSetRepository collectorResultSetRepository,
        IPayrunResultRepository payrunResultRepository, bool bulkInsert)
    : ChildDomainRepository<PayrollResultSet>(DbSchema.Tables.PayrollResult, DbSchema.PayrollResultColumn.TenantId),
        IPayrollResultSetRepository
{
    private IWageTypeResultSetRepository WageTypeResultSetRepository { get; } = wageTypeResultSetRepository ?? throw new ArgumentNullException(nameof(wageTypeResultSetRepository));
    private ICollectorResultSetRepository CollectorResultSetRepository { get; } = collectorResultSetRepository ?? throw new ArgumentNullException(nameof(collectorResultSetRepository));
    private IPayrunResultRepository PayrunResultRepository { get; } = payrunResultRepository ?? throw new ArgumentNullException(nameof(payrunResultRepository));
    private bool BulkInsert { get; } = bulkInsert;

    protected override void GetObjectCreateData(PayrollResultSet resultSet, DbParameterCollection parameters)
    {
        parameters.Add(nameof(resultSet.PayrollId), resultSet.PayrollId, DbType.Int32);
        parameters.Add(nameof(resultSet.PayrunId), resultSet.PayrunId, DbType.Int32);
        parameters.Add(nameof(resultSet.PayrunJobId), resultSet.PayrunJobId, DbType.Int32);
        parameters.Add(nameof(resultSet.EmployeeId), resultSet.EmployeeId, DbType.Int32);
        parameters.Add(nameof(resultSet.DivisionId), resultSet.DivisionId, DbType.Int32);
        parameters.Add(nameof(resultSet.CycleName), resultSet.CycleName);
        parameters.Add(nameof(resultSet.CycleStart), resultSet.CycleStart, DbType.DateTime2);
        parameters.Add(nameof(resultSet.CycleEnd), resultSet.CycleEnd, DbType.DateTime2);
        parameters.Add(nameof(resultSet.PeriodName), resultSet.PeriodName);
        parameters.Add(nameof(resultSet.PeriodStart), resultSet.PeriodStart, DbType.DateTime2);
        parameters.Add(nameof(resultSet.PeriodEnd), resultSet.PeriodEnd, DbType.DateTime2);
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
        throw new NotSupportedException("Update of payroll results not supported.");
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
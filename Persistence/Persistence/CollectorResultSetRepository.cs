using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CollectorResultSetRepository(ICollectorCustomResultRepository wageTypeCustomResultRepository,
        bool bulkInsert)
    : ChildDomainRepository<CollectorResultSet>(DbSchema.Tables.CollectorResult,
        DbSchema.CollectorResultColumn.PayrollResultId), ICollectorResultSetRepository
{
    private ICollectorCustomResultRepository CollectorCustomResultRepository { get; } = wageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(wageTypeCustomResultRepository));
    private bool BulkInsert { get; } = bulkInsert;

    protected override void GetObjectCreateData(CollectorResultSet result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.CollectorId), result.CollectorId, DbType.Int32);
        parameters.Add(nameof(result.CollectorName), result.CollectorName);
        parameters.Add(nameof(result.CollectorNameHash), result.CollectorNameHash, DbType.Int32);
        parameters.Add(nameof(result.CollectorNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.CollectorNameLocalizations));
        parameters.Add(nameof(result.CollectMode), result.CollectMode, DbType.Int32);
        parameters.Add(nameof(result.Negated), result.Negated, DbType.Boolean);
        parameters.Add(nameof(result.ValueType), result.ValueType, DbType.Int32);
        parameters.Add(nameof(result.Value), result.Value, DbType.Decimal);
        parameters.Add(nameof(result.Culture), result.Culture);
        parameters.Add(nameof(result.Start), result.Start, DbType.DateTime2);
        parameters.Add(nameof(result.StartHash), result.StartHash, DbType.Int32);
        parameters.Add(nameof(result.End), result.End, DbType.DateTime2);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override async Task OnRetrieved(IDbContext context, int payrollResultId, CollectorResultSet resultSet)
    {
        // custom collector results
        resultSet.CustomResults = (await CollectorCustomResultRepository.QueryAsync(context, resultSet.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(IDbContext context, int payrollResultId, CollectorResultSet resultSet)
    {
        // custom collector results
        if (resultSet.CustomResults != null && resultSet.CustomResults.Any())
        {
            if (BulkInsert)
            {
                await CollectorCustomResultRepository.CreateBulkAsync(context, resultSet.Id, resultSet.CustomResults);
            }
            else
            {
                await CollectorCustomResultRepository.CreateAsync(context, resultSet.Id, resultSet.CustomResults);
            }
        }

        await base.OnCreatedAsync(context, payrollResultId, resultSet);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int payrollResultId, CollectorResultSet resultSet)
    {
        throw new NotSupportedException("Update of custom collector results is not supported.");
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int wageTypeResultId)
    {
        // custom collector results
        var customResults = (await CollectorCustomResultRepository.QueryAsync(context, wageTypeResultId)).ToList();
        foreach (var customResult in customResults)
        {
            await CollectorCustomResultRepository.DeleteAsync(context, wageTypeResultId, customResult.Id);
        }
        return await base.OnDeletingAsync(context, wageTypeResultId);
    }
}
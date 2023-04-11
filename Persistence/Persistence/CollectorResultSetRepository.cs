using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class CollectorResultSetRepository : ChildDomainRepository<CollectorResultSet>, ICollectorResultSetRepository
{
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; }
    public bool BulkInsert { get; }

    public CollectorResultSetRepository(ICollectorCustomResultRepository wageTypeCustomResultRepository,
        bool bulkInsert, IDbContext context) :
        base(DbSchema.Tables.CollectorResult, DbSchema.CollectorResultColumn.PayrollResultId, context)
    {
        CollectorCustomResultRepository = wageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(wageTypeCustomResultRepository));
        BulkInsert = bulkInsert;
    }

    protected override void GetObjectCreateData(CollectorResultSet result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.CollectorId), result.CollectorId);
        parameters.Add(nameof(result.CollectorName), result.CollectorName);
        parameters.Add(nameof(result.CollectorNameHash), result.CollectorNameHash);
        parameters.Add(nameof(result.CollectorNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.CollectorNameLocalizations));
        parameters.Add(nameof(result.CollectType), result.CollectType);
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override async Task OnRetrieved(int payrollResultId, CollectorResultSet resultSet)
    {
        // custom collector results
        resultSet.CustomResults = (await CollectorCustomResultRepository.QueryAsync(resultSet.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(int payrollResultId, CollectorResultSet resultSet)
    {
        // custom collector results
        if (resultSet.CustomResults != null && resultSet.CustomResults.Any())
        {
            if (BulkInsert)
            {
                await CollectorCustomResultRepository.CreateBulkAsync(resultSet.Id, resultSet.CustomResults);
            }
            else
            {
                await CollectorCustomResultRepository.CreateAsync(resultSet.Id, resultSet.CustomResults);
            }
        }

        await base.OnCreatedAsync(payrollResultId, resultSet);
    }

    protected override Task OnUpdatedAsync(int payrollResultId, CollectorResultSet resultSet)
    {
        throw new NotSupportedException("Update of custom collector results is not supported");
    }

    protected override async Task<bool> OnDeletingAsync(int payrollResultId, int wageTypeResultId)
    {
        // custom collector results
        var customResults = (await CollectorCustomResultRepository.QueryAsync(wageTypeResultId)).ToList();
        foreach (var customResult in customResults)
        {
            await CollectorCustomResultRepository.DeleteAsync(wageTypeResultId, customResult.Id);
        }
        return await base.OnDeletingAsync(payrollResultId, wageTypeResultId);
    }
}
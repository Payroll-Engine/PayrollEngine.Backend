using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class WageTypeResultSetRepository(IWageTypeCustomResultRepository wageTypeCustomResultRepository,
        bool bulkInsert)
    : ChildDomainRepository<WageTypeResultSet>(DbSchema.Tables.WageTypeResult,
        DbSchema.WageTypeResultColumn.PayrollResultId), IWageTypeResultSetRepository
{
    private IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; } = wageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(wageTypeCustomResultRepository));
    private bool BulkInsert { get; } = bulkInsert;

    protected override void GetObjectCreateData(WageTypeResultSet result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.WageTypeId), result.WageTypeId);
        parameters.Add(nameof(result.WageTypeNumber), result.WageTypeNumber);
        parameters.Add(nameof(result.WageTypeName), result.WageTypeName);
        parameters.Add(nameof(result.WageTypeNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.WageTypeNameLocalizations));
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override async Task OnRetrieved(IDbContext context, int payrollResultId, WageTypeResultSet resultSet)
    {
        // wage type custom results
        resultSet.CustomResults = (await WageTypeCustomResultRepository.QueryAsync(context, resultSet.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(IDbContext context, int payrollResultId, WageTypeResultSet resultSet)
    {
        // wage type custom results
        if (resultSet.CustomResults != null && resultSet.CustomResults.Any())
        {
            if (BulkInsert)
            {
                await WageTypeCustomResultRepository.CreateBulkAsync(context, resultSet.Id, resultSet.CustomResults);
            }
            else
            {
                await WageTypeCustomResultRepository.CreateAsync(context, resultSet.Id, resultSet.CustomResults);
            }
        }
        await base.OnCreatedAsync(context, payrollResultId, resultSet);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int payrollResultId, WageTypeResultSet resultSet)
    {
        throw new NotSupportedException("Update of wage type results not supported");
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int wageTypeResultId)
    {
        // wage type custom results
        var customResults = (await WageTypeCustomResultRepository.QueryAsync(context, wageTypeResultId)).ToList();
        foreach (var customResult in customResults)
        {
            await WageTypeCustomResultRepository.DeleteAsync(context, wageTypeResultId, customResult.Id);
        }
        return await base.OnDeletingAsync(context, wageTypeResultId);
    }
}
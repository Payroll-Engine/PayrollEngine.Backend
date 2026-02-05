using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WageTypeResultSetRepository(IWageTypeCustomResultRepository wageTypeCustomResultRepository, bool bulkInsert)
    : ChildDomainRepository<WageTypeResultSet>(DbSchema.Tables.WageTypeResult,
        DbSchema.WageTypeResultColumn.PayrollResultId), IWageTypeResultSetRepository
{
    private IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; } =
        wageTypeCustomResultRepository ?? throw new ArgumentNullException(nameof(wageTypeCustomResultRepository));
    private bool BulkInsert { get; } = bulkInsert;

    protected override void GetObjectCreateData(WageTypeResultSet result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.WageTypeId), result.WageTypeId, DbType.Int32);
        parameters.Add(nameof(result.WageTypeNumber), result.WageTypeNumber, DbType.Decimal);
        parameters.Add(nameof(result.WageTypeName), result.WageTypeName);
        parameters.Add(nameof(result.WageTypeNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.WageTypeNameLocalizations));
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
        throw new NotSupportedException("Update of wage type results not supported.");
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
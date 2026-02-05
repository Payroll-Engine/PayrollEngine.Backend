using System;
using System.Data;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WageTypeCustomResultRepository() : ChildDomainRepository<WageTypeCustomResult>(
        DbSchema.Tables.WageTypeCustomResult, DbSchema.WageTypeCustomResultColumn.WageTypeResultId),
    IWageTypeCustomResultRepository
{
    protected override void GetObjectCreateData(WageTypeCustomResult result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.WageTypeNumber), result.WageTypeNumber, DbType.Decimal);
        parameters.Add(nameof(result.WageTypeName), result.WageTypeName);
        parameters.Add(nameof(result.WageTypeNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.WageTypeNameLocalizations));
        parameters.Add(nameof(result.Source), result.Source);
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

    protected override Task OnUpdatedAsync(IDbContext context, int wageTypeResultId, WageTypeCustomResult customResult)
    {
        throw new NotSupportedException("Update of wage type custom results is not supported.");
    }
}
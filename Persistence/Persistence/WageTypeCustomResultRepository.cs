using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class WageTypeCustomResultRepository() : ChildDomainRepository<WageTypeCustomResult>(
        DbSchema.Tables.WageTypeCustomResult, DbSchema.WageTypeCustomResultColumn.WageTypeResultId),
    IWageTypeCustomResultRepository
{
    protected override void GetObjectCreateData(WageTypeCustomResult result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.WageTypeNumber), result.WageTypeNumber);
        parameters.Add(nameof(result.WageTypeName), result.WageTypeName);
        parameters.Add(nameof(result.WageTypeNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.WageTypeNameLocalizations));
        parameters.Add(nameof(result.Source), result.Source);
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.Culture), result.Culture);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int wageTypeResultId, WageTypeCustomResult customResult)
    {
        throw new NotSupportedException("Update of wage type custom results is not supported.");
    }
}
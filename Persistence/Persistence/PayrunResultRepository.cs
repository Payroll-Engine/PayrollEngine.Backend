using System;
using System.Data;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrunResultRepository() : ChildDomainRepository<PayrunResult>(DbSchema.Tables.PayrunResult,
    DbSchema.PayrunResultColumn.PayrollResultId), IPayrunResultRepository
{
    protected override void GetObjectCreateData(PayrunResult result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.Source), result.Source);
        parameters.Add(nameof(result.Name), result.Name);
        parameters.Add(nameof(result.NameLocalizations), JsonSerializer.SerializeNamedDictionary(result.NameLocalizations));
        parameters.Add(nameof(result.Slot), result.Slot);
        parameters.Add(nameof(result.ValueType), result.ValueType, DbType.Int32);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.NumericValue), result.NumericValue, DbType.Decimal);
        parameters.Add(nameof(result.Culture), result.Culture);
        parameters.Add(nameof(result.Start), result.Start, DbType.DateTime2);
        parameters.Add(nameof(result.StartHash), result.StartHash, DbType.Int32);
        parameters.Add(nameof(result.End), result.End, DbType.DateTime2);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int parentId, PayrunResult payrunResult)
    {
        throw new NotSupportedException("Update of payrun results is not supported.");
    }
}
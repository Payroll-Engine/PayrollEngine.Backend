using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

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
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.NumericValue), result.NumericValue);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int parentId, PayrunResult payrunResult)
    {
        throw new NotSupportedException("Update of payrun results is not supported.");
    }
}
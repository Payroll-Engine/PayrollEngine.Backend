using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class CollectorCustomResultRepository() : ChildDomainRepository<CollectorCustomResult>(
        DbSchema.Tables.CollectorCustomResult, DbSchema.CollectorCustomResultColumn.CollectorResultId),
    ICollectorCustomResultRepository
{
    protected override void GetObjectCreateData(CollectorCustomResult result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.CollectorName), result.CollectorName);
        parameters.Add(nameof(result.CollectorNameHash), result.CollectorNameHash);
        parameters.Add(nameof(result.CollectorNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.CollectorNameLocalizations));
        parameters.Add(nameof(result.Source), result.Source);
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int collectorResultId, CollectorCustomResult customResult)
    {
        throw new NotSupportedException("Update of collector custom results is not supported");
    }
}
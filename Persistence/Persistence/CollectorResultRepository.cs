using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CollectorResultRepository : ChildDomainRepository<CollectorResult>, ICollectorResultRepository
{
    public CollectorResultRepository(IDbContext context) :
        base(DbSchema.Tables.CollectorResult, DbSchema.CollectorResultColumn.PayrollResultId, context)
    {
    }

    protected override void GetObjectCreateData(CollectorResult result, DbParameterCollection parameters)
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
}
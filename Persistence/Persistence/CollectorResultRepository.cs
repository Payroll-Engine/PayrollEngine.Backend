using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CollectorResultRepository() : ChildDomainRepository<CollectorResult>(DbSchema.Tables.CollectorResult,
    DbSchema.CollectorResultColumn.PayrollResultId), ICollectorResultRepository
{
    protected override void GetObjectCreateData(CollectorResult result, DbParameterCollection parameters)
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
}
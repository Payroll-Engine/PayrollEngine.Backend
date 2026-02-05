using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CollectorAuditRepository() : AuditChildDomainRepository<CollectorAudit>(DbSchema.Tables.CollectorAudit,
    DbSchema.CollectorAuditColumn.CollectorId), ICollectorAuditRepository
{
    protected override void GetObjectCreateData(CollectorAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.CollectorId), audit.CollectorId, DbType.Int32);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.CollectMode), audit.CollectMode, DbType.Int32);
        parameters.Add(nameof(audit.Negated), audit.Negated, DbType.Boolean);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.ValueType), audit.ValueType, DbType.Int32);
        parameters.Add(nameof(audit.Culture), audit.Culture);
        parameters.Add(nameof(audit.CollectorGroups), JsonSerializer.SerializeList(audit.CollectorGroups));
        parameters.Add(nameof(audit.Threshold), audit.Threshold, DbType.Decimal);
        parameters.Add(nameof(audit.MinResult), audit.MinResult, DbType.Decimal);
        parameters.Add(nameof(audit.MaxResult), audit.MaxResult, DbType.Decimal);
        parameters.Add(nameof(audit.StartExpression), audit.StartExpression);
        parameters.Add(nameof(audit.ApplyExpression), audit.ApplyExpression);
        parameters.Add(nameof(audit.EndExpression), audit.EndExpression);
        parameters.Add(nameof(audit.StartActions), JsonSerializer.SerializeList(audit.StartActions));
        parameters.Add(nameof(audit.ApplyActions), JsonSerializer.SerializeList(audit.ApplyActions));
        parameters.Add(nameof(audit.EndActions), JsonSerializer.SerializeList(audit.EndActions));
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash, DbType.Int32);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        base.GetObjectCreateData(audit, parameters);
    }
}
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CollectorRepository(IRegulationRepository regulationRepository,
    IScriptRepository scriptRepository, ICollectorAuditRepository auditRepository, bool auditDisabled)
    : ScriptTrackChildDomainRepository<Collector, CollectorAudit>(DbSchema.Tables.Collector,
        DbSchema.CollectorColumn.RegulationId, regulationRepository, scriptRepository, auditRepository, auditDisabled), ICollectorRepository
{
    protected override void GetObjectCreateData(Collector collector, DbParameterCollection parameters)
    {
        parameters.Add(nameof(collector.Name), collector.Name);
        parameters.Add(nameof(collector.CollectMode), collector.CollectMode, DbType.Int32);
        parameters.Add(nameof(collector.Negated), collector.Negated, DbType.Boolean);
        base.GetObjectCreateData(collector, parameters);
    }

    protected override void GetObjectData(Collector collector, DbParameterCollection parameters)
    {
        parameters.Add(nameof(collector.NameLocalizations), JsonSerializer.SerializeNamedDictionary(collector.NameLocalizations));
        parameters.Add(nameof(collector.OverrideType), collector.OverrideType, DbType.Int32);
        parameters.Add(nameof(collector.ValueType), collector.ValueType, DbType.Int32);
        parameters.Add(nameof(collector.Culture), collector.Culture);
        parameters.Add(nameof(collector.CollectorGroups), JsonSerializer.SerializeList(collector.CollectorGroups));
        parameters.Add(nameof(collector.Threshold), collector.Threshold, DbType.Decimal);
        parameters.Add(nameof(collector.MinResult), collector.MinResult, DbType.Decimal);
        parameters.Add(nameof(collector.MaxResult), collector.MaxResult, DbType.Decimal);
        parameters.Add(nameof(collector.StartExpression), collector.StartExpression);
        parameters.Add(nameof(collector.ApplyExpression), collector.ApplyExpression);
        parameters.Add(nameof(collector.EndExpression), collector.EndExpression);
        parameters.Add(nameof(collector.StartActions), JsonSerializer.SerializeList(collector.StartActions));
        parameters.Add(nameof(collector.ApplyActions), JsonSerializer.SerializeList(collector.ApplyActions));
        parameters.Add(nameof(collector.EndActions), JsonSerializer.SerializeList(collector.EndActions));
        parameters.Add(nameof(collector.Script), collector.Script);
        parameters.Add(nameof(collector.ScriptVersion), collector.ScriptVersion);
        parameters.Add(nameof(collector.Binary), collector.Binary, DbType.Binary);
        parameters.Add(nameof(collector.ScriptHash), collector.ScriptHash, DbType.Int32);
        parameters.Add(nameof(collector.Attributes), JsonSerializer.SerializeNamedDictionary(collector.Attributes));
        parameters.Add(nameof(collector.Clusters), JsonSerializer.SerializeList(collector.Clusters));
        base.GetObjectData(collector, parameters);
    }

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> collectorNames) =>
        await ExistsAnyAsync(context, DbSchema.CollectorColumn.RegulationId, regulationId, DbSchema.CollectorColumn.Name, collectorNames);
}
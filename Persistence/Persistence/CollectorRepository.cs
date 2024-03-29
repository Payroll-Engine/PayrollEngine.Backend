﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CollectorRepository(IScriptRepository scriptRepository, ICollectorAuditRepository auditRepository)
    : ScriptTrackChildDomainRepository<Collector, CollectorAudit>(DbSchema.Tables.Collector,
        DbSchema.CollectorColumn.RegulationId, scriptRepository, auditRepository), ICollectorRepository
{
    protected override void GetObjectCreateData(Collector collector, DbParameterCollection parameters)
    {
        parameters.Add(nameof(collector.Name), collector.Name);
        parameters.Add(nameof(collector.CollectMode), collector.CollectMode);
        parameters.Add(nameof(collector.Negated), collector.Negated);
        base.GetObjectCreateData(collector, parameters);
    }

    protected override void GetObjectData(Collector collector, DbParameterCollection parameters)
    {
        parameters.Add(nameof(collector.NameLocalizations), JsonSerializer.SerializeNamedDictionary(collector.NameLocalizations));
        parameters.Add(nameof(collector.OverrideType), collector.OverrideType);
        parameters.Add(nameof(collector.ValueType), collector.ValueType);
        parameters.Add(nameof(collector.CollectorGroups), JsonSerializer.SerializeList(collector.CollectorGroups));
        parameters.Add(nameof(collector.Threshold), collector.Threshold);
        parameters.Add(nameof(collector.MinResult), collector.MinResult);
        parameters.Add(nameof(collector.MaxResult), collector.MaxResult);
        parameters.Add(nameof(collector.StartExpression), collector.StartExpression);
        parameters.Add(nameof(collector.ApplyExpression), collector.ApplyExpression);
        parameters.Add(nameof(collector.EndExpression), collector.EndExpression);
        parameters.Add(nameof(collector.Script), collector.Script);
        parameters.Add(nameof(collector.ScriptVersion), collector.ScriptVersion);
        parameters.Add(nameof(collector.Binary), collector.Binary, DbType.Binary);
        parameters.Add(nameof(collector.ScriptHash), collector.ScriptHash);
        parameters.Add(nameof(collector.Attributes), JsonSerializer.SerializeNamedDictionary(collector.Attributes));
        parameters.Add(nameof(collector.Clusters), JsonSerializer.SerializeList(collector.Clusters));
        base.GetObjectData(collector, parameters);
    }

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> collectorNames) =>
        await ExistsAnyAsync(context, DbSchema.CollectorColumn.RegulationId, regulationId, DbSchema.CollectorColumn.Name, collectorNames);
}
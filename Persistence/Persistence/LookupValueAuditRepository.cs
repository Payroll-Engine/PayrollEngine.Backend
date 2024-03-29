﻿using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class LookupValueAuditRepository() : AuditChildDomainRepository<LookupValueAudit>(
    DbSchema.Tables.LookupValueAudit, DbSchema.LookupValueAuditColumn.LookupValueId), ILookupValueAuditRepository
{
    protected override void GetObjectCreateData(LookupValueAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.LookupValueId), audit.LookupValueId);
        parameters.Add(nameof(audit.Key), audit.Key);
        parameters.Add(nameof(audit.KeyHash), audit.KeyHash);
        parameters.Add(nameof(audit.RangeValue), audit.RangeValue);
        parameters.Add(nameof(audit.Value), audit.Value);
        parameters.Add(nameof(audit.ValueLocalizations), JsonSerializer.SerializeNamedDictionary(audit.ValueLocalizations));
        parameters.Add(nameof(audit.LookupHash), audit.LookupHash);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        base.GetObjectCreateData(audit, parameters);
    }
}
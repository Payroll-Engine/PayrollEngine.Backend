using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class LookupValueAuditRepository() : AuditChildDomainRepository<LookupValueAudit>(
    Tables.LookupValueAudit, LookupValueAuditColumn.LookupValueId), ILookupValueAuditRepository
{
    protected override void GetObjectCreateData(LookupValueAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.LookupValueId), audit.LookupValueId, DbType.Int32);
        parameters.Add(nameof(audit.Key), audit.Key);
        parameters.Add(nameof(audit.KeyHash), audit.KeyHash, DbType.Int32);
        parameters.Add(nameof(audit.RangeValue), audit.RangeValue, DbType.Decimal);
        parameters.Add(nameof(audit.Value), audit.Value);
        parameters.Add(nameof(audit.ValueLocalizations), JsonSerializer.SerializeNamedDictionary(audit.ValueLocalizations));
        parameters.Add(nameof(audit.LookupHash), audit.LookupHash, DbType.Int32);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        base.GetObjectCreateData(audit, parameters);
    }
}
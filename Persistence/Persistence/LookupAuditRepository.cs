using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupAuditRepository() : AuditChildDomainRepository<LookupAudit>(DbSchema.Tables.LookupAudit,
    DbSchema.LookupAudit.LookupId), ILookupAuditRepository
{
    protected override void GetObjectCreateData(LookupAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.LookupId), audit.LookupId, DbType.Int32);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.RangeMode), audit.RangeMode, DbType.Int32);
        parameters.Add(nameof(audit.RangeSize), audit.RangeSize, DbType.Decimal);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        base.GetObjectCreateData(audit, parameters);
    }
}
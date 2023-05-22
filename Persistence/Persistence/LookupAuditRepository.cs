using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class LookupAuditRepository : AuditChildDomainRepository<LookupAudit>, ILookupAuditRepository
{
    public LookupAuditRepository() :
        base(DbSchema.Tables.LookupAudit, DbSchema.LookupAudit.LookupId)
    {
    }

    protected override void GetObjectCreateData(LookupAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.LookupId), audit.LookupId);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        parameters.Add(nameof(audit.RangeSize), audit.RangeSize);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        base.GetObjectCreateData(audit, parameters);
    }
}
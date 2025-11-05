using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class LookupRepositoryBase<T>(ILookupAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<T, LookupAudit>(DbSchema.Tables.Lookup, DbSchema.LookupColumn.RegulationId,
        auditRepository, auditDisabled)
    where T : Lookup, new()
{
    protected override void GetObjectCreateData(T lookup, DbParameterCollection parameters)
    {
        parameters.Add(nameof(lookup.Name), lookup.Name);
        base.GetObjectCreateData(lookup, parameters);
    }

    protected override void GetObjectData(T lookup, DbParameterCollection parameters)
    {
        parameters.Add(nameof(lookup.NameLocalizations), JsonSerializer.SerializeNamedDictionary(lookup.NameLocalizations));
        parameters.Add(nameof(lookup.Description), lookup.Description);
        parameters.Add(nameof(lookup.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(lookup.DescriptionLocalizations));
        parameters.Add(nameof(lookup.OverrideType), lookup.OverrideType);
        parameters.Add(nameof(lookup.RangeMode), lookup.RangeMode);
        parameters.Add(nameof(lookup.RangeSize), lookup.RangeSize);
        parameters.Add(nameof(lookup.Attributes), JsonSerializer.SerializeNamedDictionary(lookup.Attributes));
        base.GetObjectData(lookup, parameters);
    }

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> lookupNames) =>
        await ExistsAnyAsync(context, DbSchema.LookupColumn.RegulationId, regulationId, DbSchema.LookupColumn.Name, lookupNames);
}
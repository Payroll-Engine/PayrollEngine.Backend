﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public abstract class LookupRepositoryBase<T> : TrackChildDomainRepository<T, LookupAudit>
    where T : Lookup, new()
{
    protected LookupRepositoryBase(ILookupAuditRepository auditRepository, IDbContext context) :
        base(DbSchema.Tables.Lookup, DbSchema.LookupColumn.RegulationId, auditRepository, context)
    {
    }

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
        parameters.Add(nameof(lookup.RangeSize), lookup.RangeSize);
        base.GetObjectData(lookup, parameters);
    }

    public virtual async Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> lookupNames) =>
        await ExistsAnyAsync(DbSchema.LookupColumn.RegulationId, regulationId, DbSchema.LookupColumn.Name, lookupNames);
}
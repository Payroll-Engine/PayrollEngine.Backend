using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class LookupRepositoryBase<T>(IRegulationRepository regulationRepository,
    ILookupAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<T, LookupAudit>(regulationRepository, DbSchema.Tables.Lookup,
        DbSchema.LookupColumn.RegulationId, auditRepository, auditDisabled)
    where T : Lookup, INamespaceObject, new()
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
        parameters.Add(nameof(lookup.OverrideType), lookup.OverrideType, DbType.Int32);
        parameters.Add(nameof(lookup.RangeMode), lookup.RangeMode, DbType.Int32);
        parameters.Add(nameof(lookup.RangeSize), lookup.RangeSize, DbType.Decimal);
        parameters.Add(nameof(lookup.Attributes), JsonSerializer.SerializeNamedDictionary(lookup.Attributes));
        base.GetObjectData(lookup, parameters);
    }

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> lookupNames) =>
        await ExistsAnyAsync(context, DbSchema.LookupColumn.RegulationId, regulationId, DbSchema.LookupColumn.Name, lookupNames);

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<bool> DeleteAsync(IDbContext context, int regulationId, int lookupId)
    {
        // tenant
        var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId);
        if (!tenantId.HasValue)
        {
            throw new PayrollException($"Missing tenant of regulation {regulationId}.");
        }

        // stored procedure parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterDeleteLookup.TenantId, tenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterDeleteLookup.LookupId, lookupId, DbType.Int32);
        parameters.Add("@sp_return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        try
        {
            // delete employee (stored procedure)
            await QueryAsync<Tenant>(context, DbSchema.Procedures.DeleteLookup,
                parameters, commandType: CommandType.StoredProcedure);

            // stored procedure return value
            var result = parameters.Get<int>("@sp_return");
            return result == 1;
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return false;
        }
    }

}
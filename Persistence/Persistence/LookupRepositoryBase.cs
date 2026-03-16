using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;
using LookupAudit = PayrollEngine.Domain.Model.LookupAudit;

namespace PayrollEngine.Persistence;

public abstract class LookupRepositoryBase<T>(IRegulationRepository regulationRepository,
    ILookupAuditRepository auditRepository, bool auditEnabled) :
    TrackChildDomainRepository<T, LookupAudit>(regulationRepository, Tables.Lookup,
        LookupColumn.RegulationId, auditRepository, auditEnabled)
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
        await ExistsAnyAsync(context, LookupColumn.RegulationId, regulationId, LookupColumn.Name, lookupNames);

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
        parameters.Add(ParameterDeleteLookup.TenantId, tenantId, DbType.Int32);
        parameters.Add(ParameterDeleteLookup.LookupId, lookupId, DbType.Int32);
        if (context.StoredProcedureReturnValue)
        {
            parameters.Add("@sp_return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        }

        try
        {
            // delete lookup (stored procedure)
            await QueryAsync<Tenant>(context, Procedures.DeleteLookup,
                parameters, commandType: CommandType.StoredProcedure);

            // stored procedure return value
            if (context.StoredProcedureReturnValue)
            {
                var result = parameters.Get<int>("@sp_return");
                return result == 1;
            }
            return true;
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return false;
        }
    }

}
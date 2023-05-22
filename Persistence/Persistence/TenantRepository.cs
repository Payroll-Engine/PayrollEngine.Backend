using System;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class TenantRepository : RootDomainRepository<Tenant>, ITenantRepository
{
    public TenantRepository() :
        base(DbSchema.Tables.Tenant)
    {
    }

    public virtual async Task<bool> ExistsAsync(IDbContext context, string identifier) =>
        await ExistsAsync(context, DbSchema.TenantColumn.Identifier, identifier);

    protected override void GetObjectCreateData(Tenant tenant, DbParameterCollection parameters)
    {
        parameters.Add(nameof(tenant.Identifier), tenant.Identifier);
        base.GetObjectCreateData(tenant, parameters);
    }

    protected override void GetObjectData(Tenant tenant, DbParameterCollection parameters)
    {
        parameters.Add(nameof(tenant.Culture), tenant.Culture);
        parameters.Add(nameof(tenant.Calendar), tenant.Calendar);
        parameters.Add(nameof(tenant.Attributes), JsonSerializer.SerializeNamedDictionary(tenant.Attributes));
        base.GetObjectData(tenant, parameters);
    }

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<bool> DeleteAsync(IDbContext context, int tenantId)
    {
        if (!await ExistsAsync(context, tenantId))
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}");
        }

        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterDeleteTenant.TenantId, tenantId);

        try
        {
            // delete tenant (stored procedure)
            // TODO: stored procedure result
            await QueryAsync<Tenant>(context, DbSchema.Procedures.DeleteTenant,
                parameters, commandType: CommandType.StoredProcedure);
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return false;
        }

        return true;
    }
}
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class TenantRepository : RootDomainRepository<Tenant>, ITenantRepository
{
    public TenantRepository(IDbContext context) :
        base(DbSchema.Tables.Tenant, context)
    {
    }

    public virtual async Task<bool> ExistsAsync(string identifier) =>
        await ExistsAsync(DbSchema.TenantColumn.Identifier, identifier);

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

    public override async Task<bool> DeleteAsync(int tenantId)
    {
        if (!await ExistsAsync(tenantId))
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}");
        }

        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterDeleteTenant.TenantId, tenantId);

        // delete tenant (stored procedure)
        // TODO: stored procedure result
        await QueryAsync<Tenant>(DbSchema.Procedures.DeleteTenant,
            parameters, commandType: CommandType.StoredProcedure);

        return true;
    }
}